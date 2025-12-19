# 🗺️ MVP Implementation Roadmap

**Last Updated:** 2025-12-19  
**Target:** Production-ready MVP in 12-16 weeks  
**Team:** 3-4 engineers (2 backend, 1 frontend, 1 full-stack/devops)

---

## 📐 Implementation Strategy

### Core Principles
1. **Foundation First:** Infrastructure and authentication must be rock-solid
2. **Vertical Slices:** Complete user journeys before expanding features
3. **Parallel Workstreams:** Backend and frontend can work in parallel after Sprint 1
4. **Test as You Go:** Integration tests with every feature (Testcontainers)
5. **Deploy Early:** Staging environment from Sprint 1, dogfooding from Sprint 2

### Critical Path
The following issues are on the critical path and CANNOT be parallelized:
1. INFRA-001 → INFRA-002 → INFRA-003 → INFRA-004 (Infrastructure setup)
2. AUTH-001 → AUTH-002 → AUTH-003 (Authentication pipeline)
3. COMP-001 → COMP-002 → COMP-003 → COMP-004 (Competition foundation)
4. ENTRY-001 → ENTRY-002 → ENTRY-003 (Entry workflow)
5. FLIGHT-001 → FLIGHT-002 → SCORE-001 → SCORE-002 (Judging workflow)
6. BOS-001 → BOS-002 → BOS-003 → BOS-004 (BOS workflow)

---

## 🏃 Sprint Plan (2-week sprints)

### **Sprint 0: Foundation (Weeks 1-2)**
**Goal:** Infrastructure ready for development

**Team Focus:** All hands on deck for infrastructure

**Issues (7 total - 19 days estimated):**
1. ✅ **INFRA-001:** PostgreSQL Multi-Tenant Setup (M - 3-5 days) - @backend
2. ✅ **INFRA-002:** Docker Compose Environment (M - 2-3 days) - @devops
3. **INFRA-005:** BJCP Styles Database Seeding (S - 1-2 days) - @backend
4. **OBS-001:** Structured Logging Setup (S - 1-2 days) - @backend
5. **OBS-004:** Health Checks & Readiness Probes (S - 1-2 days) - @backend
6. ✅ **INFRA-003:** Event Outbox Worker (M - 3-4 days) - @backend
7. ✅ **INFRA-004:** RabbitMQ Event Bus (M - 3-4 days) - @backend

**Parallel Workstreams:**
- **Backend Track:** INFRA-001 → INFRA-003 → INFRA-004
- **DevOps Track:** INFRA-002 → OBS-001 → OBS-004
- **Backend Track 2:** INFRA-005 (can start after INFRA-001)

**Sprint 0 Deliverables:**
- ✅ PostgreSQL with RLS running locally
- ✅ Docker Compose with all services
- ✅ Event-driven architecture functional
- ✅ BJCP styles in database
- ✅ Health checks and logging working

**Sprint 0 Demo:**
- Show Docker Compose starting all services
- Query BJCP styles from database
- Publish test event to RabbitMQ
- View logs in Seq/console

---

### **Sprint 1: Authentication & Core Competition (Weeks 3-4)**
**Goal:** Users can log in and organizers can create competitions

**Team Focus:** Backend focus on CQRS patterns, Frontend starts UI

**Backend Issues (6 issues - 20 days estimated):**
1. ✅ **AUTH-001:** Keycloak OIDC Integration (L - 5-8 days) - @backend
2. ✅ **AUTH-002:** BFF Token Validation (S - 1-2 days) - @backend
3. ✅ **AUTH-003:** RBAC Policies (S - 1-2 days) - @backend
4. ✅ **COMP-001:** Competition Domain Models (S - 2-3 days) - @backend
5. ✅ **COMP-002:** Competition CRUD API (M - 4-5 days) - @backend
6. **COMP-003:** Competition Status Transitions (S - 2-3 days) - @backend
7. **COMP-004:** BJCP Style Integration API (M - 3-4 days) - @backend

**Frontend Issues (2 issues - 12 days estimated):**
1. **UI-001:** Login & Authentication UI (M - 4-5 days) - @frontend
2. **UI-002:** Competition Dashboard (Organizer) (L - 6-8 days) - @frontend (start)

