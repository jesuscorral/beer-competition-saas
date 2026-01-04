# Complete MVP Issues List

**Generated:** 2025-12-19  
**Last Updated:** 2026-01-04  
**Total Issues:** 47 (13 completed, 34 to create)  
**MVP Timeline:** 12-16 weeks with 3-4 engineers

---

## ✅ Already Completed (13)

| Issue | Title | Priority | Complexity | Sprint | Status | Notes |
|-------|-------|----------|------------|--------|--------|-------|
| #6 | [INFRA-001] PostgreSQL Multi-Tenant Database with RLS | P0 | M | Sprint 0 | ✅ DONE | PostgreSQL 16, EF Core 10.0, migrations implemented |
| #2 | [INFRA-002] Docker Compose Development Environment | P0 | M | Sprint 0 | ✅ DONE | PostgreSQL, RabbitMQ, Keycloak running |
| #7 | [INFRA-003] Event Outbox Worker | P0 | M | Sprint 0 | ✅ DONE | Outbox pattern, CloudEvents 1.0 |
| #8 | [INFRA-004] RabbitMQ Event Bus Configuration | P0 | M | Sprint 0 | ✅ DONE | Dead Letter Queue, exchanges/queues |
| #3 | [AUTH-001] Keycloak OIDC Authentication Integration | P0 | L | Sprint 1 | ✅ DONE | OAuth 2.0 Token Exchange (RFC 8693) ⭐ |
| #9 | [AUTH-002] BFF Token Validation Middleware | P0 | S | Sprint 1 | ✅ DONE | YARP reverse proxy, token exchange |
| #10 | [AUTH-003] Role-Based Authorization Policies | P0 | S | Sprint 1 | ✅ DONE | 4 roles: Organizer, Judge, Entrant, Steward |
| #5 | [COMP-001] Competition Domain Models with Multi-Tenancy | P0 | S | Sprint 1 | ✅ DONE | Competition, Tenant, Organizer aggregates |
| #4 | [COMP-002] Competition CRUD REST API with CQRS | P0 | M | Sprint 1 | ✅ DONE | RegisterOrganizer feature, MediatR handlers |
| N/A | [ADR-008] Database Migrations Strategy | P0 | S | Sprint 0 | ✅ DONE | EF Core migrations documented |
| N/A | [ADR-010] Token Exchange Pattern | P0 | M | Sprint 1 | ✅ DONE | Service-specific audiences ⭐ |
| N/A | [TEST-001] Integration Testing Infrastructure | P0 | L | Sprint 1 | ✅ DONE | Testcontainers, Respawn, Builders ⭐ |
| N/A | [BFF-001] BFF API Gateway Implementation | P0 | L | Sprint 1 | ✅ DONE | YARP, OAuth 2.0 Token Exchange ⭐ |

**Key Implementations:**
- ⭐ **NEW**: OAuth 2.0 Token Exchange with service-specific audiences (BFF, Competition Service, Judging Service)
- ⭐ **NEW**: Integration testing infrastructure with Testcontainers, WebApplicationFactory, Respawn
- ⭐ **NEW**: Builder Pattern for test data (TenantBuilder, CompetitionBuilder)
- ⭐ **NEW**: TestTenantProvider for dynamic tenant context in tests

---

## 📋 To Create - Infrastructure & Foundation (3)

### [INFRA-005] BJCP 2021 Style Guidelines Database Seeding
- **Priority:** P0 | **Complexity:** S (1-2 days)
- **Agents:** @backend
- **Description:** Seed database with all BJCP 2021 categories, subcategories, and style guidelines
- **Acceptance Criteria:**
  - SQL script with 34 BJCP categories (1-34)
  - All subcategories (18A, 18B, etc.) with names, OG/FG ranges
  - Migration script that's idempotent
  - API endpoint to query styles by category/subcategory
- **Dependencies:** INFRA-001 (PostgreSQL setup)
- **Blocks:** Competition creation, Entry submission

### [OBS-001] Structured Logging with Serilog
- **Priority:** P0 | **Complexity:** S (1-2 days)
- **Agents:** @backend, @devops
- **Description:** Configure structured logging for all services
- **Acceptance Criteria:**
  - Serilog configured with JSON format
  - Log correlation IDs for distributed tracing
  - Log sinks: Console (dev), Seq (dev/staging), Azure Monitor (prod)
  - Tenant ID and User ID logged in all requests
  - Sensitive data (PII) excluded from logs
- **Dependencies:** INFRA-002 (Docker Compose)

