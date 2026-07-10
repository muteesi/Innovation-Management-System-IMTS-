# IMTS Architecture Decisions (Phase 2)

> **Status:** Approved  
> **Applies to:** Phase 3+ implementation  
> **Frontend:** Phase 1.5 Razor Pages app communicates only through the API Gateway — never calls a service or RabbitMQ directly.

---

## 1. Service Boundaries

Seven independently deployable ASP.NET Core Web API services, each owning its own database.

| Service | Bounded Context | Primary Responsibility |
|---------|----------------|----------------------|
| Identity Service | Authentication & Authorization | User management, JWT issuance, role-based access |
| Ideas Service | Innovation Management | Idea CRUD, categories, comments, status tracking |
| Workflow Service | Process Management | Stage transitions, timelines, approvals |
| Funding Service | Resource Allocation | Budget tracking, resource requests, approvals |
| Documents Service | Document Management | File storage, versioning, metadata |
| Notifications Service | Communication | Email, in-app notifications, templates — the only path through which async events reach the frontend |
| Reporting Service | Analytics & Reporting | KPI calculations, data aggregation, report generation |

### Non-Services (explicitly excluded)

- **Audit Logging is NOT a separate service.** It is a cross-cutting concern: every service that publishes a domain event also writes its own audit log entry locally (or publishes a lightweight `*.Audited` companion event) at publish time. There is no standalone "Audit service" and no service should list "Audit" as a subscriber.
- **Authorization is NOT a subscriber.** Per the JWT flow (Section 5), role changes take effect via token claims on next login/token refresh — no service needs to subscribe to role changes to enforce authorization in real time.

---

## 2. Database Per Service

One shared MS SQL Server instance with multiple databases. No service reads or writes another service's database directly.

| Service | Database Name | Owned Tables |
|---------|--------------|--------------|
| Identity Service | `IdentityDb` | Users, Roles, Permissions, UserRoles |
| Ideas Service | `IdeasDb` | Ideas, Categories, Comments, Attachments |
| Workflow Service | `WorkflowDb` | Stages, Statuses, Timelines, Approvals |
| Funding Service | `FundingDb` | Budgets, Requests, Allocations |
| Documents Service | `DocumentsDb` | Resources, Versions, Metadata |
| Notifications Service | `NotificationsDb` | Notifications, Templates, Emails |
| Reporting Service | `ReportingDb` | Read Models, KPIs, Aggregations |

**Rationale:** One shared instance provides logical isolation without the management overhead of separate instances. Revisit only if a specific service's load justifies its own instance.

---

## 3. Communication Patterns

### 3.1 Synchronous (REST via Gateway)

All request-response calls from the Razor Pages frontend go **through the Gateway** — never directly to a service.

| Interaction | Endpoint | Service |
|-------------|----------|---------|
| User Login/Logout | `POST /api/v1/auth/login` | Identity |
| Submit Idea | `POST /api/v1/ideas` | Ideas |
| Fetch Ideas List | `GET /api/v1/ideas` | Ideas |
| Fetch Idea Detail | `GET /api/v1/ideas/{id}` | Ideas |
| Update Idea | `PATCH /api/v1/ideas/{id}` | Ideas |
| Change Stage/Status | `POST /api/v1/workflow/ideas/{id}/transition` | Workflow |
| Add Comment | `POST /api/v1/ideas/{id}/comments` | Ideas |
| User Management (Admin) | `GET/POST/PATCH /api/v1/users` | Identity |
| Category Management | `GET/POST/PUT /api/v1/categories` | Ideas |
| Poll Notifications | `GET /api/v1/notifications` | Notifications |

### REST API Decision Summary

| Decision Area | Choice | Rationale |
|--------------|--------|-----------|
| API Style | RESTful | Standard, simple |
| Versioning | URL Path (`v1`, `v2`) | Clear, explicit |
| Serialization | `System.Text.Json` only | Default in .NET, avoid mixing with Newtonsoft to prevent boundary mismatches |
| Error Handling | Consistent `ApiResponse` envelope | Predictable client handling |
| Status Codes | HTTP standards | RESTful |
| Authentication | JWT (Bearer token) | Stateless, Gateway-validated |
| Authorization | Role + claim based | Flexible, enforced per-endpoint |
| Caching | Response + memory cache | Performance |
| Compression | Gzip | Bandwidth |
| Rate Limiting | Fixed window (revisit to sliding window if burst spikes appear) | Protection |
| Documentation | OpenAPI/Swagger | Developer UX |
| Retry Policy | Polly, 3 retries, exponential backoff with jitter (2s, 4s, 8s) | Resilience without retry storms |
| Circuit Breaker | Polly, opens after 5 consecutive failures, stays open 30s before half-open trial | Resilience |
| Timeout (interactive) | 5 seconds | User experience — idea submission, review actions, login |
| Timeout (background) | 30 seconds | Long-running aggregation/report generation only |

