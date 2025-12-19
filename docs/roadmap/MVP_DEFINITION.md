# MVP Definition & Acceptance Criteria

## Overview

This document defines the Minimum Viable Product (MVP) scope for the Beer Competition SaaS platform. The MVP focuses on delivering core competition management functionality with BJCP 2021 compliance, blind judging, and conflict-of-interest enforcement. Target completion: **12-16 weeks** for a team of 3-4 engineers.

---

## MVP Scope (Version 1.0)

### 1. **Competition Creation & Management**

**Description**: Organizers can create and configure competitions with registration deadlines, bottle submission windows, and judging schedules.

**Acceptance Criteria**:
- **AC1.1**: Organizer can create a new competition with name, description, registration deadline, bottle submission dates, and judging start date.
- **AC1.2**: Competition status transitions: DRAFT → OPEN → JUDGING → COMPLETE. Organizer can manually trigger status changes.
- **AC1.3**: Only competitions in OPEN status accept entry registrations; DRAFT competitions are hidden from entrants.

---

### 2. **Entry Registration & Payment**

**Description**: Homebrewers (entrants) can register beer entries with BJCP style selection, pay entry fees, and receive confirmation with entry ID.

**Acceptance Criteria**:
- **AC2.1**: Entrant can submit entry with beer name, BJCP style (from 2021 catalog), special ingredients (optional).
- **AC2.2**: System generates unique public `entry_id` (UUID) and internal `judging_number` (sequential integer per competition).
- **AC2.3**: Entry status = PENDING until payment confirmed; payment via Stripe integration (initiate checkout session).
- **AC2.4**: On payment success (webhook), entry status → PAID; entrant receives confirmation email with entry ID and label download link.
- **AC2.5**: Entrant can view all their entries for a competition with payment/bottle status.

---

### 3. **Label Generation**

**Description**: Entrants download printable labels with judging number only (no brewer identity) for bottle submission.

**Acceptance Criteria**:
- **AC3.1**: After payment, entrant can download PDF label for entry containing:
  - Competition name
  - Judging number (large, prominent font)
  - Optional QR code encoding `competition_id` + `judging_number`
  - **No brewer name or entry ID** (blind judging enforcement)
- **AC3.2**: Label is printable on standard Avery-compatible label sheets (2" x 4" or similar).
- **AC3.3**: QR code scannable by mobile devices for quick check-in.

---

### 4. **Bottle Check-In**

**Description**: Stewards check in physical bottles at competition venue, scanning judging numbers and recording condition. This functionality is managed by the **Competition Service**.

**Acceptance Criteria**:
- **AC4.1**: Steward can scan or manually enter judging number; system identifies entry and marks bottle as RECEIVED.
- **AC4.2**: Steward can mark bottle condition: GOOD, DAMAGED, LEAKED; add notes if defective.
- **AC4.3**: On check-in, system updates `Entries.bottle_status` and publishes `bottles.checked_in` event (includes quantity received, cellar location, defect status).
- **AC4.4**: Entrant receives confirmation email when bottle checked in (if condition=GOOD); organizer alerted if DAMAGED.
- **AC4.5**: Organizer can view list of all entries with bottle status (received, not received, defective).

---

### 5. **Flight Creation (Manual)**

**Description**: Organizers manually create flights (judging tables) and assign paid entries with received bottles.

**Acceptance Criteria**:
- **AC5.1**: Organizer can create flight with name (e.g., "Table 1") and optional scheduled time.
- **AC5.2**: Organizer can assign up to **10 entries** to a flight (system enforces max; displays error if exceeded).
- **AC5.3**: Only entries with `payment_status=PAID` and `bottle_status=RECEIVED` are assignable to flights.
- **AC5.4**: System randomizes initial tasting order when entries added to flight.
- **AC5.5**: Organizer can remove entries from flight (only if flight status = DRAFT or READY).

---

### 6. **Judge Assignment (Manual with Conflict Checks)**

**Description**: Organizers manually assign judges to flights; system validates no conflicts of interest (judge has entry in same competition).

**Acceptance Criteria**:
- **AC6.1**: Organizer can assign up to **3 judges** per flight (system enforces max).
- **AC6.2**: Before assignment, system checks if judge (`judge_user_id`) has any entry (`entrant_user_id`) in same `competition_id`.
- **AC6.3**: If conflict detected, assignment **rejected** with error: "Conflict of Interest: Judge has entry in this competition."
- **AC6.4**: If no conflict, assignment succeeds; `conflict_checked_at` timestamp recorded.
- **AC6.5**: Judge receives notification (email + in-app) with flight details: flight name, schedule, entries (judging numbers only, no brewer names).