### [OBS-004] Health Checks & Readiness Probes
- **Priority:** P0 | **Complexity:** S (1-2 days)
- **Agents:** @backend, @devops
- **Description:** Implement health check endpoints for all services
- **Acceptance Criteria:**
  - `/health/live` (liveness probe) - service is running
  - `/health/ready` (readiness probe) - DB, RabbitMQ, Redis checks
  - Health checks run every 10s with 3s timeout
  - Unhealthy services restart automatically
- **Dependencies:** INFRA-001, INFRA-004

---

## 📋 To Create - Competitions & Entries (5 P0, 2 P1)

### [COMP-003] Competition Status Transition Workflow
- **Priority:** P0 | **Complexity:** S (2-3 days)
- **Agents:** @backend
- **Description:** Implement state machine for competition lifecycle
- **Acceptance Criteria:**
  - Status transitions: Draft → Registration → Closed → Judging → Complete
  - Only organizers can transition status
  - Validation rules (can't close if no entries, can't judge without flights)
  - Events published for each transition
  - Entry submissions only allowed in Registration status
- **Dependencies:** COMP-002
- **Blocks:** Entry submission

### [COMP-004] BJCP Style Guidelines Integration
- **Priority:** P0 | **Complexity:** M (3-4 days)
- **Agents:** @backend
- **Description:** API endpoints to query and validate BJCP styles
- **Acceptance Criteria:**
  - `GET /api/styles` - list all categories
  - `GET /api/styles/{categoryId}` - get subcategories
  - `GET /api/styles/{categoryId}/{subcategoryId}` - get style details
  - Validation logic for OG/FG/IBU/SRM ranges
  - Frontend dropdown integration
- **Dependencies:** INFRA-005
- **Blocks:** Entry submission UI

