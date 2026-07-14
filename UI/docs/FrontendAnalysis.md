# Frontend Analysis — Bank of Uganda Innovation Management System

## Overview

The frontend is a multi-role portal system built with vanilla HTML, Tailwind CSS (CDN), Material Symbols, and plain JavaScript. There are 4 distinct portals:

| Portal | Role | Base Path |
|--------|------|-----------|
| Staff Portal | Staff | `/staffuser/` |
| Innovation Team | InnovationTeam | `/innovationteam/` |
| IT Administration | ITAdmin | `/IT Admin/` |
| General | Any | `/` (support, login) |

---

## Page-by-Page Analysis

### 1. Login Page (`login.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | Authenticate users and redirect to role-specific dashboard |
| **Current Data Source** | Hardcoded `DEMO_USERS` object in inline script |
| **Future API Endpoint** | `POST /api/auth/login` |
| **Backend Service** | Authentication Service (future) |
| **Required Table** | Users |
| **Expected Request** | `{ username: string, password: string }` |
| **Expected Response** | `{ token: string, user: { id, name, role, email, department }, redirectUrl: string }` |
| **Notes** | Currently stores session in localStorage as `imts_session`. Uses hardcoded demo credentials. |

---

### 2. Staff Dashboard (`staffdashboard.html` and `staffuser/staffdashboard.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | Show welcome message, stats cards, recent ideas |
| **Current Data Source** | Hardcoded stats (12, 4, 6, 2), hardcoded idea cards (2 items) |
| **Future API Endpoint** | `GET /api/dashboard/staff` |
| **Backend Service** | Dashboard / Idea Service |
| **Required Table** | Ideas (for stats) |
| **Expected Request** | `GET /api/dashboard/staff?userId={id}` |
| **Expected Response** | `{ totalSubmitted, underReview, approved, inDevelopment, recentIdeas: [] }` |

---

### 3. My Ideas (`staffuser/MyIdeas.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | List, filter, search user's submitted ideas |
| **Current Data Source** | Hardcoded `ideas` array (6 items), localStorage via `imts_ideas` |
| **Future API Endpoint** | `GET /api/ideas?userId={id}&status={status}&search={search}` |
| **Backend Service** | Idea Service |
| **Required Table** | Ideas |
| **Expected Request** | `GET /api/ideas?userId=1&page=1&pageSize=10&status=all&search=` |
| **Expected Response** | `{ data: Idea[], total: number, page: number, pageSize: number }` |
| **Notes** | Filter buttons: All, Pending, Under Review, Approved, Declined. Stats computed from filtered list. |

---

### 4. Submit New Idea (`staffuser/submitinnovationideaform.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | Multi-step form (4 pages) for idea submission |
| **Current Data Source** | localStorage via `imts_ideas` |
| **Future API Endpoint** | `POST /api/ideas` |
| **Backend Service** | Idea Service |
| **Required Table** | Ideas, IdeaTeamMembers, IdeaAttachments |
| **Expected Request** | `{ submitterName, submitterEmail, submitterDepartment, submitterRank, submissionType, businessUnit, station, teamMembers, ideaTitle, ideaCategory, description, problemStatement, proposedSolution, expectedImpact, attachments: [] }` |
| **Expected Response** | `{ id: number, reference: string, message: string }` |
| **Notes** | 4-step form with matrix tables for team composition. File attachments read as base64 data URLs. |

---

### 5. Notifications (`staffuser/notifications.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | Display, filter, manage notifications |
| **Current Data Source** | Hardcoded `notifications` array (5 items), localStorage `imts_notifications` |
| **Future API Endpoint** | `GET /api/notifications` |
| **Backend Service** | Notification Service |
| **Required Table** | Notifications |
| **Expected Request** | `GET /api/notifications?userId={id}&filter=all&page=1&pageSize=20` |
| **Expected Response** | `{ data: Notification[], total, unreadCount }` |
| **Actions** | Mark read, mark all read, delete, filter (all/unread/read) |

---

### 6. Resources (`staffuser/resources.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | Browse and download innovation resources |
| **Current Data Source** | Hardcoded `resources` array (8 items) |
| **Future API Endpoint** | `GET /api/resources` |
| **Backend Service** | Resource Service |
| **Required Table** | Resources, ResourceCategories |
| **Expected Request** | `GET /api/resources?category={category}&search={search}&page=1&pageSize=12` |
| **Expected Response** | `{ data: Resource[], total, categories: [] }` |
| **Notes** | Category chips: All Resources, Policy & Guidelines, Templates, Training & Guides, Research & Reports. Download triggers increment. |