### 3.2 Standardized API Response Model

Every endpoint returns a consistent envelope:

```json
{
  "success": true,
  "data": { /* payload */ },
  "error": null,
  "meta": {
    "requestId": "guid",
    "timestamp": "2026-07-10T12:00:00Z",
    "version": "v1"
  }
}
```

**Error response (`success: false`):**

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Human-readable description",
    "details": [
      { "field": "Title", "message": "Title is required" }
    ]
  },
  "meta": { "requestId": "guid", "timestamp": "2026-07-10T12:00:00Z", "version": "v1" }
}
```

Error codes: `VALIDATION_ERROR`, `NOT_FOUND`, `UNAUTHORIZED`, `FORBIDDEN`, `CONFLICT`, `INTERNAL_ERROR`, `SERVICE_UNAVAILABLE`.

### 3.3 Asynchronous (RabbitMQ)

For cross-service events that should not block the user. **The Razor Pages frontend is never a subscriber** — anything the UI needs to see arrives via the Notifications Service and the Gateway.

- **Exchange:** `imts-exchange` (topic exchange)
- **Queues:** One durable queue per subscribing service: `imts-notifications`, `imts-reporting`, `imts-workflow`, `imts-funding`, `imts-ideas`

### Event Catalog

| Event Name | Publisher | Subscribers |
|-----------|-----------|-------------|
| `IdeaSubmittedEvent` | Ideas | Notifications, Reporting, Workflow |
| `IdeaUpdatedEvent` | Ideas | Reporting |
| `IdeaStatusChangedEvent` | Workflow | Notifications, Reporting, Ideas |
| `IdeaStageChangedEvent` | Workflow | Notifications, Reporting, Ideas |
| `IdeaApprovedEvent` | Workflow | Notifications, Funding, Reporting |
| `IdeaRejectedEvent` | Workflow | Notifications, Reporting |
| `UserCreatedEvent` | Identity | Notifications |
| `UserRoleChangedEvent` | Identity | Notifications (informational only — authorization enforced via JWT claims, not this event) |
| `DocumentUploadedEvent` | Documents | Notifications, Reporting |
| `FundingRequestedEvent` | Funding | Notifications, Workflow, Reporting |
| `KPIUpdatedEvent` | Reporting | Notifications (Notifications then makes updated KPIs available to the frontend via the Gateway) |
| `TimelineEvent` (deadline) | Workflow | Notifications |

**Note:** Every publisher writes its own audit entry at publish time — audit logging is a side effect inside the publisher, not a subscriber.

**Note on Funding ↔ Workflow:** Funding reads Workflow synchronously (`GET /api/workflow/...` for approval status) **and** Workflow subscribes to `FundingRequestedEvent`. This is an intentional two-way dependency — flag it during deployment planning since circular dependencies affect startup order.

---

## 4. API Gateway Design

**Choice: YARP (Yet Another Reverse Proxy) with a Backend-for-Frontend (BFF) shape** — routes structured around what the Razor Pages UI needs, not a raw 1:1 passthrough.

**Why YARP:** Native ASP.NET Core, no extra dependencies, integrates with the existing .NET ecosystem, can run under IIS.

**Routing Configuration (placeholder hostnames):**

```json
{
  "ReverseProxy": {
    "Routes": {
      "identity-route": {
        "ClusterId": "identity-cluster",
        "Match": { "Path": "/api/identity/{**catch-all}" }
      },
      "ideas-route": {
        "ClusterId": "ideas-cluster",
        "Match": { "Path": "/api/ideas/{**catch-all}" }
      },
      "workflow-route": {
        "ClusterId": "workflow-cluster",
        "Match": { "Path": "/api/workflow/{**catch-all}" }
      },
      "funding-route": {
        "ClusterId": "funding-cluster",
        "Match": { "Path": "/api/funding/{**catch-all}" }
      },
      "documents-route": {
        "ClusterId": "documents-cluster",
        "Match": { "Path": "/api/documents/{**catch-all}" }
      },
      "notifications-route": {
        "ClusterId": "notifications-cluster",
        "Match": { "Path": "/api/notifications/{**catch-all}" }
      },
      "reporting-route": {
        "ClusterId": "reporting-cluster",
        "Match": { "Path": "/api/reporting/{**catch-all}" }
      }
    },
    "Clusters": {
      "identity-cluster": { "Destinations": { "dest": { "Address": "https://__IDENTITY_HOST__/" } } },
      "ideas-cluster": { "Destinations": { "dest": { "Address": "https://__IDEAS_HOST__/" } } },
      "workflow-cluster": { "Destinations": { "dest": { "Address": "https://__WORKFLOW_HOST__/" } } },
      "funding-cluster": { "Destinations": { "dest": { "Address": "https://__FUNDING_HOST__/" } } },
      "documents-cluster": { "Destinations": { "dest": { "Address": "https://__DOCUMENTS_HOST__/" } } },
      "notifications-cluster": { "Destinations": { "dest": { "Address": "https://__NOTIFICATIONS_HOST__/" } } },
      "reporting-cluster": { "Destinations": { "dest": { "Address": "https://__REPORTING_HOST__/" } } }
    }
  }
}
```

> ⚠️ Hostnames are placeholders (`__SERVICE_HOST__`) — deployment mode (Windows Services vs. containers) is unresolved. Do not hardcode container-style hostnames until that decision is made.

Gateway listens on a single external port and routes internally to each service.

---

## 5. JWT Flow & Security

**Decision: Gateway validates the JWT; downstream services trust the gateway and do not re-validate.**

1. User logs in → Gateway → Identity Service → issues JWT.
2. Razor Pages app sends the JWT on every subsequent request: `Authorization: Bearer {jwt_token}`.
3. YARP Gateway validates: signature, issuer/audience, expiry, and extracts role/claims.
4. Gateway forwards the request (plus the validated JWT) to the downstream service.
5. Downstream service does **not** re-validate the token — it reads the user/claims from `HttpContext.User` and enforces role-based access on top of that.

**Rationale:** Single validation point reduces latency and avoids duplicated validation logic across 7 services; centralization ensures uniform enforcement; downstream services focus purely on business logic; all services sit inside the BOU internal network, which is the trust boundary this model relies on.

---

## 6. Data Ownership & Cross-Service Access

**Rule: No service reaches into another service's database directly.** All cross-service reads go through synchronous API calls (via the Gateway or internal service-to-service calls) or through consumed events.

| Service | Owns | Reads From |
|---------|------|-----------|
| Identity Service | Users, Roles, Permissions | Nothing (own DB only) |
| Ideas Service | Ideas, Categories, Comments, Attachments | Nothing (own DB only) |
| Workflow Service | Stages, Statuses, Timelines, Approvals | Ideas Service (API, for idea context) |
| Funding Service | Budgets, Requests, Allocations | Workflow Service (API, for approval status) |
| Documents Service | Resources, Versions, Metadata | Ideas Service (API, for idea context) |
| Notifications Service | Notifications, Templates, Emails | Identity Service (API, for user contact info) |
| Reporting Service | Read Models, KPIs, Aggregations | **Events only**, from ALL services — including `FundingRequestedEvent` |

**Cross-Service Read Patterns:**
- **Sync API calls:** Workflow → Ideas (`GET /api/ideas/{id}`); Funding → Workflow (approval status)
- **Async events:** Reporting builds all of its read models purely from consumed events — never queries another service's database
- **Gateway/BFF aggregation:** Dashboard views needing data from multiple services are composed at the Gateway layer

---

## 7. Open Questions

| Question | Context | Action |
|----------|---------|--------|
| Deployment mode | Windows Services vs. containers — blocks finalizing Gateway hostnames | Resolve before Phase 3 starts; document the decision and update YARP config placeholders |
| Funding Service scope | Internal-only vs. external finance system integration | Resolve before building Funding Service in Phase 4 |
| Funding ↔ Workflow circular dependency | Funding calls Workflow synchronously; Workflow subscribes to Funding's events | Acceptable as designed; document deployment/startup order to account for it |

---

*This document is the foundation for all Phase 3+ implementation decisions. Do not proceed to Phase 3 until Phase 2 decisions are approved and the deployment-mode open question is resolved.*