---

### 7. **Scoresheet Submission (Mobile-First, Offline Capable)**

**Description**: Judges score entries on mobile devices using BJCP scoresheet; scores validated and stored.

**Acceptance Criteria**:
- **AC7.1**: Judge can access scoresheet form for entries in their assigned flights (identified by judging number only).
- **AC7.2**: Scoresheet fields:
  - Aroma (0-12)
  - Appearance (0-3)
  - Flavor (0-20)
  - Mouthfeel (0-5)
  - Overall Impression (0-10)
  - Text fields for aroma notes, flavor notes, mouthfeel notes, overall notes
- **AC7.3**: System validates each field (e.g., aroma ≤ 12) and total (≤ 50); displays error if validation fails.
- **AC7.4**: Judge can submit scoresheet; system saves to DB and publishes `scoresheet.submitted` event.
- **AC7.5**: **Offline capability**: If judge offline, scoresheet cached in browser IndexedDB; synced to server when connectivity restored (background sync API or manual retry).
- **AC7.6**: Judge can view and edit their own scoresheets before flight closed (edit triggers re-consensus in practice).

---

### 8. **Consensus Flow**

**Description**: After all judges submit scores, organizers/judges agree on final consensus score for each entry.

**Acceptance Criteria**:
- **AC8.1**: System displays average scores (aroma, appearance, flavor, mouthfeel, overall) from all judges for an entry.
- **AC8.2**: Organizer or head judge can view all individual scoresheets for an entry side-by-side.
- **AC8.3**: After in-person discussion, head judge enters final consensus score (may differ from average).
- **AC8.4**: System marks one scoresheet per entry as `is_consensus=true` (final score).
- **AC8.5**: Consensus triggers `consensus.completed` event; entry marked as scored.

---

### 9. **BOS Marking & BOS Flight**

**Description**: Judges mark Best of Show (BOS) candidates during flights; organizer creates BOS flight for final ranking.

**Acceptance Criteria**:
- **AC9.1**: During judging, any judge at table can mark entry as BOS candidate (button/checkbox on scoresheet UI).
- **AC9.2**: System creates `BOSCandidates` record; publishes `bos.candidate.marked` event.
- **AC9.3**: Organizer can view list of all BOS candidates for competition.
- **AC9.4**: Organizer creates BOS flight with variable number of BOS judges (3-5 typical).
- **AC9.5**: BOS judges rank candidates (1st, 2nd, 3rd, etc.); system aggregates rankings and assigns `bos_rank` to entries.
- **AC9.6**: BOS rankings published via `bos.ranked` event.

---

### 10. **Results Publication**

**Description**: Organizer publishes competition results; entrants can view scores and feedback.

**Acceptance Criteria**:
- **AC10.1**: Organizer clicks "Publish Results" button; competition status → COMPLETE.
- **AC10.2**: System publishes `results.published` event; triggers email to all entrants with results link.
- **AC10.3**: Entrant can view their entries with:
  - Final consensus score (total + breakdown)
  - Judge feedback (combined notes from all judges)
  - Placement within style category (rank by score)
  - BOS placement (if applicable)
- **AC10.4**: Public results page shows leaderboard with:
  - Entry ID (or anonymized identifier)
  - Style
  - Final score
  - Placement
  - **No brewer names** until entrant logs in (privacy).
- **AC10.5**: Organizer can download results as CSV or PDF (full report with all entries, scores, feedback).

---

### 11. **Authentication & Authorization (Keycloak)**

**Description**: Secure user authentication and role-based access control via Keycloak.

**Acceptance Criteria**:
- **AC11.1**: User can register account with email, password; Keycloak stores credentials.
- **AC11.2**: User can log in via Keycloak-hosted login page; BFF handles OAuth2 Authorization Code flow.
- **AC11.3**: Roles enforced: ORGANIZER, JUDGE, ENTRANT, STEWARD (users can have multiple roles).
- **AC11.4**: BFF validates JWT on every request; rejects if expired, invalid signature, or missing required role.
- **AC11.5**: Multi-tenant isolation: User belongs to one or more tenants (organizations); BFF injects `tenant_id` into all service requests.
- **AC11.6**: User can view/edit profile (name, email, BJCP rank if judge).

