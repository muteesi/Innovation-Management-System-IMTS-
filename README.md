# Innovation Management System (IMS) — Microservices Architecture

**Version:** 0.2 (Draft)
**Author:** Amanya Peter, Mutesi Flavia Kirabo, Kasamba Luqman
**Stack constraints:** Windows Server, MS SQL Server. Backend language -ASP.NET Core (C#) are both viable;

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) (for Tailwind CSS build)

### Quick Start

```bash
# 1. Install Tailwind CLI and build the CSS
npm install
npm run build:css

# 2. Run the ASP.NET Core application
dotnet run --project src/IMTS
```

The app starts at `http://localhost:5000`. Login with any of these usernames (password is ignored):

| Username (any case) | Role | Dashboard |
|---|---|---|
| `staff`, `jonathan`, `john`, `j.doe` | Staff User | `/StaffUser/Dashboard` |
| `admin`, `katumba`, `j.katumba` | IT Admin | `/ItAdmin/Dashboard` |
| `reviewer`, `rose`, `r.namaganda`, `namaganda` | Innovation Team | `/InnovationTeam/Dashboard` |

### Tailwind CSS (development)

```bash
npm run watch:css   # watches and rebuilds on changes
npm run build:css   # one-off production build
```

The project uses a single canonical `tailwind.config.js` with the brand palette. The CLI build output goes to `src/IMTS/wwwroot/css/site.css` and is linked from the shared `_Layout.cshtml`.

---

## 1. Purpose & Scope

This document defines the target microservices architecture for the Innovation
Management System — covering idea submission, review/evaluation, experimentation,
deployment, and funding pipeline stages. It is a working draft intended to be refined as
service boundaries are validated against real usage.

---

## 2. Architectural Principles

1. **Single responsibility per service** — each service owns one bounded context and its own data.
2. **Database-per-service** — no service reads/writes another service's tables directly. On a
   single MS SQL Server instance this means separate databases (or schemas) per service, not a
   shared schema.
3. **API Gateway as the only public entry point** — clients never call services directly.
4. **Async-first for cross-service side effects** — status changes, notifications, and audit
   events are published, not called synchronously, to avoid tight coupling and cascading failures.
5. **Stateless services** — session/auth state lives in tokens (JWT), not in-memory on a service
   instance, so any instance can serve any request.
6. **Design for partial failure** — every synchronous inter-service call has a timeout, retry
   policy, and fallback (circuit breaker).

---

## 3. High-Level Architecture

```
                                   ┌─────────────────────┐
                                   │   Web / Mobile UI    │
                                   └──────────┬───────────┘
                                              │ HTTPS
                                   ┌──────────▼───────────┐
                                   │     API Gateway       │  (YARP)
                                   │  - Auth token check    │
                                   │  - Routing              │
                                   │  - Rate limiting        │
                                   └──────────┬───────────┘
                    ┌───────────┬─────────────┼─────────────┬───────────────┐
                    ▼           ▼             ▼             ▼               ▼
            ┌──────────┐ ┌───────────┐ ┌────────────┐ ┌───────────┐ ┌──────────────┐
            │ Identity │ │   Idea    │ │  Review &  │ │  Funding  │ │ Document /   │
            │ & Access │ │Submission │ │  Workflow  │ │  Pipeline │ │  Attachment  │
            │ Service  │ │  Service  │ │  Service   │ │  Service  │ │   Service    │
            └─────┬────┘ └─────┬─────┘ └──────┬─────┘ └─────┬─────┘ └──────┬───────┘
                  │            │              │             │              │
                  │            └──────┬───────┴──────┬──────┴──────┬───────┘
                  │                   │  Event Bus (Kafka)  
                  │                   └───────────────┬──────────────┘
                  │                          ┌─────────▼──────────┐
                  │                          │  Notification      │
                  │                          │  Service           │
                  │                          └────────────────────┘
                  │                          ┌───────────────────────┐
                  └─────────────────────────►│  Reporting/Analytics  │
                                             │  Service (read model) │
                                             └───────────────────────┘

   Config Server + Service Registry (Spring Cloud Config + Eureka, or .NET equivalent) — all services
```

