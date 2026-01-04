# Beer Competition SaaS Platform

**Status**: 🟢 MVP in Progress (Sprint 0 & 1 Partially Complete)  
**Last Updated**: 2026-01-04

## Overview

A comprehensive, multi-tenant SaaS platform for managing BJCP 2021-compliant homebrew beer competitions. The system enforces blind judging, conflict-of-interest rules, and accurate scoring while supporting typical scales of 200 entrants, 50 concurrent judges, and 600 bottles per competition.

**Target Users**: Competition organizers, homebrewers (entrants), BJCP judges, stewards.

**Current Implementation Status**:
- ✅ PostgreSQL 16 with Row-Level Security (multi-tenancy)
- ✅ Keycloak OIDC authentication with OAuth 2.0 Token Exchange
- ✅ BFF (Backend-for-Frontend) API Gateway with YARP reverse proxy
- ✅ Modular monolith architecture (Host + Competition Module)
- ✅ Integration testing infrastructure (Testcontainers + Respawn + Builder Pattern)
- ✅ Event-driven architecture (RabbitMQ + Outbox Pattern)
- ⏳ Frontend React PWA (planned)
- ⏳ Complete CRUD operations (in progress)

---

## Key Features

### For Organizers
- Create and manage multiple competitions with flexible scheduling
- Configure registration deadlines, bottle submission windows, and judging dates
- Manual flight creation and judge assignment with automatic conflict validation
- Real-time competition monitoring and bottle check-in tracking
- Results publication with automated entrant notifications
- Export competition results as CSV or PDF

### For Entrants (Homebrewers)
- Register beer entries with BJCP 2021 style selection
- Online payment via Stripe integration
- Download printable bottle labels with anonymous judging numbers
- Track bottle reception status
- View detailed scoresheets and judge feedback after results publication
- See placement within style categories and Best of Show rankings

### For Judges
- Mobile-first scoresheet interface with offline capability
- Score entries using standard BJCP criteria (Aroma, Appearance, Flavor, Mouthfeel, Overall Impression)
- View assigned flights with entries (by judging number only - blind judging enforced)
- Participate in consensus scoring process
- Mark Best of Show candidates during judging
- Automatic conflict-of-interest prevention (cannot judge own entries)

### For Stewards
- Quick bottle check-in via QR code scanning or manual entry
- Track bottle condition (good, damaged, leaked)
- Real-time status updates for organizers and entrants

---

## Architecture Highlights

**Stack**:
- **Backend**: .NET 10 (Modular Monolith + BFF API Gateway) ✅
  - **Host**: Main application hosting all modules
  - **BFF**: YARP reverse proxy with OAuth 2.0 Token Exchange
  - **Modules** (Bounded Contexts):
    - ✅ **Competition Module**: Competitions, entries, organizers, tenants, styles
    - ⏳ **Judging Module**: Flights, judge assignments, scoresheets, BOS, consensus (planned)
    - ⏳ **Notification Module**: Email/SMS notifications, alerts (planned)
  - **Shared Kernel**: Common domain primitives, Result pattern, multi-tenancy abstractions
- **Frontend**: React 18 + TypeScript + Tailwind CSS (Progressive Web App) ⏳
- **Authentication**: Keycloak 23+ (OAuth2/OIDC) with service-specific audiences ✅
- **Database**: PostgreSQL 16 (multi-tenant with Row-Level Security) ✅
- **Event Bus**: RabbitMQ 3.12 with CloudEvents 1.0 format ✅
- **Cache**: Redis 7+ (future)
- **Orchestration**: Docker + Docker Compose ✅
- **Cloud**: Azure (Container Apps, PostgreSQL Flexible Server, Static Web Apps)
- **Testing**: xUnit + Testcontainers + Respawn + FluentAssertions ✅
- **Observability**: Serilog + OpenTelemetry (partial) ⏳
- **CI/CD**: GitHub Actions ⏳

**Architecture Pattern**: 
- **Current**: Modular Monolith with Vertical Slices and DDD patterns
- **Future**: Microservices with event-driven communication when scale requires
- **BFF**: Backend-for-Frontend as API gateway with token exchange

