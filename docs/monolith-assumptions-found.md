# Monolith Assumptions Found (Correction Audit)

## Status: Phase 1.5 Razor Pages Migration Complete — Monolith-Shaped Code Inventory

The Phase 1.5 Razor Pages migration has been executed. The codebase now contains a single ASP.NET Core Razor Pages project (`src/IMTS/`) hosting all three bounded contexts (StaffUser, InnovationTeam, ItAdmin) in one process. Below is the inventory of monolith-shaped code patterns found, to be addressed in Phase 3.

---

## 1. Single Shared Data Source (No Database Isolation)

**Severity: HIGH** — All bounded contexts share the same static data class.

| File | Lines | Issue |
|------|-------|-------|
| `src/IMTS/Models/StubData.cs` | 1-66 | Single static class holds ALL data: ideas, notifications, audit logs, users, routing. No separation between StaffUser, InnovationTeam, and ItAdmin data stores. |
| `src/IMTS/Models/StubData.cs` | 5-30 | Three static `UserModel` singletons (`CurrentStaff`, `CurrentReviewer`, `CurrentAdmin`) are process-wide — not session-scoped or request-scoped. Every user of the same role sees the exact same data. |

## 2. No Service/Repository Layer (All Logic in PageModels)

**Severity: HIGH** — Every PageModel directly calls into the static `StubData` class.

The following PageModels directly access `StubData.*` static methods, bypassing any service abstraction or API boundary:

| File | Line(s) | StubData Call |
|------|---------|---------------|
| `src/IMTS/Pages/StaffUser/Dashboard.cshtml.cs` | 22 | `StubData.GetIdeas()` — reads ALL ideas including others' |
| `src/IMTS/Pages/StaffUser/MyIdeas.cshtml.cs` | 17 | `StubData.GetIdeas()` — filters by name but accesses global pool |
| `src/IMTS/Pages/StaffUser/IdeaDetails.cshtml.cs` | 16 | `StubData.GetIdeas().FirstOrDefault()` — can access any idea by ID |
| `src/IMTS/Pages/StaffUser/Notifications.cshtml.cs` | 16 | `StubData.GetNotifications()` — shared notification list |
| `src/IMTS/Pages/StaffUser/SubmitIdea.cshtml.cs` | 61, 71 | `StubData.CurrentStaff` |
| `src/IMTS/Pages/StaffUser/Resources.cshtml.cs` | 10 | `StubData.CurrentStaff` |
| `src/IMTS/Pages/StaffUser/Settings.cshtml.cs` | 40, 50 | `StubData.CurrentStaff` |
| `src/IMTS/Pages/StaffUser/Support.cshtml.cs` | 25, 34 | `StubData.CurrentStaff` |
| `src/IMTS/Pages/InnovationTeam/Dashboard.cshtml.cs` | 19-21 | `StubData.CurrentReviewer`, `StubData.GetIdeas()` |
| `src/IMTS/Pages/InnovationTeam/ReviewIdeas.cshtml.cs` | 37-57 | `StubData.CurrentReviewer`, `StubData.GetIdeas()` |
| `src/IMTS/Pages/InnovationTeam/ManageCategories.cshtml.cs` | 78 | `StubData.GetIdeas()` — grouping by category |
| `src/IMTS/Pages/InnovationTeam/ReportsAndAnalytics.cshtml.cs` | 26-31 | `StubData.GetIdeas()` — analytics aggregation |
| `src/IMTS/Pages/InnovationTeam/ManageResources.cshtml.cs` | 41-66 | `StubData.CurrentReviewer` |
| `src/IMTS/Pages/InnovationTeam/SetTimelines.cshtml.cs` | 53-78 | `StubData.CurrentReviewer` |
| `src/IMTS/Pages/InnovationTeam/Settings.cshtml.cs` | 36, 42 | `StubData.CurrentReviewer` |
| `src/IMTS/Pages/InnovationTeam/Support.cshtml.cs` | 29, 36 | `StubData.CurrentReviewer` |
| `src/IMTS/Pages/ItAdmin/Dashboard.cshtml.cs` | 8, 14 | `StubData.CurrentAdmin`, `StubData.GetAuditLogs()` |
| `src/IMTS/Pages/ItAdmin/ManageAccounts.cshtml.cs` | 8 | `StubData.CurrentAdmin` |
| `src/IMTS/Pages/ItAdmin/SystemSettings.cshtml.cs` | 8 | `StubData.CurrentAdmin` |
| `src/IMTS/Pages/ItAdmin/AuditLog.cshtml.cs` | 8, 19 | `StubData.CurrentAdmin`, `StubData.GetAuditLogs()` |
| `src/IMTS/Pages/ItAdmin/Reports.cshtml.cs` | 8 | `StubData.CurrentAdmin` |
| `src/IMTS/Pages/ItAdmin/Support.cshtml.cs` | 8 | `StubData.CurrentAdmin` |