---

### 7. Settings (`staffuser/settings.html` and `innovationteam/settings.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | User profile, notification prefs, appearance, export |
| **Current Data Source** | localStorage |
| **Future API Endpoint** | `GET/PUT /api/users/{id}/settings` |
| **Backend Service** | User Service |
| **Required Table** | Users, UserPreferences |
| **Expected Request** | `PUT /api/users/{id}/settings { phone, notificationPrefs, dashboardPrefs, language }` |
| **Expected Response** | `{ success: true }` |

---

### 8. Innovation Team Dashboard (`innovationteamdashboard.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | Show stats, pending submissions table |
| **Current Data Source** | Hardcoded stats (45, 12, 8, 5, 15, 5), hardcoded table (4 rows) |
| **Future API Endpoint** | `GET /api/dashboard/innovation-team` |
| **Backend Service** | Dashboard / Idea Service |
| **Required Table** | Ideas |
| **Expected Response** | `{ totalIdeas, pendingReview, inDevelopment, deployed, approved, declined, pendingSubmissions: [] }` |

---

### 9. Review Ideas (`innovationteam/reviewideas.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | Review, score, approve/decline submitted ideas |
| **Current Data Source** | Hardcoded `ideas` array (6 items) + localStorage merge |
| **Future API Endpoint** | `GET /api/ideas?status={status}&category={category}&search={search}` |
| **Backend Service** | Idea Service |
| **Required Table** | Ideas, IdeaReviews |
| **Expected Request** | `PUT /api/ideas/{id}/review { score, decision, feedback }` |
| **Expected Response** | `{ success: true, idea: Idea }` |
| **Notes** | Filter by status and category. Review modal with score (1-10), decision dropdown, feedback textarea. |

---

### 10. Manage Resources (`innovationteam/manageresources.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | CRUD operations on innovation resources |
| **Current Data Source** | Hardcoded `resources` array (8 items) |
| **Future API Endpoint** | `GET/POST/PUT/DELETE /api/resources` |
| **Backend Service** | Resource Service |
| **Required Table** | Resources, ResourceCategories |
| **Expected Request** | `POST /api/resources { title, category, description, type, size, status }` |
| **Expected Response** | `{ id, ...resource }` |
| **Notes** | Upload modal, edit via prompt, delete with confirm. Stats: total, published, drafts, downloads. |

---

### 11. Manage Categories (`innovationteam/managecategories.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | CRUD for idea categories |
| **Current Data Source** | Hardcoded `categories` array (5 items) |
| **Future API Endpoint** | `GET/POST/PUT/DELETE /api/categories` |
| **Backend Service** | Category Service |
| **Required Table** | Categories |
| **Expected Request** | `POST /api/categories { name, description, active }` |
| **Expected Response** | `{ id, ...category }` |

---

### 12. IT Admin Dashboard (`IT Admin/admindashboard.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | System overview, server status, resource health |
| **Current Data Source** | Hardcoded stats (99.98%, 1248, 14, 2), hardcoded system services (3 items), hardcoded resource health bars |
| **Future API Endpoint** | `GET /api/dashboard/admin` |
| **Backend Service** | Dashboard / System Service |
| **Required Table** | SystemMetrics (or computed) |
| **Expected Response** | `{ serverStatus, auditEvents24h, activeSessions, failedLogins, services: [], resourceHealth: {} }` |

---

### 13. Manage Users (`IT Admin/ITadmin.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | User CRUD, search, filter, pagination |
| **Current Data Source** | Hardcoded `users` array (6 items) |
| **Future API Endpoint** | `GET/POST/PUT/DELETE /api/users` |
| **Backend Service** | User Service |
| **Required Table** | Users |
| **Expected Request** | `GET /api/users?search=&role=all&status=all&page=1&pageSize=5` |
| **Expected Response** | `{ data: User[], total, page, pageSize, totalPages }` |

---