**Implemented Services**:
1. ✅ **BFF/API Gateway** (.NET 10): Token validation, OAuth 2.0 Token Exchange, routing
2. ✅ **Competition Module** (.NET 10): Tenant/Organizer registration, domain models
3. ⏳ **Judging Module** (.NET 10): Planned
4. ⏳ **Analytics Service** (Python - Post-MVP): ML-driven insights

**Project Structure**:
```
beer-competition-saas/
├── src/
│   ├── backend/
│   │   ├── BFF/BFF.ApiGateway/         # YARP reverse proxy + Token Exchange
│   │   ├── Host/BeerCompetition.Host/  # Modular monolith host
│   │   └── Modules/                    # Bounded contexts (DDD)
│   │       ├── Competition/            # Competition module (4 projects)
│   │       └── Shared/                 # Shared Kernel (3 projects)
│   └── frontend/                       # React PWA (in progress)
├── tests/
│   ├── BFF/BFF.ApiGateway.Tests/
│   └── Modules/Competition/            # Integration tests with Testcontainers
├── infrastructure/
│   ├── docker-compose.yml              # PostgreSQL, RabbitMQ, Keycloak
│   └── keycloak/realm-export.json      # Keycloak configuration
└── docs/
    ├── architecture/                   # Architecture Decision Records (ADRs)
    ├── development/                    # Solution structure
    └── roadmap/                        # Implementation roadmap
```

---

## BJCP Compliance

**Blind Judging**:
- Judges see only anonymous judging numbers (sequential integers)
- Bottle labels contain no brewer information
- System maintains strict separation between entry IDs (public) and judging numbers (anonymous)

**Scoring Rules**:
- Standard BJCP scoresheet with exact field maxima:
  - Aroma: 12 points
  - Appearance: 3 points
  - Flavor: 20 points
  - Mouthfeel: 5 points
  - Overall Impression: 10 points
  - **Total Maximum**: 50 points
- System validates all scores; rejects submissions exceeding limits

**Conflict of Interest**:
- Automatic validation prevents judge assignment to flights if judge has entry in competition
- Assignment rejected with clear error message if conflict detected

**Styles**: Full BJCP 2021 style catalog (115+ styles across 34 categories)

---

## Getting Started (Local Development)

### Prerequisites

Before starting, ensure you have the following installed:

- **Docker Desktop** 4.25+ (includes Docker Compose v2)
  - [Download for Windows](https://www.docker.com/products/docker-desktop/)
  - Ensure WSL 2 backend is enabled (Settings → General → Use WSL 2 based engine)
- **.NET 10 SDK** ✅ **REQUIRED** (for backend services)
  - [Download .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Node.js 20+** and **npm** (for frontend - future)
  - [Download Node.js](https://nodejs.org/)
- **Git** for version control
- **PowerShell 7+** (recommended for Windows)
- **Visual Studio 2025, VS Code, or Rider** (IDE with .NET support)

### Quick Start

#### 1. Clone Repository
```powershell
git clone https://github.com/jesuscorral/beer-competition-saas.git
cd beer-competition-saas
```

#### 2. Start Infrastructure Services
```powershell
cd infrastructure
docker-compose up -d

# Verify all services are healthy
docker-compose ps

# Expected output:
# NAME                   STATUS              PORTS
# beercomp_postgres      Up (healthy)        0.0.0.0:5432->5432/tcp
# beercomp_rabbitmq      Up (healthy)        0.0.0.0:5672->5672/tcp, 0.0.0.0:15672->15672/tcp
# beercomp_keycloak      Up (healthy)        0.0.0.0:8080->8080/tcp
```

#### 3. Restore NuGet Packages & Build
```powershell
cd ..  # Return to root
dotnet restore BeerCompetition.sln
dotnet build BeerCompetition.sln
```

#### 4. Apply Database Migrations
```powershell
cd src/backend/Modules/Competition/BeerCompetition.Competition.Infrastructure
dotnet ef database update --startup-project ../../../Host/BeerCompetition.Host
```

#### 5. Insert Development Tenant
```powershell
cd ../../../..  # Return to src/backend
.\Insert-DevelopmentTenant.ps1
```

#### 6. Run Backend Host
```powershell
cd Host/BeerCompetition.Host
dotnet run

# API available at: https://localhost:5001
# Swagger UI: https://localhost:5001/swagger
```

#### 7. Run BFF API Gateway (Optional - for token exchange testing)
```powershell
# Open new terminal
cd src/backend/BFF/BFF.ApiGateway
dotnet run

# BFF available at: https://localhost:5190
```

#### 8. Run Integration Tests
```powershell
# Requires Docker running for Testcontainers
cd tests/Modules/Competition/BeerCompetition.Competition.IntegrationTests
dotnet test

# Expected: All tests passing (RegisterOrganizerIntegrationTests: 4/4 pass)
```

### Management Interfaces

- **Keycloak Admin Console**: http://localhost:8080/admin
  - Login: `admin` / `<your-password>`
  - Realm: `beercomp`
  - Clients: `frontend-spa`, `bff-api`, `competition-service`, `judging-service`

- **RabbitMQ Management**: http://localhost:15672
  - Login: `<your-username>` / `<your-password>`
  - View queues, exchanges, messages

- **PostgreSQL**: `localhost:5432`
  - Database: `<your-database-name>`
  - Use `psql` or pgAdmin to connect
### Stopping Services

```powershell
# Stop all services (preserves data in volumes)
cd infrastructure
docker-compose stop

# Stop and remove containers (preserves data)
docker-compose down

# DANGER: Remove all data (reset to clean state)
docker-compose down -v
```

---

## Documentation

### Architecture & Design
- [Architecture Overview](docs/architecture/ARCHITECTURE.md) - Complete system design
- [Architecture Decision Records](docs/architecture/decisions/) - ADRs documenting key decisions:
  - ✅ [ADR-001: Tech Stack Selection](docs/architecture/decisions/ADR-001-tech-stack-selection.md)
  - ✅ [ADR-002: Multi-Tenancy Strategy](docs/architecture/decisions/ADR-002-multi-tenancy-strategy.md)
  - ✅ [ADR-003: Event-Driven Architecture](docs/architecture/decisions/ADR-003-event-driven-architecture.md)
  - ✅ [ADR-004: Authentication & Authorization](docs/architecture/decisions/ADR-004-authentication-authorization.md)
  - ✅ [ADR-005: CQRS Implementation](docs/architecture/decisions/ADR-005-cqrs-implementation.md)
  - ✅ [ADR-006: Testing Strategy](docs/architecture/decisions/ADR-006-testing-strategy.md) ⭐ **UPDATED**
  - ✅ [ADR-007: Frontend Architecture](docs/architecture/decisions/ADR-007-frontend-architecture.md)
  - ✅ [ADR-008: Database Migrations Strategy](docs/architecture/decisions/ADR-008-database-migrations-strategy.md)
  - ✅ [ADR-009: Modular Monolith with Vertical Slices](docs/architecture/decisions/ADR-009-modular-monolith-vertical-slices.md)
  - ✅ [ADR-010: Token Exchange Pattern](docs/architecture/decisions/ADR-010-token-exchange-pattern.md) ⭐ **NEW**
- [Service Audiences & Token Exchange](docs/architecture/SERVICE_AUDIENCES_TOKEN_EXCHANGE.md) - Implementation guide

### Development
- [Solution Structure](docs/development/SOLUTION_STRUCTURE.md) ⭐ **UPDATED** - Project organization, testing infrastructure
- [Integration Tests README](tests/Modules/Competition/README.md) ⭐ **NEW** - Testcontainers + Respawn + Builder Pattern

### Product & Planning
- [MVP Definition](docs/roadmap/MVP_DEFINITION.md) - MVP features, acceptance criteria
- [Implementation Roadmap](docs/roadmap/IMPLEMENTATION_ROADMAP.md) ⭐ **UPDATED** - Sprint plan with progress tracking
- [MVP Issues Complete](docs/roadmap/MVP_ISSUES_COMPLETE.md) ⭐ **UPDATED** - Issue tracking (13 completed, 34 remaining)

### Operations
- [Deployment Guide](docs/deployment/DEPLOYMENT.md) - Docker Compose, Azure deployment
- [Secrets Management](infrastructure/SECRETS_MANAGEMENT.md) - Environment variables, Azure Key Vault

---

## Testing

### Unit Tests
```powershell
# Run unit tests only
dotnet test --filter "Category=Unit"
```

### Integration Tests ⭐ **IMPLEMENTED**
```powershell
# Requires Docker running for Testcontainers
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# Example: RegisterOrganizerIntegrationTests
cd tests/Modules/Competition/BeerCompetition.Competition.IntegrationTests
dotnet test --filter "FullyQualifiedName~RegisterOrganizerIntegrationTests"
```

**Integration Testing Infrastructure:**
- ✅ **Testcontainers**: Automatic PostgreSQL 16 container management
- ✅ **WebApplicationFactory**: In-memory API hosting with test configuration
- ✅ **Respawn**: Intelligent database cleanup (truncates data, preserves schema)
- ✅ **Builder Pattern**: Fluent builders for test data (TenantBuilder, CompetitionBuilder)
- ✅ **TestTenantProvider**: Dynamic tenant context switching during tests
- ✅ **Mock Services**: NSubstitute mocks for Keycloak and external services

See: [Integration Tests README](tests/Modules/Competition/README.md) for detailed guide

### E2E Tests
```powershell
# Cypress E2E tests (future)
cd frontend
npm run test:e2e
```

---

## Deployment

### Azure Production Deployment

**Prerequisites**:
- Azure subscription
- Azure CLI installed and authenticated
- Service principal with Contributor role

**Deploy Infrastructure**:
```bash
# Deploy Azure resources via Bicep
az deployment sub create \
  --location eastus \
  --template-file infrastructure/bicep/main.bicep \
  --parameters environment=prod postgresAdminPassword='<secure-password>'
```

**Deploy Services**:
- CI/CD pipeline automatically deploys on push to `main` branch
- See [.github/workflows/deploy.yml](.github/workflows/deploy.yml) for full pipeline


---

## Multi-Tenancy

**Isolation Model**: Hard data isolation via PostgreSQL Row-Level Security (RLS)

**Tenant Management**:
- Each organizer represents a tenant (organization)
- All tables include `tenant_id` column
- RLS policies enforce automatic filtering by session `tenant_id`
- BFF extracts `tenant_id` from Keycloak JWT and injects into all service requests

**Subscription Tiers** (future enhancement):
- FREE: 1 competition, 50 entries
- PRO: Unlimited competitions, 500 entries each
- ENTERPRISE: Custom limits, dedicated support

---

## Security

**Authentication**: OAuth2/OIDC via Keycloak; JWT-based API access.

**Authorization**: Role-based access control (RBAC) enforced at BFF and service layers.

**Data Protection**:
- TLS 1.3 for all communication
- Secrets stored in Azure Key Vault
- Database credentials rotated quarterly
- OWASP Top 10 compliance

**Blind Judging Enforcement**: Architectural guarantee that judging numbers never resolve to brewer identity in judge-facing APIs.

**Audit Logging**: All state changes logged to immutable `EventStore` table.

---

## Performance & Scale

**Current Scale**: 200 entrants, 50 concurrent judges, 600 bottles per competition.

**Scalability**:
- Stateless microservices scale horizontally
- PostgreSQL read replicas for heavy read workloads
- Redis caching for session state and scoresheet drafts
- RabbitMQ handles thousands of messages/second

**Load Testing**: Planned for 500 entrants, 100 judges, 1500 bottles (3x MVP scale).

---

## Observability

**Distributed Tracing**: OpenTelemetry instruments all services; traces exported to Azure Monitor.

**Metrics**:
- Request rate, latency (p50/p95/p99), error rate
- Database query performance
- Event bus throughput and lag
- Cache hit rate

**Dashboards**: Real-time dashboards in Azure Monitor for ops team.

**Alerts**: Configured for high error rates, latency spikes, queue depth, DB connection pool exhaustion.

---

## Contributing

**Branching Strategy**:
- `main`: Production-ready code
- `develop`: Integration branch
- Feature branches: `{issue-number}-{short-description}` (e.g., `16-entry-submission-api`)

**Branch Creation Workflow** (MANDATORY for all developers):
```bash
# Always start from main
git checkout main
git pull origin main

# Create branch using convention: {issue-number}-{short-description}
git checkout -b 16-entry-submission-api

# After implementation, commit with issue reference
git commit -m "feat: implement entry submission API (#16)"

# Push and create PR
git push -u origin 16-entry-submission-api
```

**Pull Request Process**:
1. Create feature branch from `main` using naming convention above
2. Write tests (unit + integration) - minimum 70% coverage
3. Open PR with clear description and link to issue
4. Automated tests run via GitHub Actions
5. Require 1 approval from code owner
6. Merge to `main` → production deployment

**Code Standards**:
- Follow .NET conventions (C#) and PEP 8 (Python)
- Conventional Commits for commit messages (feat:, fix:, docs:, etc.)
- Minimum 70% code coverage
- Always reference issue number in commits: `feat: description (#issue-number)`

---

## Project Management

### 📋 GitHub Project Setup

The project uses GitHub Projects for comprehensive tracking and organization:

**Project Board**: https://github.com/users/jesuscorral/projects/9

**Custom Fields**:
- **Status**: Backlog → Ready → In Progress → In Review → Done
- **Priority**: P0 (Critical), P1 (High), P2 (Medium)
- **Sprint**: Sprint 0-6, Post-MVP
- **Epic**: Infrastructure, Competitions, Entries, Flights, Scoring, Best of Show, Authentication, UI/Frontend, Observability
- **Complexity**: S (1-2 days), M (3-4 days), L (5-7 days)
- **Agent**: Backend, Frontend, DevOps, QA, Data Science, Product Owner

### 🏷️ Initial Project Setup

**First-time setup** (run once):

```powershell
# 1. Create all GitHub labels
cd c:\MyPersonalWS\beer-competition-saas
.\scripts\setup-github-labels.ps1

# 2. Update all existing issues with project fields
.\scripts\update-project-fields.ps1
```

**Testing changes** (dry run mode):
```powershell
# See what would change without applying
.\scripts\update-project-fields.ps1 -DryRun
```

### 🤖 Automated Issue Management

GitHub Actions automatically handle:
- **Auto-labeling**: Issues are labeled based on title/content
- **Project sync**: Issues are added to project board automatically
- **Field updates**: Sprint, Epic, Priority auto-assigned from labels

**Creating New Issues**:
1. Use [Feature Task template](.github/ISSUE_TEMPLATE/feature_task.yml)
2. Fill in all fields (Epic, Priority, Sprint, Complexity, Agent)
3. Issue is automatically added to project with fields set
4. Create feature branch: `{issue-number}-{short-description}`

**Bug Reports**:
1. Use [Bug Report template](.github/ISSUE_TEMPLATE/bug_report.yml)
2. Auto-labeled as `bug` and `triage`
3. Organizer will triage and assign priority/sprint

### 📊 Available Scripts

| Script | Purpose | Usage |
|--------|---------|-------|
| `setup-github-labels.ps1` | Create all project labels | `.\scripts\setup-github-labels.ps1` |
| `update-project-fields.ps1` | Bulk update project fields | `.\scripts\update-project-fields.ps1` |

**Script Requirements**:
- GitHub CLI (`gh`) installed and authenticated
- PowerShell 7+ (recommended)
- Permissions: `repo`, `project` scopes

---

## Roadmap

**v1.0 (MVP)** - Q2 2026:
- Core competition management
- Manual judge assignment
- Offline scoresheet capability
- Basic reporting

**v1.1** - Q3 2026:
- Analytics Service (ML-driven insights)
- Automated judge assignment suggestions
- Advanced reporting and dashboards
- Calendar integration (Google/Outlook)

**v2.0** - Q4 2026:
- Native mobile apps (iOS/Android)
- Multi-language support (Spanish, German, French)
- GDPR compliance tools (data export/deletion)
- Payment refund automation

**v3.0** - 2027:
- Event sourcing migration
- Multi-competition package pricing
- Judge certification tracking
- Style evolution tracking (BJCP updates)

---

## License

[MIT License](LICENSE) - Free for personal and commercial use.

---

## Support

**Documentation**: See [docs/](docs/) folder for comprehensive guides.

**Issues**: Report bugs or request features via [GitHub Issues](https://github.com/your-org/beer-competition-saas/issues).


---

## Acknowledgments

- **BJCP**: For standardized beer style guidelines and judging criteria.
- **Homebrew Community**: For feedback and beta testing.
- **Open Source Contributors**: For libraries and tools that made this possible.

---

**Project Status**: 🚧 In Development (MVP Target: Q2 2026)

**Version**: 0.1.0-alpha  
**Last Updated**: 2025-12-18