---

## 4. Service Decomposition

| Service | Responsibility | Owns Data (MS SQL DB) | Key APIs |
|---|---|---|---|
| **Identity & Access** | Auth, users, roles/permissions, JWT issuance | `ims_identity` | `POST /auth/login`, `POST /users`, `GET /users/{id}/roles` |
| **Idea Submission** | Idea capture, drafts, submitter view/edit/retract | `ims_ideas` | `POST /ideas`, `GET /ideas/{id}`, `PATCH /ideas/{id}` |
| **Review & Workflow** | Stage-gating (concept brief → experimentation → deployment), reviewer decisions, timelines/SLAs | `ims_workflow` | `POST /reviews/{ideaId}/decision`, `GET /workflow/{ideaId}/stage` |
| **Funding Pipeline** | Budget requests, approvals, disbursement tracking | `ims_funding` | `POST /funding/requests`, `PATCH /funding/{id}/approve` |
| **Document/Attachment** | File upload/storage metadata, versioning, access control | `ims_documents` | `POST /documents`, `GET /documents/{id}` |
| **Notification** | Email/SMS/in-app notifications, templates | `ims_notifications` (mostly transient) | consumes events only; `GET /notifications/{userId}` |
| **Reporting/Analytics** | Cross-service read model for dashboards, KPI reports (PDF/Excel export) | `ims_reporting` (denormalized, event-sourced) | `GET /reports/kpi`, `GET /reports/export` |

**Note on granularity:** if your team is small, consider merging Review & Workflow with Idea
Submission initially ("Idea Lifecycle Service") and splitting later once the workflow logic
grows complex enough to justify its own team/deploy cadence. Over-splitting early is a common
trap — start with 4–5 services, not 8.

---

## 5. Communication Patterns

