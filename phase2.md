# IMTS Phase 2 — Microservices Architecture Decisions

## Context & Objective

You are working on the **Bank of Uganda Innovation Management System (IMTS)**. The frontend (Razor Pages) has been built with stub data in **Phase 1.5** (per the IMTS agent prompt). Now you must lock down the **backend architecture before writing any services**.

**The system will be implemented as microservices**, not a monolith. Each service is independently deployable with its own database. The Razor Pages app communicates **only through the API Gateway** — it never calls a service directly, and it never consumes RabbitMQ events directly. Any event-driven update that needs to reach the UI (e.g. a timeline deadline notice, a KPI refresh) must be surfaced through the Notifications Service and exposed to the frontend via the Gateway (API polling or SignalR), not via a direct queue subscription from Razor Pages.

## Phase 2 Deliverables

By the end of this phase, produce **one markdown document** (`/docs/architecture-decisions.md`) containing:

1. **Service Boundaries** — confirmed list of services with responsibilities
2. **Database Per Service** — each service's database name and owned tables
3. **Communication Patterns** — sync (REST) vs. async (RabbitMQ) decision matrix
4. **API Gateway Design** — YARP configuration with routing rules
5. **JWT Flow** — gateway validates; downstream services trust it (with justification)
6. **Data Ownership** — per-service ownership with cross-service access rules
7. **REST API Contract** — standardized request/response models, error handling, versioning

---

## Architecture Decisions (Must Provide Rationale)

### 1. Service Boundaries

Confirm or adjust these service boundaries. Each is a **deployable ASP.NET Core Web API** with its own database. There are **7 deployable services** — nothing outside this list is a separate deployable.

| Service | Bounded Context | Primary Responsibility |
|---------|-----------------|------------------------|
| Identity Service | Authentication & Authorization | User management, JWT issuance, role-based access |
| Ideas Service | Innovation Management | Idea CRUD, categories, comments, status tracking |
| Workflow Service | Process Management | Stage transitions, timelines, approvals |
| Funding Service | Resource Allocation | Budget tracking, resource requests, approvals |
| Documents Service | Document Management | File storage, versioning, metadata |
| Notifications Service | Communication | Email, in-app notifications, templates, and the only path through which async events reach the frontend |
| Reporting Service | Analytics & Reporting | KPI calculations, data aggregation, report generation |

**Audit Logging is NOT a separate service.** It is a **cross-cutting concern**: every service that publishes a domain event also writes its own audit log entry locally (or publishes a lightweight `*.Audited` companion event) at the moment the action occurs. There is no standalone "Audit service" and no service should list "Audit" as a subscriber — audit logging happens inside the publishing service itself, alongside whatever it publishes to RabbitMQ.

**Authorization is not a subscriber either.** Per the JWT flow (Section 5), role changes take effect via token claims on next login/token refresh — no service needs to "subscribe" to role changes to enforce authorization in real time.

---

### 2. Database Per Service

Each service owns its own MS SQL database. No service reaches into another's database directly.

| Service | Database Name | Owned Tables |
|---------|---------------|--------------|
| Identity Service | `IdentityDb` | Users, Roles, Permissions, UserRoles |
| Ideas Service | `IdeasDb` | Ideas, Categories, Comments, Attachments |
| Workflow Service | `WorkflowDb` | Stages, Statuses, Timelines, Approvals |
| Funding Service | `FundingDb` | Budgets, Requests, Allocations |
| Documents Service | `DocumentsDb` | Resources, Versions, Metadata |
| Notifications Service | `NotificationsDb` | Notifications, Templates, Emails |
| Reporting Service | `ReportingDb` | Read Models, KPIs, Aggregations |

**Decision:** One shared SQL Server instance with multiple databases (simpler management, still provides logical isolation). Revisit only if a specific service's load justifies its own instance later.

---

### 3. Communication Patterns

#### Synchronous (REST)

For request-response calls the Razor Pages frontend makes **through the Gateway** — never directly to a service.

| Interaction | Endpoint | Service |
|-------------|----------|---------|
| User Login/Logout | `/api/v1/auth/login` | Identity |
| Submit Idea | `/api/v1/ideas` | Ideas |
| Fetch Ideas List | `/api/v1/ideas` | Ideas |
| Fetch Idea Detail | `/api/v1/ideas/{id}` | Ideas |
| Update Idea | `/api/v1/ideas/{id}` | Ideas |
| Change Stage/Status | `/api/v1/workflow/ideas/{id}/transition` | Workflow |
| Add Comment | `/api/v1/ideas/{id}/comments` | Ideas |
| User Management (Admin) | `/api/v1/users` | Identity |
| Category Management | `/api/v1/categories` | Ideas |
| Poll Notifications | `/api/v1/notifications` | Notifications |

