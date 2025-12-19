# MVP Implementation Guide - Beer Competition SaaS

**Date**: 2025-01-20  
**Status**: Ready for Sprint 0 Kickoff  
**Total Issues**: 47 (15 created, 32 remaining)

---

## Current Progress

### ✅ Issues Created in GitHub (15/47)

| Issue | Title | Priority | Epic | Sprint |
|-------|-------|----------|------|--------|
| [#6](https://github.com/jesuscorral/beer-competition-saas/issues/6) | INFRA-001: PostgreSQL RLS | P0 | Infrastructure | Sprint 0 |
| [#2](https://github.com/jesuscorral/beer-competition-saas/issues/2) | INFRA-002: Docker Compose | P0 | Infrastructure | Sprint 0 |
| [#7](https://github.com/jesuscorral/beer-competition-saas/issues/7) | INFRA-003: Event Outbox Worker | P0 | Infrastructure | Sprint 0 |
| [#8](https://github.com/jesuscorral/beer-competition-saas/issues/8) | INFRA-004: RabbitMQ Setup | P0 | Infrastructure | Sprint 0 |
| [#13](https://github.com/jesuscorral/beer-competition-saas/issues/13) | INFRA-005: BJCP Styles Seeding | P0 | Infrastructure | Sprint 0 |
| [#3](https://github.com/jesuscorral/beer-competition-saas/issues/3) | AUTH-001: Keycloak Integration | P0 | Authentication | Sprint 1 |
| [#9](https://github.com/jesuscorral/beer-competition-saas/issues/9) | AUTH-002: BFF Token Middleware | P0 | Authentication | Sprint 1 |
| [#10](https://github.com/jesuscorral/beer-competition-saas/issues/10) | AUTH-003: RBAC Policies | P0 | Authentication | Sprint 1 |
| [#5](https://github.com/jesuscorral/beer-competition-saas/issues/5) | COMP-001: Competition Domain Models | P0 | Competitions | Sprint 1 |
| [#4](https://github.com/jesuscorral/beer-competition-saas/issues/4) | COMP-002: Competition CRUD API | P0 | Competitions | Sprint 1 |
| [#15](https://github.com/jesuscorral/beer-competition-saas/issues/15) | COMP-003: Status Transitions | P0 | Competitions | Sprint 1 |
| [#14](https://github.com/jesuscorral/beer-competition-saas/issues/14) | COMP-004: BJCP Style API | P0 | Competitions | Sprint 1 |
| [#11](https://github.com/jesuscorral/beer-competition-saas/issues/11) | OBS-001: Structured Logging | P0 | Observability | Sprint 0 |
| [#12](https://github.com/jesuscorral/beer-competition-saas/issues/12) | OBS-004: Health Checks | P0 | Observability | Sprint 0 |
| [#1](https://github.com/jesuscorral/beer-competition-saas/issues/1) | Initial Documentation | - | - | - |

---

## 📋 Remaining Issues to Create (32)

### Option 1: Manual Creation via GitHub UI
All issue details are documented in **[docs/MVP_ISSUES_COMPLETE.md](MVP_ISSUES_COMPLETE.md)**. For each issue:
1. Go to https://github.com/jesuscorral/beer-competition-saas/issues/new/choose
2. Select **"Feature Task"** template
3. Copy title, description, and labels from MVP_ISSUES_COMPLETE.md
4. Assign appropriate labels: `priority-p0/p1`, `complexity-S/M/L/XL`, `epic-*`, `sprint-*`
5. Assign to agent: `@backend-agent`, `@frontend-agent`, or `@devops-agent`

### Option 2: Bulk Creation via GitHub CLI
```bash
# Install GitHub CLI
winget install --id GitHub.cli

# Authenticate
gh auth login

# Create issues from MVP_ISSUES_COMPLETE.md (requires script)
# See section "GitHub CLI Bulk Import Script" below
```

### Option 3: Re-enable GitHub Issue Writer Tool
If you disabled the `mcp_io_github_git_issue_write` tool:
1. Check VS Code settings: `Copilot > Tools > GitHub`
2. Re-enable the tool
3. Run: `@product-owner continue creating remaining 32 issues`

---

## 🚀 Priority Order for Remaining Issues

### **CRITICAL P0 - Sprint 2 (Entries) - CREATE FIRST**
1. **ENTRY-001**: Entry Submission API with Payment (M, 3-5 days)
2. **ENTRY-002**: Entry Label PDF Generation (M, 2-3 days)
3. **ENTRY-003**: Bottle Check-In System (M, 3-4 days)

### **CRITICAL P0 - Sprint 1 (Frontend Foundation)**
4. **UI-001**: Login UI with Keycloak (S, 2 days)
5. **UI-002**: Competition Dashboard (M, 3-4 days)

### **CRITICAL P0 - Sprint 2 (Frontend Entries)**
6. **UI-003**: Entry Registration Form (M, 3 days)
7. **UI-004**: Bottle Check-In Interface (S, 2 days)

### **CRITICAL P0 - Sprint 3 (Flights)**
8. **FLIGHT-004**: COI Detection Algorithm (M, 3-4 days)
9. **FLIGHT-001**: Flight Auto-Assignment (L, 5-7 days)
10. **FLIGHT-002**: Manual Judge Assignment (M, 3-4 days)

### **CRITICAL P0 - Sprint 4 (Offline PWA - Largest Feature)**
11. **SCORE-001**: Offline PWA Scoresheet (L, 5-7 days)
12. **SCORE-002**: Scoresheet Sync Engine (M, 3-5 days)
13. **SCORE-003**: BJCP 2021 Validation (M, 3-4 days)

### **CRITICAL P0 - Sprint 4 (Frontend Judging)**
14. **UI-005**: Judge Dashboard with Flights (M, 3-4 days)

### **CRITICAL P0 - Sprint 5 (Best of Show)**
15. **BOS-001**: BOS Round Management (M, 3-4 days)
16. **BOS-002**: BOS Judge Selection (S, 2-3 days)
17. **BOS-003**: BOS Scoresheet Entry (M, 3 days)
18. **BOS-004**: Winner Calculation (M, 3-4 days)

### **HIGH P1 - Sprint 1-6 (Enhancements)**
19. **COMP-005**: Entry Fee Tracking (S, 2 days)
20. **COMP-006**: Competition Settings (M, 3 days)
21. **FLIGHT-003**: Steward Assignment (S, 2 days)
22. **FLIGHT-005**: Flight Scheduling (M, 3 days)
23. **FLIGHT-006**: Flight Balancing (M, 3 days)
24. **SCORE-004**: Mini-BOS Calculation (M, 3 days)
25. **SCORE-005**: Judge Notes (S, 1-2 days)
26. **SCORE-006**: Scoresheet PDF Export (M, 2-3 days)
27. **BOS-005**: Awards & Medals (M, 3 days)
28. **BOS-006**: Winners Notification (S, 2 days)
29. **BOS-007**: Results Export (M, 2-3 days)
30. **UI-006**: Results Publishing (M, 3 days)
31. **AUTH-004**: User Registration & Profile (M, 3 days)
32. **AUTH-005**: Tenant Onboarding (L, 4-5 days)

---

## 📊 Sprint Breakdown Summary

| Sprint | Duration | Focus Area | Issues | Story Points | Critical Path |
|--------|----------|------------|--------|--------------|---------------|
| **Sprint 0** | 2 weeks | Foundation | 5 (all created) | 19 days | ✅ Ready |
| **Sprint 1** | 2-3 weeks | Auth & Competitions | 8 (4 created, 4 need creation) | 31 days | Need UI-001, UI-002 |
| **Sprint 2** | 2-3 weeks | Entry Management | 5 (0 created, 5 need creation) | 28 days | **BLOCKER** |
| **Sprint 3** | 2-3 weeks | Flight Management | 6 (0 created, 6 need creation) | 32 days | **BLOCKER** |
| **Sprint 4** | 3-4 weeks | Offline PWA | 7 (0 created, 7 need creation) | 38 days | **BLOCKER** |
| **Sprint 5** | 2 weeks | Best of Show | 7 (0 created, 7 need creation) | 22 days | **BLOCKER** |
| **Sprint 6** | 1-2 weeks | Results & Polish | 9 (0 created, 9 need creation) | 14 days | P1 features |

**Total Effort**: 184 person-days (12-16 weeks with 3-4 engineers)

---

## 🛠️ GitHub CLI Bulk Import Script

Create this PowerShell script to import remaining issues:

```powershell
# create-remaining-issues.ps1
$repo = "jesuscorral/beer-competition-saas"

# Define issues array (excerpt - see MVP_ISSUES_COMPLETE.md for full content)
$issues = @(
    @{
        title = "[ENTRY-001] Entry Submission API with Payment Integration"
        body = @"
## User Story
As an **Entrant**, I want to submit beer entries...
(Copy full description from MVP_ISSUES_COMPLETE.md)
"@
        labels = "feature,backend,priority-p0,complexity-M,epic-entry-management,sprint-2"
    },
    # ... Add all 32 remaining issues here
)

foreach ($issue in $issues) {
    Write-Host "Creating issue: $($issue.title)" -ForegroundColor Yellow
    
    gh issue create `
        --repo $repo `
        --title $issue.title `
        --body $issue.body `
        --label $issue.labels
    
    Start-Sleep -Seconds 2  # Rate limit protection
}

Write-Host "✅ All issues created!" -ForegroundColor Green
```

**Run script:**
```powershell
cd c:\MyPersonalWS\beer-competition-saas
.\create-remaining-issues.ps1
```

---

## 🎯 Critical Path Analysis

### **Weeks 1-2: Sprint 0 (Foundation)** - ✅ Issues Created
**Status**: Ready to start immediately  
**Issues**: #6, #2, #7, #8, #13, #11, #12 (all created)

**Parallel Tracks:**
- **Backend Track 1**: PostgreSQL RLS (#6) → Event Outbox (#7)
- **Backend Track 2**: BJCP Styles Seeding (#13) → BJCP API (#14)
- **DevOps Track**: Docker Compose (#2) → RabbitMQ (#8) → Health Checks (#12)
- **Observability Track**: Logging (#11) → Health Checks (#12)

**Sprint 0 Goal**: "Infrastructure ready for development" ✅

---

### **Weeks 3-5: Sprint 1 (Auth & Competitions)** - ⚠️ 4 Issues Missing
**Status**: Partially ready (4/8 issues created)  
**Created**: #3, #9, #10, #5, #4, #15, #14  
**Missing**: UI-001 (Login), UI-002 (Dashboard), COMP-005 (P1), COMP-006 (P1)

**Parallel Tracks:**
- **Backend**: Keycloak (#3) → BFF Middleware (#9) → RBAC (#10)
- **Backend**: Domain Models (#5) → Competition API (#4) → Status Workflow (#15)
- **Frontend** (❌ BLOCKED): Login UI (need UI-001) → Dashboard (need UI-002)

**Unblock Sprint 1**: Create UI-001 and UI-002 issues ASAP

---

### **Weeks 6-8: Sprint 2 (Entries)** - ❌ 0 Issues Created - **CRITICAL BLOCKER**
**Status**: Completely blocked (0/5 issues)  
**Missing**: ENTRY-001, ENTRY-002, ENTRY-003, UI-003, UI-004

**Cannot Start**: Entry Management is core MVP functionality  
**Impact**: Delays Sprint 3, 4, 5, 6 (entire critical path)

**Action Required**: Create ENTRY-001, ENTRY-002, ENTRY-003 immediately (highest priority)

---

### **Weeks 9-11: Sprint 3 (Flights)** - ❌ 0 Issues Created - **CRITICAL BLOCKER**
**Status**: Blocked (0/6 issues)  
**Missing**: FLIGHT-001 through FLIGHT-006

**Cannot Start**: Depends on Sprint 2 completion  
**Impact**: Judging cannot begin without flight assignments

---

### **Weeks 12-15: Sprint 4 (Offline PWA)** - ❌ 0 Issues Created - **LARGEST FEATURE BLOCKED**
**Status**: Blocked (0/7 issues)  
**Missing**: SCORE-001 (L-complexity), SCORE-002, SCORE-003, UI-005, plus 3 P1 features

**Cannot Start**: Most complex feature (offline-first architecture)  
**Impact**: Core judging workflow unavailable

---

### **Weeks 16-17: Sprint 5 (Best of Show)** - ❌ 0 Issues Created - **BLOCKER**
**Status**: Blocked (0/7 issues)  
**Missing**: BOS-001 through BOS-007

**Cannot Start**: Requires completed scoresheets from Sprint 4  
**Impact**: Cannot determine competition winners

---

### **Weeks 18-19: Sprint 6 (Results & Polish)** - ❌ 0 Issues Created
**Status**: Blocked (0/9 issues)  
**Missing**: UI-006, AUTH-004, AUTH-005, plus 6 P1 features

**Cannot Start**: Depends on all previous sprints  
**Impact**: Results cannot be published

---

## ⚠️ Risk Assessment

### **HIGH RISK: Critical Path Blocked**
- **Issue**: 32/47 issues (68%) not yet in GitHub
- **Impact**: Team cannot see full scope, cannot start Sprint 2+
- **Mitigation**: Create remaining issues ASAP (prioritize P0 first)

### **HIGH RISK: Sprint 2 Cannot Start**
- **Issue**: 0/5 Entry Management issues created
- **Impact**: Entire MVP delayed if Sprint 2 blocked
- **Mitigation**: Create ENTRY-001, ENTRY-002, ENTRY-003 today

### **MEDIUM RISK: Frontend Track Blocked**
- **Issue**: UI-001, UI-002 missing for Sprint 1
- **Impact**: Frontend engineer idle during Sprint 1
- **Mitigation**: Create UI issues alongside Entry issues

### **LOW RISK: P1 Features Can Wait**
- **Issue**: P1 features (COMP-005, COMP-006, AUTH-004, etc.) not critical for MVP
- **Impact**: Can defer to Sprint 6 or post-MVP
- **Mitigation**: Focus on P0 issues first

---

## 🎬 Next Steps - Action Plan

### **Immediate (Today)**
1. ✅ **Created**: This implementation guide
2. ⬜ **Decide**: Manual GitHub UI creation vs GitHub CLI script vs re-enable tool
3. ⬜ **Create**: ENTRY-001, ENTRY-002, ENTRY-003 (Sprint 2 blockers)
4. ⬜ **Create**: UI-001, UI-002 (Sprint 1 blockers)
5. ⬜ **Create**: FLIGHT-004, FLIGHT-001, FLIGHT-002 (Sprint 3 critical path)

### **This Week**
6. ⬜ **Create**: All remaining P0 issues (SCORE, BOS, UI-003, UI-004, UI-005)
7. ⬜ **Run**: Setup Project Infrastructure workflow (labels + milestones)
8. ⬜ **Assign**: Sprint 0 issues to @backend-agent and @devops-agent
9. ⬜ **Start**: Sprint 0 development (Docker Compose + PostgreSQL RLS)
10. ⬜ **Test**: Testcontainers setup for integration tests

### **Next Week**
11. ⬜ **Create**: All P1 issues (nice-to-have features)
12. ⬜ **Sprint Review**: Sprint 0 → Validate infrastructure works
13. ⬜ **Sprint Planning**: Sprint 1 → Assign AUTH and COMP issues
14. ⬜ **Kickoff**: Sprint 1 development (Auth + Competitions)

### **Weeks 3-4**
15. ⬜ **Sprint Review**: Sprint 1 → Validate auth and competitions working
16. ⬜ **Sprint Planning**: Sprint 2 → Assign ENTRY issues
17. ⬜ **Kickoff**: Sprint 2 development (Entry Management)

---

## 📚 Key Documents

| Document | Purpose | Status |
|----------|---------|--------|
| [MVP_ISSUES_COMPLETE.md](MVP_ISSUES_COMPLETE.md) | Complete specs for all 47 issues | ✅ Complete |
| [IMPLEMENTATION_ROADMAP.md](IMPLEMENTATION_ROADMAP.md) | 6-sprint plan with parallel workstreams | ✅ Complete |
| [MVP_IMPLEMENTATION_GUIDE.md](MVP_IMPLEMENTATION_GUIDE.md) | This file - action plan and status | ✅ Complete |
| [BACKLOG.md](BACKLOG.md) | Product backlog with priorities | ✅ Existing |
| [ARCHITECTURE.md](architecture/ARCHITECTURE.md) | System architecture overview | ✅ Existing |
| [ADR-001 through ADR-007](architecture/decisions/) | Architecture decision records | ✅ Existing |

---

## 🏆 Success Criteria

### **Sprint 0 Ready** (Current Status)
- ✅ 5/5 infrastructure issues created in GitHub
- ✅ All Sprint 0 issues have detailed specs
- ✅ Team can start development immediately
- ⬜ Docker Compose running locally
- ⬜ PostgreSQL RLS working
- ⬜ RabbitMQ configured

### **Sprint 1 Ready**
- ⚠️ 4/8 issues created (missing UI-001, UI-002)
- ⬜ Keycloak configured
- ⬜ BFF token validation working
- ⬜ Competition CRUD API functional

### **Sprint 2 Ready** (BLOCKED)
- ❌ 0/5 issues created
- ❌ Team cannot start Sprint 2 without issues
- **ACTION REQUIRED**: Create ENTRY issues today

### **MVP Complete** (Target: Week 18)
- ⬜ 47/47 issues created in GitHub
- ⬜ All P0 features (31 issues) completed
- ⬜ All P1 features (16 issues) completed or deferred
- ⬜ System deployed to production
- ⬜ First competition runs successfully

---

## 🆘 Support

**Questions?** Contact:
- **Product Owner**: @product-owner agent for roadmap/prioritization questions
- **Backend**: @backend-agent for technical implementation questions
- **Frontend**: @frontend-agent for UI/UX questions
- **DevOps**: @devops-agent for deployment questions

**Issue with this guide?** Update this document and commit changes.

---

**Last Updated**: 2025-01-20  
**Next Review**: After completing remaining issue creation  
**Document Owner**: Product Owner Agent