### [ENTRY-001] Entry Submission API with Payment
- **Priority:** P0 | **Complexity:** M (4-5 days)
- **Agents:** @backend
- **Description:** Entrant submits entry with beer details and payment
- **Acceptance Criteria:**
  - `POST /api/entries` creates entry with beer name, style, ingredients
  - System assigns sequential judging_number per competition
  - Entry status: Draft → Submitted → Paid
  - Events published: entry.submitted, entry.paid
  - Multi-tenancy enforced (entrant's tenant only)
- **Dependencies:** COMP-003, COMP-004, AUTH-003
- **Blocks:** Bottle check-in

### [ENTRY-002] Entry Label PDF Generation
- **Priority:** P0 | **Complexity:** M (3-4 days)
- **Agents:** @backend
- **Description:** Generate printable labels with judging number (blind)
- **Acceptance Criteria:**
  - `GET /api/entries/{id}/label` returns PDF
  - Label shows: judging_number, category/subcategory, QR code
  - NO brewer information (blind judging)
  - QR code encodes judging_number for scanning
  - Labels print on Avery 5160 format (30 labels/sheet)
- **Dependencies:** ENTRY-001
- **Blocks:** Bottle check-in

### [ENTRY-003] Bottle Check-In System
- **Priority:** P0 | **Complexity:** M (4-5 days)
- **Agents:** @backend, @frontend
- **Description:** Stewards scan/enter judging numbers to check in bottles
- **Acceptance Criteria:**
  - `POST /api/entries/{judgingNumber}/check-in` endpoint
  - QR code scanner integration (frontend)
  - Entry status changes to CheckedIn with timestamp
  - Validation: entry must be Paid status
  - Event published: bottle.checked_in
  - Dashboard shows check-in count per category
- **Dependencies:** ENTRY-002, AUTH-003 (steward role)
- **Blocks:** Flight assignment

### [COMP-005] Entry Fee & Payment Tracking
- **Priority:** P1 | **Complexity:** M (4-5 days)
- **Agents:** @backend
- **Description:** Track entry fees and payment status per entry
- **Acceptance Criteria:**
  - Competition has configurable `entry_fee` field
  - Entry tracks `payment_status`, `payment_method`, `paid_at`
  - Manual payment option for cash/check entries
  - Payment report for organizers
- **Dependencies:** ENTRY-001

### [COMP-006] Competition Settings & Configuration
- **Priority:** P1 | **Complexity:** S (2-3 days)
- **Agents:** @backend
- **Description:** Organizer configures competition rules and limits
- **Acceptance Criteria:**
  - Max entries per entrant
  - Entry fee amount
  - Judges per flight (min/max)
  - Mini-BOS enabled/disabled
  - Notification preferences
- **Dependencies:** COMP-002

---

## 📋 To Create - Flight Management (4 P0, 2 P1)

### [FLIGHT-001] Flight Creation & Auto-Assignment Algorithm
- **Priority:** P0 | **Complexity:** L (6-8 days)
- **Agents:** @backend
- **Description:** Automatically assign checked-in entries to flights
- **Acceptance Criteria:**
  - Group entries by category/subcategory
  - Create flights with 6-10 entries each (configurable)
  - Assign sequential flight numbers (Flight 1, Flight 2, etc.)
  - Algorithm balances flight sizes
  - Event published: flight.created, entry.assigned_to_flight
  - Can regenerate flights if needed (before judging starts)
- **Dependencies:** ENTRY-003
- **Blocks:** Judge assignment

### [FLIGHT-002] Judge Assignment to Flights
- **Priority:** P0 | **Complexity:** M (4-5 days)
- **Agents:** @backend
- **Description:** Assign judges to flights with COI detection
- **Acceptance Criteria:**
  - Organizer assigns 3+ judges per flight
  - System detects COI (judge is entry owner)
  - Judge can't be assigned to flight with their own entries
  - Event published: judge.assigned_to_flight
  - Judges can view assigned flights
- **Dependencies:** FLIGHT-001, FLIGHT-004 (COI detection)
- **Blocks:** Scoresheet entry

### [FLIGHT-004] Conflict of Interest (COI) Detection
- **Priority:** P0 | **Complexity:** M (3-4 days)
- **Agents:** @backend
- **Description:** Detect and prevent judges scoring their own entries
- **Acceptance Criteria:**
  - Algorithm identifies judge's entries across all categories
  - Warns organizer during judge assignment
  - Prevents judge from opening scoresheets for their entries (frontend)
  - COI flagged in database
- **Dependencies:** ENTRY-001, AUTH-001
- **Blocks:** FLIGHT-002

### [FLIGHT-003] Steward Assignment & Management
- **Priority:** P1 | **Complexity:** S (2-3 days)
- **Agents:** @backend
- **Description:** Assign stewards to flights for logistics
- **Acceptance Criteria:**
  - Steward role assignment
  - Steward sees assigned flights
  - Steward delivers bottles to judges
  - Steward collects completed scoresheets
- **Dependencies:** FLIGHT-002

### [FLIGHT-005] Flight Scheduling & Timing
- **Priority:** P1 | **Complexity:** S (2-3 days)
- **Agents:** @backend, @frontend
- **Description:** Schedule flights with start times and locations
- **Acceptance Criteria:**
  - Organizer sets flight start_time and location
  - Judges notified of schedule
  - Dashboard shows flight timeline
- **Dependencies:** FLIGHT-002

### [FLIGHT-006] Flight Balancing Algorithm
- **Priority:** P1 | **Complexity:** M (4-5 days)
- **Agents:** @backend
- **Description:** Optimize flight sizes and judge workload
- **Acceptance Criteria:**
  - Even distribution of entries across flights
  - Balance judge workload (entries per judge)
  - Minimize number of flights while staying within size limits
- **Dependencies:** FLIGHT-001

---

## 📋 To Create - Scoring & Judging (4 P0, 2 P1)

### [SCORE-001] Offline PWA Scoresheet Entry UI
- **Priority:** P0 | **Complexity:** XL (10-12 days)
- **Agents:** @frontend
- **Description:** Progressive Web App for judges to enter scoresheets offline
- **Acceptance Criteria:**
  - PWA installable on tablets (iOS/Android)
  - Works offline with Service Workers
  - BJCP scoresheet format (Aroma, Appearance, Flavor, Mouthfeel, Overall)
  - Score fields (0-50) with validation
  - Comments/notes for each section
  - Overall score auto-calculated (max 50)
  - Draft scoresheets saved to IndexedDB
  - Sync to backend when online
- **Dependencies:** FLIGHT-002, UI-005
- **Blocks:** SCORE-002 (sync)

### [SCORE-002] Scoresheet Sync & Conflict Resolution
- **Priority:** P0 | **Complexity:** L (6-8 days)
- **Agents:** @backend, @frontend
- **Description:** Sync offline scoresheets to backend with conflict handling
- **Acceptance Criteria:**
  - Background sync from IndexedDB to API
  - Detect conflicts (same scoresheet edited by multiple judges)
  - Conflict resolution: last-write-wins with timestamp
  - Retry failed syncs with exponential backoff
  - Event published: scoresheet.submitted
  - UI shows sync status (pending, synced, failed)
- **Dependencies:** SCORE-001
- **Blocks:** Mini-BOS calculation

### [SCORE-003] BJCP Scoresheet Validation
- **Priority:** P0 | **Complexity:** M (3-4 days)
- **Agents:** @backend, @frontend
- **Description:** Validate scoresheet scores and completeness
- **Acceptance Criteria:**
  - Aroma (0-12), Appearance (0-3), Flavor (0-20), Mouthfeel (0-5), Overall (0-10)
  - Total score = sum of sections (max 50)
  - Required fields: All scores, overall impression comment
  - Frontend validation before submission
  - Backend validation on API
- **Dependencies:** SCORE-001

### [SCORE-004] Mini-BOS (Best of Show) Calculation
- **Priority:** P1 | **Complexity:** M (4-5 days)
- **Agents:** @backend
- **Description:** Calculate flight winners (Mini-BOS) from scoresheets
- **Acceptance Criteria:**
  - For each flight, rank entries by average score
  - Top 1-3 entries advance to BOS round
  - Handle ties with tiebreaker rules
  - Organizer can override Mini-BOS selections
  - Event published: mini_bos.calculated
- **Dependencies:** SCORE-002

### [SCORE-005] Judge Notes & Comments
- **Priority:** P1 | **Complexity:** S (2-3 days)
- **Agents:** @backend, @frontend
- **Description:** Rich text comments on scoresheets
- **Acceptance Criteria:**
  - Markdown support for formatting
  - Character limit per section (500 chars)
  - Spell check enabled
- **Dependencies:** SCORE-001

### [SCORE-006] Scoresheet PDF Export
- **Priority:** P1 | **Complexity:** M (3-4 days)
- **Agents:** @backend
- **Description:** Generate PDF of completed scoresheets for entrants
- **Acceptance Criteria:**
  - `GET /api/scoresheets/{id}/pdf` endpoint
  - BJCP standard scoresheet format
  - All scores and comments included
  - Judge names visible (after competition ends)
- **Dependencies:** SCORE-002

---

## 📋 To Create - Best of Show (4 P0, 3 P1)

### [BOS-001] BOS Round Management
- **Priority:** P0 | **Complexity:** M (4-5 days)
- **Agents:** @backend
- **Description:** Manage Best of Show final judging round
- **Acceptance Criteria:**
  - BOS round created after all Mini-BOS completed
  - Entries advanced from Mini-BOS auto-added to BOS table
  - Organizer can manually add/remove BOS entries
  - BOS status: Pending → InProgress → Complete
- **Dependencies:** SCORE-004
- **Blocks:** BOS judging

### [BOS-002] BOS Judge Selection
- **Priority:** P0 | **Complexity:** S (2-3 days)
- **Agents:** @backend
- **Description:** Assign experienced judges to BOS round
- **Acceptance Criteria:**
  - Organizer selects 5-7 judges for BOS
  - Prefer judges with BJCP rank ≥ Certified
  - COI detection for BOS judges
  - Event published: bos_judge.assigned
- **Dependencies:** BOS-001, FLIGHT-004 (COI)

### [BOS-003] BOS Scoresheet Entry
- **Priority:** P0 | **Complexity:** M (4-5 days)
- **Agents:** @frontend, @backend
- **Description:** Simplified scoresheet for BOS round
- **Acceptance Criteria:**
  - Judges rank BOS entries (1st, 2nd, 3rd)
  - Optional scores (not required for BOS)
  - Comments for top 3 entries
  - Real-time submission (not offline)
- **Dependencies:** BOS-002, SCORE-001 (UI pattern)

### [BOS-004] BOS Winner Calculation
- **Priority:** P0 | **Complexity:** S (2-3 days)
- **Agents:** @backend
- **Description:** Calculate BOS winners from judge rankings
- **Acceptance Criteria:**
  - Aggregate judge rankings (weighted voting)
  - Determine 1st, 2nd, 3rd place
  - Handle ties with organizer override
  - Event published: bos.winner_announced
- **Dependencies:** BOS-003

### [BOS-005] Awards & Medals Assignment
- **Priority:** P1 | **Complexity:** M (3-4 days)
- **Agents:** @backend
- **Description:** Assign gold/silver/bronze medals to category winners
- **Acceptance Criteria:**
  - Gold: Score ≥ 45
  - Silver: Score 38-44
  - Bronze: Score 30-37
  - Medals assigned per category/subcategory
  - BOS winner gets special award
- **Dependencies:** BOS-004

### [BOS-006] Winners Notification System
- **Priority:** P1 | **Complexity:** S (2-3 days)
- **Agents:** @backend
- **Description:** Email notifications to winners
- **Acceptance Criteria:**
  - Automated email to BOS winners
  - Automated email to medal winners
  - Email template with competition name, place, category
  - Batch send after results published
- **Dependencies:** BOS-005

### [BOS-007] Results Export (CSV/PDF)
- **Priority:** P1 | **Complexity:** M (3-4 days)
- **Agents:** @backend
- **Description:** Export competition results for publishing
- **Acceptance Criteria:**
  - CSV export: All entries with scores, medals
  - PDF results book: Winners by category, BOS results
  - Include judge names and scores
  - Organizer download from dashboard
- **Dependencies:** BOS-004, BOS-005

---

## 📋 To Create - Frontend UI (6 P0)

### [UI-001] Login & Authentication UI
- **Priority:** P0 | **Complexity:** M (4-5 days)
- **Agents:** @frontend
- **Description:** User login, registration, and profile management
- **Acceptance Criteria:**
  - Login form with Keycloak redirect
  - Registration form (email, password, role selection)
  - Profile page (view/edit name, email, BJCP rank)
  - Logout functionality
  - Responsive mobile-first design
  - WCAG 2.1 AA accessibility
- **Dependencies:** AUTH-001
- **Blocks:** All authenticated UI

### [UI-002] Competition Dashboard (Organizer)
- **Priority:** P0 | **Complexity:** L (6-8 days)
- **Agents:** @frontend
- **Description:** Organizer dashboard to manage competitions
- **Acceptance Criteria:**
  - List all competitions (create new, edit, view stats)
  - Competition detail view: Entries count, check-in status, flight assignments
  - Status transition buttons (Open Registration → Close → Start Judging → Complete)
  - Real-time updates via WebSocket
  - Charts: Entries by category, check-in progress
  - Export buttons (labels, results)
- **Dependencies:** UI-001, COMP-002
- **Blocks:** Organizer workflows

### [UI-003] Entry Registration Form
- **Priority:** P0 | **Complexity:** M (4-5 days)
- **Agents:** @frontend
- **Description:** Entrant submits entries for competition
- **Acceptance Criteria:**
  - Multi-step form: Select competition → Enter beer details → Payment
  - BJCP style dropdown (category → subcategory)
  - Beer name, ingredients, special notes fields
  - Entry fee display and payment integration
  - Confirmation screen with entry number
  - Mobile-responsive
- **Dependencies:** UI-001, ENTRY-001, COMP-004
- **Blocks:** Entry submission

### [UI-004] Bottle Check-In Interface (Steward)
- **Priority:** P0 | **Complexity:** M (4-5 days)
- **Agents:** @frontend
- **Description:** Steward interface for checking in bottles
- **Acceptance Criteria:**
  - QR code scanner (camera or USB scanner)
  - Manual entry field for judging number
  - Real-time validation (entry exists, not already checked in)
  - Check-in confirmation with audio/visual feedback
  - Check-in log showing recent scans
  - Works on tablets (iPad, Android)
- **Dependencies:** UI-001, ENTRY-003
- **Blocks:** Bottle management

### [UI-005] Judge Dashboard & Flight List
- **Priority:** P0 | **Complexity:** M (4-5 days)
- **Agents:** @frontend
- **Description:** Judge views assigned flights and enters scoresheets
- **Acceptance Criteria:**
  - List of assigned flights with status
  - Flight details: Entries (by judging number only), judges, steward
  - Button to start scoresheet entry
  - Scoresheet list (completed, in-progress, pending)
  - Offline indicator showing sync status
- **Dependencies:** UI-001, FLIGHT-002
- **Blocks:** SCORE-001 (scoresheet UI)

### [UI-006] Results Publishing Page
- **Priority:** P1 | **Complexity:** M (4-5 days)
- **Agents:** @frontend
- **Description:** Public results page after competition
- **Acceptance Criteria:**
  - List winners by category (gold/silver/bronze)
  - BOS winners prominently displayed
  - Entrant can view their scoresheets (PDF download)
  - Public URL shareable (e.g., /competitions/{id}/results)
  - Mobile-responsive
- **Dependencies:** BOS-007

---

## 📊 Issue Summary

| Epic | P0 Count | P1 Count | P2 Count | Total |
|------|----------|----------|----------|-------|
| Infrastructure | 5 | 1 | 0 | 6 |
| Authentication | 3 | 2 | 0 | 5 |
| Competitions | 5 | 2 | 0 | 7 |
| Entry Management | 3 | 1 | 0 | 4 |
| Flight Management | 3 | 3 | 0 | 6 |
| Scoring & Judging | 3 | 3 | 0 | 6 |
| Best of Show | 4 | 3 | 0 | 7 |
| Frontend UI | 5 | 1 | 0 | 6 |
| **Total** | **31** | **16** | **0** | **47** |

**MVP Scope:** 31 P0 issues (critical blockers)  
**Full MVP:** 47 issues (P0 + P1)  
**Estimated Timeline:** 12-16 weeks with 3-4 engineers