### 14. Manage Accounts (`IT Admin/manageaccounts.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | View/manage system API accounts |
| **Current Data Source** | Hardcoded `accounts` array (5 items) |
| **Future API Endpoint** | `GET /api/system-accounts` |
| **Backend Service** | System Service |
| **Required Table** | SystemAccounts |
| **Expected Request** | `GET /api/system-accounts` |
| **Expected Response** | `{ data: SystemAccount[] }` |

---

### 15. System Settings (`IT Admin/systemsettings.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | Admin profile, preferences, security settings |
| **Current Data Source** | localStorage |
| **Future API Endpoint** | `GET/PUT /api/admin/settings` |
| **Backend Service** | Admin Service |
| **Required Table** | SystemSettings |
| **Expected Response** | `{ success: true, settings: {} }` |

---

### 16. Audit Log (`IT Admin/auditlog.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | View, search, filter, export audit logs |
| **Current Data Source** | Hardcoded `logs` array (8 items) |
| **Future API Endpoint** | `GET /api/audit-logs` |
| **Backend Service** | Audit Service |
| **Required Table** | AuditLogs |
| **Expected Request** | `GET /api/audit-logs?search=&type=all&status=all&page=1&pageSize=5` |
| **Expected Response** | `{ data: AuditLog[], total, page, pageSize }` |

---

### 17. Reports & Analytics (`innovationteam/reportsandanalytics.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | KPI stats, charts, archived reports |
| **Current Data Source** | Hardcoded stats (38, 4.8, 31.5%, 145M), hardcoded bar chart, hardcoded pie chart, hardcoded reports table (3 rows) |
| **Future API Endpoint** | `GET /api/analytics/overview` |
| **Backend Service** | Analytics / Idea Service |
| **Required Table** | Ideas, IdeaReviews |
| **Expected Response** | `{ submissionRate, avgReviewSpeed, approvalRatio, budgetAllocated, ideasByCategory: [], archivedReports: [] }` |

---

### 18. Reports (`IT Admin/reports.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | IT admin security reports |

*Need to verify this page exists*

---

### 19. Support (`support.html`)

| Attribute | Value |
|-----------|-------|
| **Purpose** | FAQ, contact info, support ticket form |
| **Current Data Source** | Static HTML + FAQ toggle JS |
| **Future API Endpoint** | `POST /api/support/tickets` |
| **Backend Service** | Support Service |
| **Required Table** | SupportTickets |
| **Expected Request** | `{ name, email, category, subject, description, attachment }` |

---

## Summary of All Mock/Hardcoded Data Sources

| # | File | Data Type | Lines | Description |
|---|------|-----------|-------|-------------|
| 1 | `login.html` | Hardcoded object | 304-313 | `DEMO_USERS` with 8 user credentials |
| 2 | `staffdashboard.html` | Hardcoded numbers | 163-176 | 4 stat cards |
| 3 | `staffdashboard.html` | Hardcoded HTML | 185-207 | 2 idea cards |
| 4 | `staffuser/MyIdeas.html` | Hardcoded array | 149-156 | 6 ideas |
| 5 | `staffuser/notifications.html` | Hardcoded array | 143-149 | 5 notifications |
| 6 | `staffuser/resources.html` | Hardcoded array | 137-146 | 8 resources |
| 7 | `staffuser/settings.html` | Hardcoded values | 115-125 | Profile fields |
| 8 | `staffuser/submitinnovationideaform.html` | localStorage | 949-951 | Saves to `imts_ideas` |
| 9 | `innovationteamdashboard.html` | Hardcoded numbers | 110-151 | 6 stat cards |
| 10 | `innovationteamdashboard.html` | Hardcoded HTML | 172-199 | 4 table rows |
| 11 | `innovationteam/manageresources.html` | Hardcoded array | 221-230 | 8 resources |
| 12 | `innovationteam/managecategories.html` | Hardcoded array | 183-189 | 5 categories |
| 13 | `innovationteam/reviewideas.html` | Hardcoded array | 181-188 | 6 ideas + localStorage |
| 14 | `innovationteam/reportsandanalytics.html` | Hardcoded numbers/HTML | 104-123, 136-163, 137-215, 237-273 | Stats, chart, pie chart, table |
| 15 | `IT Admin/admindashboard.html` | Hardcoded numbers/HTML | 153-172, 182-203, 207-227 | Stats, services, resource health |
| 16 | `IT Admin/ITadmin.html` | Hardcoded array | 226-233 | 6 users |
| 17 | `IT Admin/manageaccounts.html` | Hardcoded array | 133-139 | 5 accounts |
| 18 | `IT Admin/systemsettings.html` | Hardcoded values | 113-125 | Profile fields |
| 19 | `IT Admin/auditlog.html` | Hardcoded array | 167-176 | 8 logs |
| 20 | `support.html` | Static HTML | 189-238 | FAQ content |

