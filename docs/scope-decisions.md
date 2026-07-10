# Scope Decisions (Phase 0)

## Build Target

- **Microservices**: 7 independently deployable ASP.NET Core services (Identity, Ideas, Workflow, Funding, Documents, Notifications, Reporting), each with its own MS SQL database, communicating through a YARP API Gateway. See `docs/architecture-decisions.md` for full service boundaries, communication patterns, and data ownership rules.

## Deployment Target

- **Windows Service + IIS** — per the README recommendation (Option A), using ASP.NET Core's native `.UseWindowsService()` and IIS via ANCM.

## Authentication Flow (BFF Pattern)

- **Browser ↔ Razor Pages app**: standard ASP.NET Core **cookie authentication** — the browser never sees or holds a JWT.
- **Razor Pages app (server-side) ↔ API Gateway**: **JWT Bearer token**, obtained by the Razor Pages backend at login time and held server-side only (never exposed to the browser). This is the Backend-for-Frontend (BFF) pattern.

This two-hop model ensures the browser interacts only via session cookies, while service-to-service calls use JWT through the Gateway.

## Unresolved Questions

The following are documented as open — no decisions have been made:

1. **Funding Pipeline scope** — the README does not clarify whether this is internal budget tracking only or if it needs to integrate with an external finance system at BOU.
2. **Multi-tenancy** — the README asks whether the system will serve multiple organizations (commercial SaaS) or be deployed per-client. This remains unresolved.