## 3. Cross-Boundary Data Access

**Severity: MEDIUM** — Pages in one bounded context directly access data that belongs to another bounded context.

- **StaffUser pages call `StubData.GetIdeas()`** which returns ALL ideas, not just the current user's. The only filtering is a string comparison on `Submitter` name in the page model (`MyIdeas.cshtml.cs:17`). An `IdeaDetails` page can access any idea by ID regardless of ownership (`IdeaDetails.cshtml.cs:16`).
- **InnovationTeam pages call `StubData.GetIdeas()`** to count ideas per category and generate analytics, directly ingesting StaffUser domain data in-process.
- **ItAdmin pages call `StubData.GetAuditLogs()`** which is a shared static list — no per-service isolation.

## 4. Shared Base Class and Layout

**Severity: LOW-MEDIUM** — Infrastructure coupling.

| File | Lines | Issue |
|------|-------|-------|
| `src/IMTS/Models/BasePageModel.cs` | 5-16 | All three role areas inherit from this single base class. |
| `src/IMTS/Pages/Shared/_Layout.cshtml` | 24-39 | Single layout renders role-specific nav partials based on `ViewData["Role"]`. |
| `src/IMTS/Models/StubData.cs` | 60-65 | `GetRoleRoutes()` is a single dictionary mapping all roles to dashboards — couples all contexts in one place. |

## 5. No EF Core / Database Infrastructure (Expected at Phase 1.5)

**Not a monolith problem per se, but worth noting:** The project has zero database infrastructure.
- `src/IMTS/IMTS.csproj` has no `<PackageReference>` entries.
- No `DbContext`, `AddDbContext`, or connection strings in any file.
- All data is static in-memory collections in `StubData.cs`.

When a real database is introduced in Phase 3, care must be taken to use per-bounded-context `DbContext` instances rather than a single shared one.

---

## Auth Flow: Two-Hop BFF Pattern — Confirmed Correct

The authentication flow correctly follows the two-hop Backend-for-Frontend (BFF) pattern:

1. **Browser ↔ Razor Pages App**: Standard ASP.NET Core cookie authentication (`CookieAuthenticationDefaults.AuthenticationScheme`). The browser receives an HTTP-only cookie via `HttpContext.SignInAsync()`. No JWT is ever transmitted to the browser.

2. **Razor Pages App ↔ API Gateway (future)**: When Phase 3 introduces the API Gateway, the Razor Pages backend will obtain a JWT Bearer token at login time (server-side only) and use it for calls to downstream services. This is consistent with `docs/architecture-decisions.md`.

**Relevant files:**
- `src/IMTS/Program.cs:6-12` — Cookie auth configuration
- `src/IMTS/Pages/Login.cshtml.cs:45-55` — Claims identity creation and cookie issuance

**Current limitation:** PageModels do not resolve the current user from `HttpContext.User` claims. Instead, each PageModel uses a hardcoded static singleton (`StubData.CurrentStaff`/`CurrentReviewer`/`CurrentAdmin`). This decouples the auth cookie from the data the user sees and will need fixing in Phase 3.

---

## Summary for Phase 3

| Pattern | Severity | Files Affected |
|---------|----------|----------------|
| Single shared data source | HIGH | `Models/StubData.cs` — all data in one class |
| No service/repository layer | HIGH | All 23 PageModel `.cs` files |
| Static singleton users | HIGH | `Models/StubData.cs:5-30` |
| Cross-boundary data access | MEDIUM | `StaffUser/Dashboard.cshtml.cs:22`, `MyIdeas.cshtml.cs:17`, `IdeaDetails.cshtml.cs:16`, `InnovationTeam/ManageCategories.cshtml.cs:78`, `InnovationTeam/ReportsAndAnalytics.cshtml.cs:26-31` |
| Shared base class | LOW | `Models/BasePageModel.cs` |
| Single layout rendering all navs | LOW | `Pages/Shared/_Layout.cshtml` |
| Auth two-hop BFF correct | GOOD (no fix needed) | `Program.cs`, `Login.cshtml.cs` |

**Total files to split/refactor in Phase 3:** 1 project → 7 services; ~25 source files to reorganize.