**Parallel Workstreams:**
- **Backend Track 1:** AUTH-001 → AUTH-002 → AUTH-003
- **Backend Track 2:** COMP-001 → COMP-002 → COMP-003 (starts after AUTH-003)
- **Frontend Track:** UI-001 → UI-002 (starts after AUTH-001 deployed)

**Sprint 1 Deliverables:**
- ✅ User can log in via Keycloak
- ✅ Organizer role enforced
- ✅ Organizer can create competitions
- ✅ Competition status transitions work
- ✅ Frontend dashboard shows competitions list

**Sprint 1 Demo:**
- Register new user (organizer role)
- Log in and see competition dashboard
- Create new competition
- Transition competition to "Registration Open"

---

### **Sprint 2: Entry Management (Weeks 5-6)**
**Goal:** Entrants can submit entries and generate labels

**Team Focus:** Complete entry workflow end-to-end

**Backend Issues (4 issues - 17 days estimated):**
1. **ENTRY-001:** Entry Submission API (M - 4-5 days) - @backend
2. **ENTRY-002:** Entry Label PDF Generation (M - 3-4 days) - @backend
3. **ENTRY-003:** Bottle Check-In API (M - 4-5 days) - @backend
4. **COMP-005:** Entry Fee & Payment Tracking (M - 4-5 days) - @backend (start)

**Frontend Issues (2 issues - 13 days estimated):**
1. **UI-002:** Competition Dashboard (finish from Sprint 1) (L - 4 days remaining) - @frontend
2. **UI-003:** Entry Registration Form (M - 4-5 days) - @frontend
3. **UI-004:** Bottle Check-In Interface (M - 4-5 days) - @frontend (start)

**Parallel Workstreams:**
- **Backend Track 1:** ENTRY-001 → ENTRY-002
- **Backend Track 2:** ENTRY-003 (can start after ENTRY-001)
- **Frontend Track:** UI-002 (finish) → UI-003 → UI-004 (start)

**Sprint 2 Deliverables:**
- Entrant can register and submit entries
- Entry labels generated as PDF
- Payment tracking working
- Steward can check in bottles
- Dashboard shows entry stats

**Sprint 2 Demo:**
- Entrant logs in and submits 2 entries
- Download entry labels (blind - no brewer info)
- Print labels and affix to bottles
- Steward scans QR code to check in bottles
- Organizer sees check-in progress on dashboard

---

### **Sprint 3: Flight Assignment & COI (Weeks 7-8)**
**Goal:** Flights created automatically with COI detection

**Team Focus:** Flight algorithm is complex - backend priority

**Backend Issues (4 issues - 19 days estimated):**
1. **FLIGHT-004:** COI Detection Algorithm (M - 3-4 days) - @backend
2. **FLIGHT-001:** Flight Creation & Auto-Assignment (L - 6-8 days) - @backend
3. **FLIGHT-002:** Judge Assignment to Flights (M - 4-5 days) - @backend
4. **FLIGHT-003:** Steward Assignment (S - 2-3 days) - @backend (P1)

**Frontend Issues (2 issues - 8 days estimated):**
1. **UI-004:** Bottle Check-In Interface (finish) (M - 2 days remaining) - @frontend
2. **UI-005:** Judge Dashboard & Flight List (M - 4-5 days) - @frontend

**Parallel Workstreams:**
- **Backend Track 1:** FLIGHT-004 → FLIGHT-002
- **Backend Track 2:** FLIGHT-001 (can start after ENTRY-003 complete)
- **Frontend Track:** UI-004 (finish) → UI-005

**Sprint 3 Deliverables:**
- Flights auto-generated from checked-in entries
- COI detection prevents judges scoring own entries
- Judges assigned to flights
- Judge can see assigned flights on dashboard

**Sprint 3 Demo:**
- Organizer clicks "Generate Flights" button
- System creates 8 flights (6-10 entries each)
- Organizer assigns 3 judges per flight
- System warns about COI (judge has entry in Flight 3)
- Judge logs in and sees assigned flights

---

### **Sprint 4: Offline Scoresheet PWA (Weeks 9-10)**
**Goal:** Judges can enter scoresheets offline

