# Product Backlog

## Overview

This document contains a prioritized list of tasks and issues grouped by epic for the Beer Competition SaaS MVP. Each item includes a brief description, T-shirt size estimate (S/M/L), and priority (P0=Critical, P1=High, P2=Medium).

**Legend**:
- **S**: Small (1-3 days)
- **M**: Medium (4-7 days)
- **L**: Large (8-15 days)
- **P0**: Critical for MVP launch
- **P1**: High priority, needed for full MVP
- **P2**: Nice-to-have, can be deferred post-MVP

---

## Epic 1: Platform Infrastructure

**Goal**: Establish foundational infrastructure, multi-tenancy, and core database schema.

- [ ] **INFRA-001**: Set up PostgreSQL database with multi-tenant schema and RLS policies  
  _Size_: **M** | _Priority_: **P0**  
  _Description_: Create Tenants, Users, Competitions, Entries, Styles tables with `tenant_id` on all tables; configure Row-Level Security.

- [ ] **INFRA-002**: Configure Docker Compose for local development (PostgreSQL, RabbitMQ, Redis, Keycloak, 2 core services)
  _Size_: **M** | _Priority_: **P0**
  _Description_: Define `docker-compose.yml` with all infrastructure services (Competition Service, Judging Service); test service connectivity.

- [ ] **INFRA-003**: Seed BJCP 2021 styles into database  
  _Size_: **S** | _Priority_: **P0**  
  _Description_: Create SQL script to insert all BJCP 2021 styles (categories, subcategories) into Styles table.

- [ ] **INFRA-004**: Set up RabbitMQ exchanges and queues with durable configuration  
  _Size_: **S** | _Priority_: **P0**  
  _Description_: Define topic exchange; create queues for key events; configure DLQ.

- [ ] **INFRA-005**: Configure OpenTelemetry Collector and integrate with Azure Monitor  
  _Size_: **M** | _Priority_: **P1**  
  _Description_: Deploy OTEL Collector container; configure trace/metric export to Azure Application Insights.

---

## Epic 2: Authentication & User Management

**Goal**: Implement secure authentication with Keycloak and role-based access control.

- [ ] **AUTH-001**: Integrate Keycloak with BFF (OAuth2 Authorization Code flow)  
  _Size_: **M** | _Priority_: **P0**  
  _Description_: Configure Keycloak realm, clients; implement BFF middleware for token validation, session management.

- [ ] **AUTH-002**: Implement user registration and profile management  
  _Size_: **S** | _Priority_: **P0**  
  _Description_: User can register via Keycloak; sync user to PostgreSQL Users table with roles (ENTRANT, JUDGE, ORGANIZER, STEWARD).

- [ ] **AUTH-003**: Implement role-based authorization in BFF  
  _Size_: **M** | _Priority_: **P0**  
  _Description_: BFF enforces role constraints on endpoints (e.g., only ORGANIZER can create competitions); reject unauthorized requests with 403.

- [ ] **AUTH-004**: Add BJCP rank field to judge profiles  
  _Size_: **S** | _Priority_: **P1**  
  _Description_: Extend Users table with `bjcp_rank` enum; UI for judges to select rank during profile setup.

- [ ] **AUTH-005**: Implement tenant isolation and context injection  
  _Size_: **M** | _Priority_: **P0**  
  _Description_: BFF extracts `tenant_id` from Keycloak token; injects `X-Tenant-ID` header to all downstream service calls.

---

## Epic 3: Competitions & Entries

**Goal**: Enable organizers to create competitions and entrants to register entries with payment.

- [ ] **COMP-001**: Implement competition CRUD (Create, Read, Update, Delete)  
  _Size_: **M** | _Priority_: **P0**  
  _Description_: Competition Service API endpoints for competition management; enforce status transitions (DRAFT → OPEN → JUDGING → COMPLETE).
  _Size_: **M** | _Priority_: **P0**  
  _Description_: Entrant submits entry with style, beer name, ingredients; system generates `entry_id` (UUID) and sequential `judging_number`.

