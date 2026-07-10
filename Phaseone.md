# Agent Task: IMTS Frontend Cleanup & Razor Pages Migration

## Context

You are working on the **Innovation Management System (IMTS)** for Bank of Uganda, currently at `https://github.com/AmanyaPeter/Innovation-Management-System-IMTS-`.

The repo currently contains **26 static HTML mockup pages** (Tailwind CSS via CDN, Google Fonts, Material Symbols icons) with **no backend, no JS logic, no working forms**. The README describes a much larger target microservices architecture (ASP.NET Core, MS SQL Server, Windows Server/IIS) — treat the README as the eventual direction, not as something already built. Your job in this task is NOT to build the microservices backend yet. It is to take the static mockups through cleanup and into a real, server-rendered ASP.NET Core Razor Pages app that can later be wired to real services.

Roles in the app: **Staff User** (`staffuser/`), **Innovation Team** (`innovationteam/`), **IT Admin** (`IT Admin/` — note the space, to be renamed).

Stack decision already made: **ASP.NET Core Razor Pages**, server-rendered, one deploy target (Windows Service + IIS eventually). Use htmx or Alpine.js for lightweight interactivity — do NOT introduce React or a separate SPA build.

---

## Phase 0 — Reconcile scope before touching code

1. Read the README in full. Do not delete or rewrite its architecture sections yet — just produce a short `docs/scope-decisions.md` file summarizing:
   - Confirm this build targets a **modular monolith** (single ASP.NET Core app with clear internal module boundaries: Identity, Ideas, Workflow, Funding, Documents) rather than 7 separately deployed services, unless you find explicit evidence in the repo that contradicts this.
   - Note the deployment target as Windows Service + IIS.
   - Flag the Funding Pipeline scope and multi-tenancy questions from README section 12 as unresolved — do not guess answers, just document them as open.

---

## Phase 1 — Clean up the existing static mockups (do this before converting to Razor)

Work directly in the existing HTML files first so the diffs are easy to review:

1. **Resolve duplicate admin dashboards**: `IT Admin/ITadmin.html` and `IT Admin/admindashboard.html` both claim to be the admin dashboard and link to each other. Compare their content, merge any unique sections into `ITadmin.html` (it's the one referenced by the login redirect), delete `admindashboard.html`, and update every sidebar link and `sitemap.html` reference that pointed to it.
2. **Delete the orphaned root-level `settings.html`** — confirm via grep that nothing links to it (`grep -rn "href=\"settings.html\"" .` should only match files inside `staffuser/` and `innovationteam/` referencing their own local copies), then remove it.
3. **Extract one shared Tailwind config.** Every page currently has its own inline `<script id="tailwind-config">` block, and the color hex values have already drifted between pages (verify by diffing `login.html` vs `staffuser/staffdashboard.html`). Create a single `tailwind.config.js` with one canonical color palette (pick the version that best matches the BOU brand/logo, ask a human if genuinely ambiguous) and remove the inline duplicates from every HTML file.
4. **Remove duplicate `<link>` tags** — several files load the same Google Fonts stylesheet twice in `<head>`. Deduplicate.
5. **Rename `IT Admin/` to `itadmin/`** (lowercase, no space) to match `staffuser/` and `innovationteam/` conventions. Update every relative link across all 26 files, plus the login redirect (`window.location.href = 'IT Admin/ITadmin.html'`) and `sitemap.html`.
6. **Move `sitemap.html`** into a `/docs` folder or clearly label it as a dev reference — confirm no nav links to it (it currently doesn't).

Commit Phase 1 as its own commit/PR before moving on, so it's reviewable in isolation.

---

## Phase 1.5 — Migrate to ASP.NET Core Razor Pages

1. **Scaffold the project**: new ASP.NET Core Razor Pages project. Folder structure:
   ```
   /Pages/StaffUser/
   /Pages/InnovationTeam/
   /Pages/ItAdmin/
   /Pages/Shared/
   /wwwroot/css/
   ```
2. **Extract a shared `_Layout.cshtml`** from the common `<head>`, sidebar nav, and header markup duplicated across all pages. Build role-based sidebar partials (`_StaffNav.cshtml`, `_InnovationTeamNav.cshtml`, `_ItAdminNav.cshtml`) selected based on the logged-in user's role — not three hardcoded copies.
3. **Set up Tailwind CLI build** (not the CDN script) compiling to `wwwroot/css/site.css`, using the single `tailwind.config.js` from Phase 1 as the source of truth.
4. **Convert each mockup page to `.cshtml` + PageModel**, one role at a time, in this order:
   1. Login page → wire to ASP.NET Core Identity / cookie auth with role-based redirect after `POST`.
   2. Staff User: idea submission form, MyIdeas, idea details.
   3. Innovation Team: review ideas, manage categories, timelines.
   4. IT Admin: manage accounts, audit log, system settings.
   5. Remaining pages (notifications, resources, reports, support, settings) for each role.
   - Replace hardcoded lists/tables with `@foreach` loops over PageModel properties. It's fine for the PageModel to return stub/sample data for now if the real backend service doesn't exist yet — just structure it so swapping in a real data source later doesn't require touching the view.
5. **Wire up forms** with real `method="post"` handlers and `[BindProperty]` models, replacing current no-op `action="#"` forms — starting with the idea submission form and login form.
6. **Add htmx/Alpine.js** only where the mockups implied dynamic behavior (tab switches, live filters on audit log/MyIdeas, inline validation) — keep it minimal, don't reach for a full JS framework.

---

## Constraints / things to avoid

- Do not introduce React, Vue, or any SPA build tooling — this is a server-rendered app by design decision.
- Do not delete the README's architecture content — it's the target reference, just not what's built yet.
- Do not invent backend services or database schemas in this task — PageModels can use stub data; real service wiring is a separate future task.
- Keep Phase 1 (static HTML cleanup) and Phase 1.5 (Razor migration) as separate commits/PRs so each is independently reviewable.
- If you hit a genuine ambiguity (e.g. which dashboard file has the "correct" content, which color palette is canonical), stop and flag it rather than guessing silently.

## Deliverable

A working ASP.NET Core Razor Pages app that renders all pages from the original mockups (with stub data where needed), a shared layout eliminating the duplicated nav/head, a single Tailwind config, working login redirect by role, and a `docs/scope-decisions.md` summarizing Phase 0 output.