**Team Focus:** Biggest frontend feature - may need backend support

**Frontend Issues (2 issues - 18 days estimated):**
1. **SCORE-001:** Offline PWA Scoresheet Entry UI (XL - 10-12 days) - @frontend (primary), @backend (support)
2. **SCORE-003:** BJCP Scoresheet Validation (M - 3-4 days) - @frontend + @backend
3. **SCORE-005:** Judge Notes & Comments (S - 2-3 days) - @frontend (P1)

**Backend Issues (2 issues - 9 days estimated):**
1. **SCORE-002:** Scoresheet Sync & Conflict Resolution (L - 6-8 days) - @backend
2. **SCORE-003:** BJCP Validation (backend portion) (M - 2 days) - @backend

**Parallel Workstreams:**
- **Frontend Track:** SCORE-001 (main PWA) → SCORE-005 (P1)
- **Backend Track:** SCORE-003 (backend validation) → SCORE-002 (sync)
- **Integration:** Frontend + Backend collaborate on SCORE-002 sync protocol

**Sprint 4 Deliverables:**
- PWA installable on tablets
- Scoresheet entry works offline
- Data syncs when back online
- Conflict resolution tested
- BJCP validation enforced

**Sprint 4 Demo:**
- Judge installs PWA on iPad
- Turn off WiFi
- Enter 5 scoresheets offline
- Turn WiFi back on
- Scoresheets sync automatically to backend
- Show sync status indicators

---

### **Sprint 5: Mini-BOS & Best of Show (Weeks 11-12)**
**Goal:** Complete judging workflow with BOS round

**Team Focus:** BOS calculations and final results

**Backend Issues (5 issues - 16 days estimated):**
1. **SCORE-004:** Mini-BOS Calculation (M - 4-5 days) - @backend
2. **BOS-001:** BOS Round Management (M - 4-5 days) - @backend
3. **BOS-002:** BOS Judge Selection (S - 2-3 days) - @backend
4. **BOS-003:** BOS Scoresheet Entry (M - 4-5 days) - @backend
5. **BOS-004:** BOS Winner Calculation (S - 2-3 days) - @backend

**Frontend Issues (1 issue - 4 days estimated):**
1. **BOS-003:** BOS Scoresheet UI (M - 4 days) - @frontend

**Parallel Workstreams:**
- **Backend Track 1:** SCORE-004 → BOS-001 → BOS-002
- **Backend Track 2:** BOS-004 (can start after BOS-003 data model ready)
- **Frontend Track:** BOS-003 UI (uses SCORE-001 patterns)

**Sprint 5 Deliverables:**
- Mini-BOS winners calculated per flight
- BOS round created with top entries
- BOS judges assigned
- BOS scoresheets submitted
- Winners announced

**Sprint 5 Demo:**
- Show Mini-BOS top 3 from Flight 5
- Organizer creates BOS round
- Assign 7 BOS judges (BJCP Certified+)
- BOS judges enter rankings
- System calculates: 1st Place - IPA, 2nd Place - Stout, 3rd Place - Lager

---

### **Sprint 6: Results & Polish (Weeks 13-14)**
**Goal:** Results publishing and MVP refinement

**Team Focus:** Polish and production readiness

**Backend Issues (4 issues - 13 days estimated):**
1. **BOS-005:** Awards & Medals Assignment (M - 3-4 days) - @backend
2. **BOS-006:** Winners Notification System (S - 2-3 days) - @backend
3. **BOS-007:** Results Export (CSV/PDF) (M - 3-4 days) - @backend
4. **SCORE-006:** Scoresheet PDF Export (M - 3-4 days) - @backend

**Frontend Issues (1 issue - 5 days estimated):**
1. **UI-006:** Results Publishing Page (M - 4-5 days) - @frontend

**DevOps/QA Issues:**
1. **E2E Testing:** Full competition workflow tests (5 days) - @qa
2. **Load Testing:** 200 entrants, 50 judges simulation (3 days) - @devops
3. **Security Audit:** Penetration testing, OWASP checks (3 days) - @devops + @backend