- [ ] **COMP-003**: Integrate Stripe for entry payment  
  _Size_: **L** | _Priority_: **P0**  
  _Description_: Create Stripe Checkout session on entry submission; handle webhook for payment confirmation; update `payment_status` to PAID.

- [ ] **COMP-004**: Generate PDF entry labels with judging number and QR code  
  _Size_: **M** | _Priority_: **P0**  
  _Description_: Use library (e.g., PDFSharp, ReportLab) to generate printable label with judging number (no brewer info); include optional QR code.

- [ ] **COMP-005**: Implement entry withdrawal and refund workflow  
  _Size_: **M** | _Priority_: **P2**  
  _Description_: Entrant can withdraw entry before deadline; organizer processes manual refund; update `payment_status` to REFUNDED.

- [ ] **COMP-006**: Add entry editing before registration closes  
  _Size_: **S** | _Priority_: **P1**  
  _Description_: Entrant can edit beer name, style, ingredients (only if competition status = OPEN).

---

## Epic 4: Cellar & Logistics

- [ ] **COMP-007**: Implement bottle check-in API (scan/enter judging number)
  _Size_: **M** | _Priority_: **P0**
  _Description_: Competition Service API to check in bottle; update `Entries.bottle_status` to RECEIVED; publish `bottles.checked_in` event.

- [ ] **COMP-008**: Add bottle condition tracking (GOOD, DAMAGED, LEAKED)
  _Size_: **S** | _Priority_: **P0**
  _Description_: Steward selects condition on check-in; store in `BottleReception` table; alert organizer if DAMAGED.

- [ ] **COMP-009**: Build steward UI for bottle check-in (mobile-first)
  _Size_: **M** | _Priority_: **P0**
  _Description_: React page with QR scanner (or manual entry); display entry details (no brewer name); one-click check-in.

- [ ] **COMP-010**: Implement bottle status dashboard for organizers
  _Size_: **M** | _Priority_: **P1**
  _Description_: List view of all entries with filters (received, not received, defective); export as CSV.

---

## Epic 4: Flights & Judge Assignment

**Goal**: Enable organizers to create flights and assign judges with conflict validation.

- [ ] **FLIGHT-001**: Implement flight CRUD (Create, Read, Update, Delete)  
  _Size_: **M** | _Priority_: **P0**  
  _Description_: Judging Service API for flight management; enforce max 10 entries per flight.

- [ ] **FLIGHT-002**: Implement entry-to-flight assignment with randomized tasting order  
  _Size_: **M** | _Priority_: **P0**  
  _Description_: Organizer assigns entries to flight; system randomizes initial `tasting_order`; only allow PAID + RECEIVED entries.

- [ ] **FLIGHT-003**: Implement judge assignment with conflict-of-interest validation  
  _Size_: **M** | _Priority_: **P0**  
  _Description_: Before assigning judge to flight, check if judge has entry in competition; reject with 409 if conflict exists.

- [ ] **FLIGHT-004**: Build organizer UI for flight management (drag-and-drop entry assignment)  
  _Size_: **L** | _Priority_: **P1**  
  _Description_: React UI with drag-and-drop to assign entries to flights; visual feedback for max entries, conflicts.

- [ ] **FLIGHT-005**: Implement tasting order reordering for judges  
  _Size_: **S** | _Priority_: **P1**  
  _Description_: Judge can reorder entries within flight during session (drag-and-drop); update `tasting_order` in DB.

- [ ] **FLIGHT-006**: Add flight scheduling and judge notifications  
  _Size_: **M** | _Priority_: **P1**  
  _Description_: Organizer sets flight scheduled time; system sends email/in-app notification to assigned judges with details.

---

## Epic 5: Scoring & Judging

**Goal**: Enable judges to score entries on mobile devices with offline capability.