**Total mock data sources identified: 20**

---

## All Future API Endpoints Required

| Method | Endpoint | Service | Page |
|--------|----------|---------|------|
| POST | `/api/auth/login` | Auth | login.html |
| POST | `/api/auth/logout` | Auth | auth.js |
| GET | `/api/dashboard/staff` | Dashboard | staffdashboard.html |
| GET | `/api/dashboard/innovation-team` | Dashboard | innovationteamdashboard.html |
| GET | `/api/dashboard/admin` | Dashboard | admindashboard.html |
| GET | `/api/notifications` | Notification | notifications.html |
| GET | `/api/notifications/unread-count` | Notification | notifications.html |
| PUT | `/api/notifications/{id}/read` | Notification | notifications.html |
| PUT | `/api/notifications/read-all` | Notification | notifications.html |
| DELETE | `/api/notifications/{id}` | Notification | notifications.html |
| GET | `/api/resources` | Resource | resources.html, manageresources.html |
| POST | `/api/resources` | Resource | manageresources.html |
| PUT | `/api/resources/{id}` | Resource | manageresources.html |
| DELETE | `/api/resources/{id}` | Resource | manageresources.html |
| GET | `/api/resources/download/{id}` | Resource | resources.html |
| GET | `/api/categories` | Category | managecategories.html |
| POST | `/api/categories` | Category | managecategories.html |
| PUT | `/api/categories/{id}` | Category | managecategories.html |
| DELETE | `/api/categories/{id}` | Category | managecategories.html |
| GET | `/api/ideas` | Idea | MyIdeas.html, reviewideas.html |
| POST | `/api/ideas` | Idea | submitinnovationideaform.html |
| PUT | `/api/ideas/{id}` | Idea | MyIdeas.html |
| DELETE | `/api/ideas/{id}` | Idea | MyIdeas.html |
| PUT | `/api/ideas/{id}/review` | Idea | reviewideas.html |
| GET | `/api/users` | User | ITadmin.html |
| POST | `/api/users` | User | ITadmin.html |
| PUT | `/api/users/{id}` | User | ITadmin.html |
| DELETE | `/api/users/{id}` | User | ITadmin.html |
| PUT | `/api/users/{id}/lock` | User | ITadmin.html |
| PUT | `/api/users/{id}/reset-password` | User | ITadmin.html |
| GET | `/api/users/{id}/settings` | User | settings.html |
| PUT | `/api/users/{id}/settings` | User | settings.html |
| GET | `/api/audit-logs` | Audit | auditlog.html |
| GET | `/api/analytics/overview` | Analytics | reportsandanalytics.html |
| GET | `/api/system-accounts` | System | manageaccounts.html |
| POST | `/api/support/tickets` | Support | support.html |
| GET | `/api/email/templates` | Email | (internal) |

---

## Technology Stack (Frontend)

| Technology | Usage |
|------------|-------|
| **HTML5** | Page structure |
| **Tailwind CSS** (CDN) | Styling framework |
| **Material Symbols** (Google) | Icon library |
| **Public Sans** (Google Fonts) | Typography |
| **Vanilla JavaScript** | All interactivity |
| **localStorage** | Client-side data persistence |
| **Vite** | Build tool (configured but not heavily used) |

---

## Notes for Backend Implementation

1. **No JSON files used** - All mock data is hardcoded in arrays or stored in localStorage
2. **No fetch/axios calls exist** - The frontend has zero API calls currently
3. **All data must be served via REST API** - Every page needs API integration
4. **localStorage must be replaced** - Session, notifications, ideas, settings all use localStorage
5. **File uploads** - Idea attachments and resource uploads need multipart form data endpoints
6. **Download tracking** - Resource download counts need incrementing via API
7. **Notification badge** - Unread count must be fetched across all pages
8. **Pagination** - Users table (page size 5), Audit logs (page size 5), Ideas (page size TBD)
9. **Filtering** - Status filters, category filters, search filters needed on multiple pages
10. **Sorting** - Not explicitly implemented in frontend but should be supported