**Parallel Workstreams:**
- **Backend Track:** BOS-005 → BOS-006, BOS-007, SCORE-006
- **Frontend Track:** UI-006
- **QA Track:** E2E tests + Load testing
- **DevOps Track:** Security audit + performance tuning

**Sprint 6 Deliverables:**
- Results published automatically
- Winners receive email notifications
- CSV/PDF exports working
- Entrants can download scoresheets
- E2E tests pass
- Load testing validates 200+ concurrent users

**Sprint 6 Demo (MVP Complete):**
- Full competition workflow end-to-end:
  1. Organizer creates competition
  2. 10 entrants submit 25 entries
  3. Entries checked in by steward
  4. Flights auto-generated (3 flights)
  5. Judges assigned (3 per flight)
  6. 9 judges enter 25 scoresheets (offline)
  7. Mini-BOS calculated
  8. BOS round with 9 top entries
  9. BOS judges rank entries
  10. Winners announced (Gold/Silver/Bronze per category + BOS)
  11. Results published online
  12. Entrants receive scoresheets via email

---

## 🎯 Sprints 7-8: Post-MVP Enhancements (Weeks 15-16)

**P1 Features (Optional for MVP):**
- **AUTH-004:** User Registration & Profile (M) - @backend
- **AUTH-005:** Tenant Onboarding (L) - @backend
- **COMP-006:** Competition Settings (S) - @backend
- **FLIGHT-005:** Flight Scheduling & Timing (S) - @backend
- **FLIGHT-006:** Flight Balancing Algorithm (M) - @backend
- **INFRA-005:** Observability Stack (OpenTelemetry, Azure Monitor) (L) - @devops

---

## 📊 Effort Summary by Sprint

| Sprint | Backend Days | Frontend Days | DevOps Days | Total Days | Critical Path? |
|--------|--------------|---------------|-------------|------------|----------------|
| Sprint 0 | 14 | 0 | 5 | 19 | ✅ Yes |
| Sprint 1 | 20 | 12 | 0 | 32 | ✅ Yes |
| Sprint 2 | 17 | 13 | 0 | 30 | ✅ Yes |
| Sprint 3 | 19 | 8 | 0 | 27 | ✅ Yes |
| Sprint 4 | 9 | 18 | 0 | 27 | ✅ Yes |
| Sprint 5 | 16 | 4 | 0 | 20 | ✅ Yes |
| Sprint 6 | 13 | 5 | 11 | 29 | ⚠️ Partial |
| **Total** | **108** | **60** | **16** | **184 person-days** | |

**With 3-4 Engineers:**
- **3 engineers:** ~61 working days = 12-13 weeks (realistic with buffer)
- **4 engineers:** ~46 working days = 9-10 weeks (aggressive, assumes perfect parallelization)

**Recommendation:** 12-16 weeks with 3-4 engineers (includes buffer for unknowns, testing, bug fixes)

---

## 🚀 Deployment Strategy

### Continuous Deployment
- **Sprint 1+:** Deploy to staging environment weekly
- **Sprint 2+:** Internal dogfooding (team uses app to plan test competition)
- **Sprint 4+:** Beta testing with 1-2 real competitions
- **Sprint 6:** Production launch

### Environments
1. **Development:** Local Docker Compose
2. **Staging:** Azure Container Apps with PostgreSQL Flexible Server
3. **Production:** Azure Container Apps with HA PostgreSQL, Redis cache, multi-region

---

## 🧪 Testing Strategy

### Per Sprint
- **Unit Tests:** Required for all handlers, validators (>80% coverage)
- **Integration Tests:** Testcontainers with real PostgreSQL/RabbitMQ
- **Manual Testing:** Each story tested by PM before sprint end

### Sprint 6 (Pre-Launch)
- **E2E Tests:** Cypress tests for critical user journeys
- **Load Testing:** k6 scripts simulating 200+ concurrent users
- **Security:** OWASP ZAP scan, dependency audit
- **Accessibility:** WCAG 2.1 AA compliance check (Axe DevTools)

---

## 🔄 Risk Mitigation

### High Risks
1. **Offline PWA Complexity (SCORE-001):**
   - Risk: Service Workers and IndexedDB sync issues
   - Mitigation: Spike in Sprint 3, prototype offline flow
   - Fallback: Online-only version if PWA blocked