**REST API Decision Summary**

| Decision Area | Choice | Rationale |
|---|---|---|
| API Style | RESTful | Standard, simple |
| Versioning | URL Path (v1, v2) | Clear, explicit |
| Serialization | System.Text.Json (STJ) only | Default in .NET, avoid mixing with Newtonsoft to prevent boundary mismatches |
| Error Handling | Consistent `ApiResponse` envelope | Predictable |
| Status Codes | HTTP standards | RESTful |
| Authentication | JWT (Bearer token) | Stateless |
| Authorization | Role + claim based | Flexible |
| Caching | Response + memory cache | Performance |
| Compression | Gzip | Bandwidth |
| Rate Limiting | Fixed window (internal system; revisit to sliding window if burst spikes appear) | Protection |
| Documentation | OpenAPI/Swagger | Developer UX |
| Retry Policy | Polly, 3 retries, exponential backoff with jitter (2s, 4s, 8s) | Resilience without retry storms |
| Circuit Breaker | Polly, opens after 5 consecutive failures, stays open 30s before a half-open trial request | Resilience |
| Timeout (interactive calls) | 5 seconds | User experience — idea submission, review actions, login |
| Timeout (background/report calls) | 30 seconds | Long-running aggregation/report generation only |

---

#### Asynchronous (RabbitMQ)

For cross-service events that shouldn't block the user. **The Razor Pages frontend is never a subscriber to any of these** — anything the UI needs to see arrives via the Notifications Service and the Gateway.

Exchange/queue naming: `imts-exchange` (topic exchange) with one durable queue per subscribing service (`imts-notifications`, `imts-reporting`, `imts-workflow`, `imts-funding`, `imts-ideas`).

**Event Catalog**

| Event Name | Publisher | Subscribers |
|---|---|---|
| `IdeaSubmittedEvent` | Ideas | Notifications, Reporting, Workflow |
| `IdeaUpdatedEvent` | Ideas | Reporting |
| `IdeaStatusChangedEvent` | Workflow | Notifications, Reporting, Ideas |
| `IdeaStageChangedEvent` | Workflow | Notifications, Reporting, Ideas |
| `IdeaApprovedEvent` | Workflow | Notifications, Funding, Reporting |
| `IdeaRejectedEvent` | Workflow | Notifications, Reporting |
| `UserCreatedEvent` | Identity | Notifications |
| `UserRoleChangedEvent` | Identity | Notifications (informational only — authorization itself is enforced via JWT claims, not this event) |
| `DocumentUploadedEvent` | Documents | Notifications, Reporting |
| `FundingRequestedEvent` | Funding | Notifications, Workflow, Reporting |
| `KPIUpdatedEvent` | Reporting | Notifications (Notifications then makes updated KPIs available to the frontend via the Gateway) |
| `TimelineEvent` (deadline) | Workflow | Notifications |

> Note: every publisher writes its own audit entry at publish time (see Section 1) — audit logging is not a separate row in this table because it isn't a subscriber, it's a side effect inside the publisher.

> Note: Funding reads Workflow synchronously (`GET /api/workflow/...` for approval status) **and** Workflow subscribes to `FundingRequestedEvent`. This is an intentional two-way dependency between these two services — flag it during deployment planning, since circular service dependencies affect startup order and can complicate independent deployments.

---

### 4. API Gateway Design

**Choice: YARP (Yet Another Reverse Proxy) with a Backend-for-Frontend (BFF) shape** — routes structured around what the Razor Pages UI needs, not a raw 1:1 passthrough.

**Why YARP:** native ASP.NET Core, no extra dependencies, integrates with the existing .NET ecosystem, can run under IIS.

**Routing Configuration (placeholder hostnames — see deployment note below):**

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

> ⚠️ Hostnames are placeholders (`__SERVICE_HOST__`) rather than assumed container DNS names (e.g. `identity-service:5001`), because **deployment mode (Windows Services vs. containers) is still an open question** (Section: Open Questions). Do not hardcode container-style hostnames until that decision is made — resolve the deployment question first, then fill in real addresses (machine names/ports for Windows Services, or service names for containers).

Gateway listens on a single external port (e.g. 5000) and routes internally to each service.

---

### 5. JWT Flow & Security

**Decision: Gateway validates the JWT; downstream services trust the gateway and do not re-validate.**

1. User logs in → Gateway → Identity Service → issues JWT.
2. Razor Pages app sends the JWT on every subsequent request: `Authorization: Bearer {jwt_token}`.
3. YARP Gateway validates: signature, issuer/audience, expiry, and extracts role/claims.
4. Gateway forwards the request (plus the validated JWT) to the downstream service.
5. Downstream service does **not** re-validate the token — it reads the user/claims from `HttpContext.User` and enforces role-based access on top of that.