---

### 12. **Docker Compose Local Infrastructure**

**Description**: Local development environment with all services, databases, and dependencies.

**Acceptance Criteria**:
- **AC12.1**: `docker-compose up -d` starts:
  - PostgreSQL (primary DB)
  - RabbitMQ (event bus)
  - Redis (cache/session store)
  - Keycloak (identity provider)
  - BFF, Competition Service, Judging Service
  - Frontend (React dev server or Nginx)
  - (Analytics Service not part of MVP)
- **AC12.2**: Services can communicate via Docker network; no manual network configuration required.
- **AC12.3**: Database schema auto-created via migrations on first start.
- **AC12.4**: BJCP 2021 styles seeded from `scripts/seed-data.sql`.
- **AC12.5**: Developers can attach debuggers to services; logs visible via `docker-compose logs -f <service>`.

---

### 13. **CI/CD Pipeline Skeleton (GitHub Actions)**

**Description**: Automated build, test, and deployment pipeline.

**Acceptance Criteria**:
- **AC13.1**: On push to `main` branch:
  - Build all .NET services and frontend (React).
  - Run unit tests and integration tests.
  - Build Docker images and push to Azure Container Registry.
  - Run database migrations.
  - Deploy containers to Azure (Container Instances or App Service).
- **AC13.2**: On pull request:
  - Run tests and linters.
  - Block merge if tests fail or code coverage < 70%.
- **AC13.3**: Pipeline logs visible in GitHub Actions UI; failures notify team via email/Slack.

---

## Out of Scope for MVP (Post-v1 Enhancements)

**Not included in MVP** but planned for future releases:

- **Automated Judge Assignment**: AI-based suggestion algorithm (manual assignment only in MVP).
- **Calendar Integration**: Google/Outlook calendar sync for judge availability.
- **Multi-Language Support**: MVP is English-only.
- **Advanced Analytics & ML**: Competition dashboards, judge performance tracking, ML-driven judge assignment suggestions (handled by Analytics Service in post-MVP).
- **Mobile Native Apps**: MVP uses responsive web app (PWA); native iOS/Android apps post-MVP.
- **GDPR Compliance Tools**: Data export/deletion workflows (manual process in MVP).
- **Payment Refunds**: Manual refund processing (no automated refund workflow in MVP).
- **Temperature Logging**: Cellar service tracks received/defective only; no temp sensors in MVP.
- **Event Sourcing**: MVP uses standard CRUD with event bus; full event sourcing migration post-MVP.

---

## Success Metrics (Post-Launch)

**KPIs to measure MVP success**:

1. **User Adoption**: 10+ organizers onboard within 3 months; 200+ entrants register.
2. **Scoring Efficiency**: Average time to complete flight scoring ≤ 35 minutes (target: 30 min).
3. **Offline Sync Reliability**: 95%+ of offline scoresheets successfully sync when reconnected.
4. **System Uptime**: 99.5% uptime during judging windows.
5. **Conflict Detection**: Zero instances of judges scoring their own entries (validation works 100%).
6. **Payment Success Rate**: 98%+ of payment attempts succeed (Stripe integration).

---

## Prioritization & Phasing

**Phase 1 (Weeks 1-4)**: Infrastructure & Core Entities
- Docker Compose setup.
- Database schema and migrations.
- Keycloak integration.
- Basic BFF routing.

**Phase 2 (Weeks 5-8)**: Competition & Entry Management
- Competition CRUD.
- Entry registration and payment (Stripe).
- Label generation.
- Bottle check-in.

**Phase 3 (Weeks 9-12)**: Judging & Scoring
- Flight creation and entry assignment.
- Judge assignment with conflict checks.
- Scoresheet submission (online and offline).
- Consensus flow.

**Phase 4 (Weeks 13-14)**: BOS & Results
- BOS candidate marking.
- BOS flight and ranking.
- Results publication.

**Phase 5 (Weeks 15-16)**: Testing, Hardening, Deployment
- Integration and E2E tests.
- Performance testing (200 entrants, 50 judges load).
- Security audit (OWASP Top 10).
- Production deployment to Azure.

---

**Document Version**: 1.0  
**Last Updated**: 2025-12-18
