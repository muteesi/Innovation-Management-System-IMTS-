# IMTS Correction Task: Complete Phase 1.5 (Tailwind Consolidation + Real Razor Pages Migration)

## Context

The `Peter` branch has partial progress: folder renaming, duplicate-dashboard removal, and font-tag dedup are done correctly, and `docs/scope-decisions.md` correctly confirms microservices. However, two things are incomplete and must be finished before Phase 3 (real services) can begin:

1. **Tailwind config consolidation is half-done** and has introduced a *new* inconsistency — pages are now in three different states (10 reference the old browser-inline `tailwind-config.js`, 2 still have their own separate inline config block, and 12 reference no config at all and are silently rendering with Tailwind's default palette instead of the brand colors).
2. **Phase 1.5 (the actual Razor Pages migration) was never executed.** The repo still contains only static HTML — no `.csproj`, `.sln`, or `Pages/` folder exists. The `docs/monolith-assumptions-found.md` file correctly reports "nothing to audit" because there is nothing yet to audit.

This task finishes both. Do not touch `docs/scope-decisions.md` or `docs/architecture-decisions.md` — those are already correct.

---

## Part 1 — Finish Tailwind consolidation (fix before converting to Razor)

1. **Pick `tailwind.config.js`** (the proper Node/CLI-format file, `module.exports = {...}`) as the single source of truth. **Delete `tailwind-config.js`** (the old browser-inline-format file) entirely.
2. **Fix the 2 pages with their own separate inline config** (`innovationteam/reportsandanalytics.html` and `staffuser/submitinnovationideaform.html`) — remove their inline `tailwind.config = {...}` block.
3. **Fix the 12 pages currently referencing no config at all** (this is the highest-priority bug — they're currently rendering off-brand). Full list to check and fix: `itadmin/manageaccounts.html`, `itadmin/auditlog.html`, `itadmin/systemsettings.html`, `itadmin/reports.html`, `itadmin/support.html`, `innovationteam/settings.html`, `innovationteam/settimelines.html`, `innovationteam/support.html`, `staffuser/settings.html`, `staffuser/ideadetails.html`, `staffuser/support.html`, plus re-verify `docs/sitemap.html` doesn't need styling at all (it's a dev reference page, can be left as-is).
4. Since `tailwind.config.js` uses `module.exports`, it is **not directly loadable via a browser `<script src>` tag** — that syntax only works in a Node/build context. Before wiring pages to it, confirm this file's content matches the intended brand palette (previously extracted from the most-trusted page, per the original Phase 1 cleanup), and set up the actual Tailwind CLI build (see Part 2) rather than trying to reference the CLI-format file directly from HTML.
5. **Remove the CDN Tailwind script** (`<script src="https://cdn.tailwindcss.com...">`) from all 24 HTML files — this was flagged in the original Phase 1 cleanup and never done. Replace it with a `<link rel="stylesheet" href="/css/site.css">` (or equivalent output path) once the CLI build is in place.

## Part 2 — Set up the real Tailwind CLI build

1. Add Tailwind CLI as a dev dependency (`package.json` + `npm install -D tailwindcss`).
2. Point `tailwind.config.js`'s `content` array at wherever the Razor views will live (see Part 3) so the build purges unused classes correctly.
3. Create a build input file (e.g. `styles/site.css` with `@tailwind base; @tailwind components; @tailwind utilities;`) and a build script that outputs to `wwwroot/css/site.css`.
4. Confirm the brand color palette in `tailwind.config.js` is the single canonical version — if there's any doubt which of the previously-diverged color sets is correct, flag it rather than guessing.

## Part 3 — Actually execute the Razor Pages migration (this did not happen yet)

This is the real Phase 1.5 work — scaffold and convert, not just document:

1. **Scaffold the ASP.NET Core Razor Pages project**:
   ```
   /Pages/StaffUser/
   /Pages/InnovationTeam/
   /Pages/ItAdmin/
   /Pages/Shared/
   /wwwroot/css/
   ```
2. **Extract `_Layout.cshtml`** from the common `<head>`, sidebar nav, and header currently duplicated across all 24 HTML files. Build role-based nav partials (`_StaffNav.cshtml`, `_InnovationTeamNav.cshtml`, `_ItAdminNav.cshtml`).
3. **Wire the Tailwind CLI output** (`wwwroot/css/site.css` from Part 2) into `_Layout.cshtml` — this is what finally replaces the CDN script everywhere, structurally rather than page-by-page.
4. **Convert each mockup page to `.cshtml` + PageModel**, in this order: Login (with real ASP.NET Core Identity/cookie auth, role-based redirect) → Staff User (submission form, MyIdeas, idea details) → Innovation Team (review, categories, timelines) → IT Admin (accounts, audit log, system settings) → remaining pages. PageModels can return stub/sample data for now — structure them so swapping in real data later doesn't require touching the view.
5. **Wire real forms** (`method="post"` + `[BindProperty]`) replacing the current no-op `action="#"` forms — starting with idea submission and login.
6. **Add htmx/Alpine.js only where the mockups implied dynamic behavior** (tab switches, live filters) — no SPA framework.

## Part 4 — Update the audit doc

Once Part 3 is done, update `docs/monolith-assumptions-found.md` with the real inventory this file was originally meant to contain: does the new project use a single shared `DbContext`? Do any PageModels directly call into another bounded context's logic in-process? List every file where this appears — don't fix it here, just inventory it for Phase 3.

---

## Constraints

- Do not modify `docs/scope-decisions.md` or `docs/architecture-decisions.md` — already correct.
- Do not introduce React/Vue or any SPA build tooling.
- Do not build real backend services or database schemas — PageModels use stub data. That's Phase 3.
- Keep Part 1–2 (Tailwind fix) and Part 3 (Razor migration) as separate commits/PRs so each is independently reviewable.
- If genuinely ambiguous which color palette or page content is canonical, stop and flag it — don't guess silently.

## Deliverable

A working ASP.NET Core Razor Pages app: one canonical Tailwind config wired via a real CLI build (no CDN script anywhere), a shared layout eliminating duplicated nav/head across all pages, all 24 mockup pages converted with stub-data PageModels, working login redirect by role, and an updated `docs/monolith-assumptions-found.md` reflecting the real code.