**Rationale:** single validation point reduces latency and avoids duplicated validation logic across 7 services; centralization ensures uniform enforcement; downstream services can focus purely on business logic; all services sit inside the BOU internal network, which is the trust boundary this model relies on.

---

### 6. Data Ownership & Cross-Service Access

**Rule: no service reaches into another service's database directly.** All cross-service reads go through synchronous API calls (via the Gateway or internal service-to-service calls) or through consumed events.

| Service | Owns | Reads From |
|---|---|---|
| Identity Service | Users, Roles, Permissions | Nothing (own DB only) |
| Ideas Service | Ideas, Categories, Comments, Attachments | Nothing (own DB only) |
| Workflow Service | Stages, Statuses, Timelines, Approvals | Ideas Service (API, for idea context) |
| Funding Service | Budgets, Requests, Allocations | Workflow Service (API, for approval status) |
| Documents Service | Resources, Versions, Metadata | Ideas Service (API, for idea context) |
| Notifications Service | Notifications, Templates, Emails | Identity Service (API, for user contact info) |
| Reporting Service | Read Models, KPIs, Aggregations | **Events only**, from ALL services — including `FundingRequestedEvent`, so Funding activity is not invisible to dashboards |

**Cross-Service Read Patterns:**
- **Sync API calls:** Workflow → Ideas (`GET /api/ideas/{id}`); Funding → Workflow (approval status)
- **Async events:** Reporting builds all of its read models purely from consumed events — it must never query another service's database directly, including Funding's
- **Gateway/BFF aggregation:** dashboard views that need data from multiple services are composed at the Gateway layer, not by one service querying another's database

---

## Success Criteria

This phase is complete when:

1. `/docs/architecture-decisions.md` exists with all sections filled in
2. Every decision has a documented rationale
3. The REST API contract is fully specified (models, endpoints, error responses)
4. The event catalog is defined with RabbitMQ exchange/queue naming, and every event's subscriber list contains only the 7 real services (no phantom "Admin" or "Audit" or "Authorization" subscribers)
5. YARP gateway routing configuration is in place, using placeholder hostnames until deployment mode is resolved
6. JWT flow is documented with the downstream-trust model
7. Data ownership is clearly defined, including Reporting's events-only rule covering all 7 services

---

## Constraints

- **No code implementation in Phase 2** — this is architecture design only.
- **ASP.NET Core** is the framework for all services.
- **MS SQL Server** is the database (one instance, multiple databases).
- **RabbitMQ** is the message broker for async events.
- **YARP** is the API Gateway technology.
- **Phase 1.5 Razor Pages frontend remains unchanged** during this phase, and never calls a service or RabbitMQ directly — Gateway only.
- The architecture must support **Windows Server + IIS deployment**, pending resolution of the containers-vs-Windows-Services open question below.

---

## Open Questions to Resolve

| Question | Context | Action |
|---|---|---|
| Deployment mode | Windows Services vs. containers — blocks finalizing Gateway hostnames | Resolve before Phase 3 starts; document the decision and update the YARP config placeholders |
| Funding Service scope | Internal-only vs. external finance system integration | Carried over from Phase 0; resolve before building Funding Service in Phase 4 |
| Funding ↔ Workflow circular dependency | Funding calls Workflow synchronously; Workflow subscribes to Funding's events | Acceptable as designed, but document explicitly so deployment/startup order accounts for it |

---

## Deliverable Checklist

- [ ] Service boundaries confirmed (7 services, no phantom services)
- [ ] Database names and owned tables defined
- [ ] REST API decision table completed
- [ ] Event catalog completed with only real-service subscribers
- [ ] YARP routing configuration documented (placeholder hosts, pending deployment decision)
- [ ] JWT flow documented
- [ ] Data ownership matrix completed, including Reporting ← Funding events
- [ ] Deployment mode question resolved or explicitly flagged for pre-Phase-3 resolution

---

## Handoff to Phase 3

Once Phase 2 decisions are documented and the deployment-mode question is resolved, move to **Phase 3 — Build the vertical slice as real services**:

1. **Identity Service** — user store, roles, JWT issuance
2. **API Gateway** — stand up YARP alongside Identity from day one
3. **Ideas Service** — idea submission and storage
4. **Workflow Service** — review state transitions, proving the full submit → review → status-change loop across independently deployed services

The Phase 1.5 Razor Pages frontend gets wired to call these services **through the Gateway only**, replacing stub data with real data.

---

*This document is the foundation for all Phase 3+ implementation decisions. Do not proceed to Phase 3 until Phase 2 decisions are approved and the deployment-mode open question is resolved.*