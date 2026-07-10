# Monolith Assumptions Found (Correction Audit)

## Status: No Razor Pages Code to Audit

The Phase 1.5 Razor Pages migration has **not been executed yet**. The current codebase consists solely of static HTML mockup pages. Therefore:

- **No shared `DbContext`** exists — no database access code has been written.
- **No in-process cross-boundary calls** exist — all pages are static HTML with no backend logic.
- **No PageModels** exist to inspect for stub data usage.

This file serves as a placeholder. Once Phase 1.5 (Razor Pages migration) is executed, this document should be updated with an inventory of any monolith-shaped code found.

## Expected findings (for Phase 3)

When the Razor Pages app is built, the following are expected to be present in a monolith-shaped Phase 1.5 build:

1. A single `WebApplication` project hosting all bounded contexts (Ideas, Workflow, Funding, etc.) in one process — this is the expected starting point and will need splitting in Phase 3.
2. PageModels that directly call into other bounded contexts' logic rather than going through an API Gateway or service client — these should be listed per file.
3. Any single `DbContext` that spans multiple bounded contexts — should be identified and listed.

## Auth flow note

No code comments or documentation in the current static HTML suggest a single-hop JWT flow. The login.html mockup uses `window.location.href` redirects and has no JWT handling. This will be addressed when the Razor Pages app is built (see JWT correction in `scope-decisions.md`).