- [ ] **SCORE-001**: Build scoresheet form UI (mobile-first, BJCP fields)  
  _Size_: **M** | _Priority_: **P0**  
  _Description_: React form with Aroma (0-12), Appearance (0-3), Flavor (0-20), Mouthfeel (0-5), Overall (0-10), text notes; real-time validation.

- [ ] **SCORE-002**: Implement scoresheet submission API with validation  
  _Size_: **M** | _Priority_: **P0**  
  _Description_: Judging Service validates field maxima and total ≤ 50; reject with 400 if invalid; save to Scoresheets table; publish event.
  _Size_: **L** | _Priority_: **P0**  
  _Description_: Service Worker caches scoresheets in IndexedDB when offline; background sync or manual retry when connectivity restored.

- [ ] **SCORE-004**: Implement consensus score calculation and UI  
  _Size_: **M** | _Priority_: **P0**  
  _Description_: Display average scores from all judges; head judge enters final consensus score; mark one scoresheet as `is_consensus=true`.

- [ ] **SCORE-005**: Add scoresheet editing before flight closed  
  _Size_: **S** | _Priority_: **P1**  
  _Description_: Judge can edit own scoresheet before flight status = CLOSED; track `edited_at` timestamp.

- [ ] **SCORE-006**: Build judge flight dashboard (list assigned flights, entries, scoring progress)  
  _Size_: **M** | _Priority_: **P1**  
  _Description_: Judge sees list of flights; click to view entries (judging numbers); track which entries scored.

---

## Epic 6: Best of Show & Results

**Goal**: Enable BOS candidate marking, BOS judging, and results publication.

- [ ] **BOS-001**: Implement BOS candidate marking during judging  
  _Size_: **S** | _Priority_: **P0**  
  _Description_: Judge clicks button on scoresheet to mark entry as BOS candidate; create `BOSCandidates` record; publish event.

- [ ] **BOS-002**: Build BOS flight creation and judge assignment  
  _Size_: **M** | _Priority_: **P0**  
  _Description_: Organizer creates BOS flight with variable judges; assign candidates; BOS judges rank entries.

- [ ] **BOS-003**: Implement BOS ranking aggregation (Borda count or IRV)  
  _Size_: **M** | _Priority_: **P0**  
  _Description_: Aggregate BOS judge rankings; calculate final `bos_rank` (1st, 2nd, 3rd); update `BOSCandidates` table.

- [ ] **BOS-004**: Implement results publication workflow  
  _Size_: **M** | _Priority_: **P0**  
  _Description_: Organizer clicks "Publish Results"; status → COMPLETE; send emails to entrants; make public results page visible.

- [ ] **BOS-005**: Build entrant results view (scores, feedback, placement)  
  _Size_: **M** | _Priority_: **P0**  
  _Description_: Entrant sees consensus score, judge notes, category placement, BOS rank (if applicable).

- [ ] **BOS-006**: Build public results leaderboard (anonymized)  
  _Size_: **M** | _Priority_: **P1**  
  _Description_: Public page with entries sorted by score; show entry ID (anonymized), style, score; no brewer names until entrant logs in.

- [ ] **BOS-007**: Implement results export (CSV, PDF)  
  _Size_: **M** | _Priority_: **P1**  
  _Description_: Organizer can download full results report; include all entries, scores, feedback, placements.

---

## Epic 7: Observability & CI/CD

**Goal**: Establish monitoring, logging, and automated deployment pipeline.

- [ ] **OBS-001**: Instrument all services with OpenTelemetry (traces, metrics)  
  _Size_: **M** | _Priority_: **P0**  
  _Description_: Add OTEL SDK to .NET and Python services; propagate trace context across service boundaries and event bus.

- [ ] **OBS-002**: Configure Azure Monitor dashboards and alerts  
  _Size_: **M** | _Priority_: **P1**  
  _Description_: Create dashboards for request rate, latency (p50/p95/p99), error rate, DB performance; set alerts for high error rate, latency spikes.