**Synchronous (REST, through the Gateway or direct service-to-service on the internal
network):**
- Used only when the caller needs an immediate answer to proceed (e.g., Review Service checking
  Identity Service to validate a reviewer's role before accepting a decision).
- Every sync call wrapped with timeout (2–3s), retry (max 2), circuit breaker —
  Resilience4j on Spring Boot, or Polly on ASP.NET Core.

**Asynchronous (event bus — RabbitMQ recommended over Kafka for a Windows-hosted, moderate-
throughput system; simpler ops on Windows Server):**

| Event | Published by | Consumed by |
|---|---|---|
| `IdeaSubmitted` | Idea Submission | Review & Workflow, Notification, Reporting |
| `StageChanged` | Review & Workflow | Notification, Reporting, Funding Pipeline |
| `ReviewDecisionMade` | Review & Workflow | Notification, Reporting |
| `FundingApproved` | Funding Pipeline | Notification, Reporting |
| `DocumentUploaded` | Document/Attachment | Reporting |
| `UserAccountChanged` | Identity & Access | Notification |

Reporting/Analytics is built as an **event-sourced read model** — it never queries other
services' databases; it rebuilds its own denormalized view from the event stream. This keeps
KPI/report generation fast and decoupled from transactional load on other services.

---

## 6. Data Architecture

- **One MS SQL Server instance, multiple databases** (not one shared DB) — gives you
  database-per-service isolation without needing separate SQL Server licenses/instances.
- No cross-database joins or foreign keys between service databases. If Review & Workflow needs
  idea details, it either stores a denormalized copy (updated via events) or calls Idea
  Submission's API.
- Each service manages its own schema migrations independently — Flyway/Liquibase on Spring
  Boot, or EF Core Migrations on ASP.NET Core.
- Document binaries (attachments) should NOT go into MS SQL as BLOBs at scale — store files on a
  file share or object storage, with only metadata (path, hash, size) in `ims_documents`.

---

## 7. API Gateway & Security

- **Gateway:** single HTTPS entry point, terminates TLS, validates JWT signature/expiry on
  every request before routing — Spring Cloud Gateway on Java, or YARP (Yet Another Reverse
  Proxy) / Ocelot on .NET.
- **Auth model:** OAuth2/JWT issued by Identity & Access Service. Access token (short-lived,
  ~15 min) + refresh token. Role/permission claims embedded in the JWT so downstream services
  don't need to call back to Identity for every request. On .NET, ASP.NET Core Identity gives
  you most of this out of the box; on Java, Spring Security handles the equivalent.
- **Service-to-service auth:** internal calls use a service-level client credential (mutual
  TLS or a shared internal JWT), never end-user tokens passed through blindly.
- **Rate limiting & request logging** enforced at the Gateway, not duplicated per service.

---

## 8. Deployment Architecture (Windows Server / on-prem)

Given the Windows Server + MS SQL Server constraint, two realistic options:

**Option A — Windows Services (no containers)**

*Spring Boot:* each service packaged as an executable JAR, run via `WinSW` (Windows Service
Wrapper) as a native Windows Service. IIS sits in front as a reverse proxy (using
Application Request Routing) to the Gateway service.

*ASP.NET Core:* each service calls `.UseWindowsService()` and runs natively as a Windows
Service — no third-party wrapper needed. IIS integrates directly via the ASP.NET Core
Module (ANCM) as an in-process or reverse-proxy host, which is a first-class, well-documented
IIS deployment path (unlike proxying to a JVM process). This is the more natural fit if your
team already leans Java-to-C# via existing coursework/knowledge and wants less deployment
plumbing.

- Pros: matches BOU-style constraints exactly (no container platform needed), simpler for IT
  Ops teams unfamiliar with Docker. .NET has a shorter path to production here.
- Cons: manual scaling, no isolation between services on the same box.

**Option B — Docker Desktop / Docker Engine on Windows Server + Windows containers or WSL2**
Each service in its own container, orchestrated with Docker Compose (or Kubernetes if the org
is ready for it).
- Pros: consistent deployment, easier horizontal scaling, closer to industry-standard
  microservices practice.
- Cons: needs buy-in from IT Ops to run container runtime on Windows Server; more operational
  complexity upfront.

**Recommendation:** start with Option A if you're deploying into an environment like BOU's
(strict on-prem, Windows-only, conservative IT Ops), and design services to be
container-ready (externalized config, no local file-system state beyond documents) so you can
migrate to Option B later without a rewrite.

```
┌─────────────────────────── Windows Server ───────────────────────────┐
│                                                                        │
│   IIS (reverse proxy / TLS termination, or ANCM in-process for .NET) │
│         │                                                              │
│         ▼                                                              │
│   API Gateway (Windows Service — WinSW-wrapped JAR, or native         │
│                 .UseWindowsService() for ASP.NET Core)                │
│         │                                                              │
│   ┌─────┴─────┬─────────┬─────────┬─────────┬─────────┐               │
│   ▼           ▼         ▼         ▼         ▼         ▼               │
│ Identity   Idea Sub   Review    Funding   Documents  Notification      │
│ (Win Svc)  (Win Svc)  (Win Svc) (Win Svc) (Win Svc)  (Win Svc)         │
│                                                                        │
│   RabbitMQ (Windows Service)         MS SQL Server (multiple DBs)     │
│                                                                        │
└────────────────────────────────────────────────────────────────────┘
```

---

## 9. Cross-Cutting Concerns

- **Centralized config:** Spring Cloud Config Server on Java; on .NET, `appsettings.json` per
  environment plus Consul KV or Azure App Configuration if centralization is needed. For a
  small team, a shared config repo is fine either way — no hardcoded connection strings.
- **Service discovery:** Eureka on Java; Consul (works the same way on .NET) or static config
  if everything runs on one box — simpler, less overhead for a small deployment.
- **Logging:** structured JSON logs from every service, shipped to a central store. Java
  typically uses Logback/SLF4J; .NET uses Serilog (very common pairing with Seq for a
  searchable log viewer on Windows). Either way, start with rolling file + Windows Event Log,
  add a proper log store later.
- **Audit trail:** a dedicated `AuditEvent` published by every service on create/update/delete —
  consumed into its own audit log store, mirroring the BOU document's audit requirements
  (log ID, timestamp, event type, user, operation, source IP).
- **Health checks:** `/actuator/health` on Spring Boot; ASP.NET Core has built-in health check
  middleware (`AddHealthChecks()`) exposing the same kind of endpoint.
- **Resilience:** Resilience4j on Spring Boot; Polly on ASP.NET Core — both give circuit
  breakers, retries, and bulkheads on sync calls.

---

## 10. Non-Functional Targets (draft — tune to your context)

| Attribute | Target |
|---|---|
| Availability | 99.5% uptime per quarter |
| Response time | <2s for standard requests under normal load |
| Concurrent users | 50+ with <10% performance degradation |
| Audit retention | all user actions logged, exportable (PDF/Excel/CSV) |
| Backup | daily automated DB backup per service database |

---

## 11. Technology Stack Comparison

| Layer | Spring Boot (Java) | ASP.NET Core (C#) |
|---|---|---|
| Services | Spring Boot 3.x (Java 17+) | ASP.NET Core 8 (C# 12) |
| API Gateway | Spring Cloud Gateway | YARP or Ocelot |
| Service discovery | Eureka (or static, single-host) | Consul (or static, single-host) |
| Config | Spring Cloud Config | `appsettings.json` + Consul KV / Azure App Config |
| Sync inter-service calls | OpenFeign + Resilience4j | `HttpClientFactory` + Polly |
| Async messaging | RabbitMQ (via Spring AMQP) | RabbitMQ (via MassTransit) |
| Database | MS SQL Server (one instance, DB-per-service) | same |
| Migrations | Flyway | EF Core Migrations |
| Auth | Spring Security + OAuth2/JWT | ASP.NET Core Identity + JWT (built-in) |
| Hosting | WinSW-wrapped Windows Service + IIS reverse proxy | Native `.UseWindowsService()` + IIS via ANCM |
| Logging | Logback/SLF4J | Serilog (+ Seq) |
| Reporting export | Apache POI (Excel), iText/OpenPDF (PDF) | ClosedXML (Excel), QuestPDF/iText (PDF) |
| Testing | JUnit 5 + Mockito | xUnit + Moq |

**Practical note:** MassTransit on .NET is worth calling out specifically — it wraps RabbitMQ
publish/subscribe, retry, outbox pattern, and even saga orchestration (useful for your
multi-stage review workflow: submitted → concept brief → experimentation → deployment) behind
a much thinner API than hand-rolling the equivalent with Spring AMQP. If the Review & Workflow
service's stage-gating logic gets complex, a MassTransit saga is a strong fit for it.

**Decision framing:** given you already have Java experience (favors Spring Boot's learning
curve) but the deployment target is 100% Windows Server + IIS + MS SQL (favors ASP.NET Core's
native tooling fit), this is genuinely close. If BOU-style institutional deployment
constraints are representative of where this system will actually run, ASP.NET Core has less
friction end-to-end. If you're optimizing for what you can build fastest given existing
knowledge, Spring Boot wins on ramp-up time.

---

## 12. Open Questions to Resolve Next

1. Team size — does splitting into 7 services make sense now, or start with a smaller
   "modular monolith that can split later" approach?
2. Container readiness — will the deployment environment (CITT / your hosting target) allow
   Docker, or is it strictly Windows Services like BOU's?
3. Funding Pipeline scope — is this internal budget tracking only, or does it need to integrate
   with an external finance system?
4. Multi-tenancy — will this system serve multiple organizations (commercial SaaS) or one
   deployment per client?