2. **Flight Assignment Algorithm (FLIGHT-001):**
   - Risk: Algorithm doesn't balance flights well
   - Mitigation: Start with simple heuristic, iterate
   - Fallback: Manual flight creation by organizer

3. **Multi-Tenancy Leaks:**
   - Risk: Cross-tenant data access bugs
   - Mitigation: RLS at database level (defense in depth)
   - Testing: Dedicated integration tests for tenant isolation

4. **Scoresheet Sync Conflicts (SCORE-002):**
   - Risk: Complex conflict resolution logic
   - Mitigation: Last-write-wins with timestamps (simple)
   - Monitoring: Log all conflicts for analysis

---

## 📝 Implementation Notes

### Recommended Team Structure
- **Backend Engineer 1:** Infrastructure, Auth, Competition/Entry APIs
- **Backend Engineer 2:** Flight/Scoring/BOS logic, Event Bus
- **Frontend Engineer:** All UI (PWA focus)
- **Full-Stack/DevOps:** Docker, CI/CD, Observability, Testing

### Technology Choices Confirmed
- **Backend:** .NET 10, Entity Framework Core, MediatR, FluentValidation
- **Frontend:** React 18, TypeScript, TanStack Query, Zustand, IndexedDB (Dexie.js)
- **Database:** PostgreSQL 17+ with RLS
- **Message Bus:** RabbitMQ 3.12+
- **Identity:** Keycloak 23+
- **Observability:** Serilog, OpenTelemetry, Seq (dev), Azure Monitor (prod)
- **Testing:** xUnit, FluentAssertions, Testcontainers, Cypress

### Critical Path Bottlenecks
- **Sprint 1:** AUTH-001 blocks all authenticated endpoints
- **Sprint 2:** ENTRY-003 blocks flight assignment
- **Sprint 3:** FLIGHT-002 blocks scoresheet entry
- **Sprint 4:** SCORE-001 blocks judging workflow

**Mitigation:** Ensure critical path issues are staff-ed with senior engineers

---

## ✅ Success Criteria (MVP Complete)

### Functional
- [ ] Organizer can create competition and configure rules
- [ ] Entrants can register and submit entries with payment
- [ ] Labels generated (blind - no brewer info)
- [ ] Steward can check in bottles via QR scan
- [ ] Flights auto-generated with balanced sizes
- [ ] Judges assigned to flights without COI
- [ ] Judges enter scoresheets offline (PWA)
- [ ] Scoresheets sync automatically when online
- [ ] Mini-BOS winners calculated per flight
- [ ] BOS round managed with final rankings
- [ ] Winners announced (Gold/Silver/Bronze + BOS)
- [ ] Results published online
- [ ] Entrants receive scoresheets via email/download

### Non-Functional
- [ ] Multi-tenancy enforced (no cross-tenant data leaks)
- [ ] Authentication & authorization working (Keycloak + RBAC)
- [ ] Event-driven architecture functional (RabbitMQ + Outbox)
- [ ] PWA works offline on iOS/Android tablets
- [ ] API response times <200ms (95th percentile)
- [ ] System supports 200+ entrants, 50+ judges, 600+ bottles
- [ ] WCAG 2.1 AA accessibility compliance
- [ ] 80%+ test coverage (unit + integration)
- [ ] Zero critical security vulnerabilities (OWASP scan)

---

## 🎉 Post-MVP Roadmap (Weeks 17-24)

### Phase 2: Analytics & Insights
- Analytics Service (Python FastAPI)
- Judge performance analytics
- Entry trends by category/region
- Organizer dashboard insights

### Phase 3: Mobile Apps
- Native iOS app for judges
- Native Android app for judges
- Offline-first architecture

### Phase 4: Enterprise Features
- Multi-competition management
- White-label branding per tenant
- Advanced reporting
- API for third-party integrations

---

**📌 Next Steps:**
1. Run `setup-project-infrastructure.yml` workflow to create all labels/milestones
2. Begin Sprint 0 with INFRA-001 (PostgreSQL setup)
3. Daily standups to track progress vs. this roadmap
4. Weekly sprint reviews and retrospectives

**Let's build an amazing MVP! 🍺**