- [ ] **OBS-003**: Implement structured logging with correlation IDs  
  _Size_: **S** | _Priority_: **P1**  
  _Description_: All services log in JSON format with `trace_id`, `span_id`, `tenant_id`, `user_id`; export to Azure Monitor.

- [ ] **OBS-004**: Set up GitHub Actions CI/CD pipeline  
  _Size_: **L** | _Priority_: **P0**  
  _Description_: Build workflow: test → build Docker images → push to ACR → run migrations → deploy to Azure; trigger on push to `main`.

- [ ] **OBS-005**: Add automated integration tests to CI pipeline  
  _Size_: **M** | _Priority_: **P1**  
  _Description_: Run integration tests for each service in CI; fail pipeline if coverage < 70% or tests fail.

- [ ] **OBS-006**: Implement health checks for all services  
  _Size_: **S** | _Priority_: **P1**  
  _Description_: Each service exposes `/health` and `/ready` endpoints; Docker Compose and Azure health probes monitor service status.

---

## Epic 8: Analytics & ML (Post-MVP)

**Goal**: Provide advanced analytics, ML-driven insights, and intelligent judge assignment suggestions.

- [ ] **ANALYTICS-001**: Set up Analytics Service infrastructure (Python FastAPI, pandas, scikit-learn)
  _Size_: **M** | _Priority_: **P2**
  _Description_: Create Analytics Service with FastAPI; configure RabbitMQ consumers to aggregate all events into analytics database.

- [ ] **ANALYTICS-002**: Implement judge assignment suggestion algorithm (ML-based)
  _Size_: **L** | _Priority_: **P2**
  _Description_: Train ML model on historical judge assignments, BJCP ranks, conflict data; recommend optimal judge-flight pairings.

- [ ] **ANALYTICS-003**: Build anomaly detection for scoresheet validation
  _Size_: **M** | _Priority_: **P2**
  _Description_: Detect outlier scores (e.g., one judge scores 15 points higher than others); flag for organizer review.

- [ ] **ANALYTICS-004**: Implement advanced reporting dashboards
  _Size_: **L** | _Priority_: **P2**
  _Description_: Competition-level analytics: average scores by style, judge performance metrics, entry distribution, historical trends.

- [ ] **ANALYTICS-005**: Add predictive analytics for competition planning
  _Size_: **M** | _Priority_: **P2**
  _Description_: Predict number of flights needed, optimal judging schedule, entry volume forecasting based on historical data.

- [ ] **ANALYTICS-006**: Implement judge performance insights
  _Size_: **M** | _Priority_: **P2**
  _Description_: Track judge scoring patterns, consistency, calibration with peers; provide feedback to organizers and judges.

- [ ] **ANALYTICS-007**: Build data export API for external BI tools
  _Size_: **S** | _Priority_: **P2**
  _Description_: REST API to export competition data in formats compatible with Tableau, Power BI, or Looker.

---

## Summary Statistics

**Total Tasks**: 54 (47 MVP + 7 Post-MVP)
**Critical (P0)**: 25
**High (P1)**: 19
**Medium (P2)**: 10

**Estimated Effort**:
- **Small (S)**: 9 tasks (~27 days)
- **Medium (M)**: 32 tasks (~192 days)
- **Large (L)**: 6 tasks (~75 days)
- **Total**: ~294 person-days (~14-16 weeks for team of 3-4 engineers)

---

## Task Assignment Strategy

**Team Structure** (recommended):
- **Engineer 1**: Backend (Competition Service) + Event Bus
- **Engineer 2**: Backend (Judging Service, BFF, Keycloak) + Infra
- **Engineer 3**: Frontend (React UI, PWA, Offline Sync) + UX
- **Engineer 4** (optional): QA/DevOps (CI/CD, Testing, Observability)
- **Engineer 5** (Post-MVP): Data Science (Analytics Service, ML models)

**Sprint Planning**: 2-week sprints; prioritize P0 tasks first, then P1; defer P2 to post-MVP.

---

**Document Version**: 1.0  
**Last Updated**: 2025-12-18
