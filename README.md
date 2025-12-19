# Beer Competition SaaS Platform

## Overview

A comprehensive, multi-tenant SaaS platform for managing BJCP 2021-compliant homebrew beer competitions. The system enforces blind judging, conflict-of-interest rules, and accurate scoring while supporting typical scales of 200 entrants, 50 concurrent judges, and 600 bottles per competition.

**Target Users**: Competition organizers, homebrewers (entrants), BJCP judges, stewards.

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
- **Backend**: .NET 10 (BFF + 2 Core Services: Competition, Judging)
- **Analytics** (Post-MVP): Python (FastAPI, ML capabilities)
- **Frontend**: React + Tailwind CSS (Progressive Web App)
- **Authentication**: Keycloak (OAuth2/OIDC)
- **Database**: PostgreSQL (multi-tenant with Row-Level Security)
- **Event Bus**: RabbitMQ
- **Cache**: Redis
- **Orchestration**: Docker + Docker Compose
- **Cloud**: Azure (Container Instances, PostgreSQL Flexible Server, Static Web Apps)
- **Observability**: OpenTelemetry + Azure Monitor
- **CI/CD**: GitHub Actions

**Architecture Pattern**: Microservices with event-driven communication; BFF (Backend-for-Frontend) as API gateway.

**Core Services**:
1. **BFF/API Gateway** (.NET 10): Authentication, authorization, routing, rate limiting
2. **Competition Service** (.NET 10): Competitions, entries, styles, payments, bottle check-in
3. **Judging Service** (.NET 10): Flights, judge assignments, scoresheets, BOS, consensus
4. **Analytics Service** (Python - Post-MVP): ML-driven insights, advanced reporting, judge assignment suggestions

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
- **.NET 10 SDK** (for backend services - future)
  - [Download .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Node.js 20+** and **npm** (for frontend - future)
  - [Download Node.js](https://nodejs.org/)
- **Git** for version control
- **PowerShell 7+** (recommended for Windows)

### Quick Start (Infrastructure Only - Sprint 0)

```powershell
# 1. Clone repository
git clone https://github.com/jesuscorral/beer-competition-saas.git
cd beer-competition-saas

# 2. Navigate to infrastructure folder
cd infrastructure

# 3. Copy environment template and configure
Copy-Item .env.example .env
# Edit .env with your preferences (default values work for local dev)

# 4. Start all infrastructure services
docker-compose up -d

# 5. Verify all services are healthy
docker-compose ps

# Expected output:
# NAME                   STATUS              PORTS
# beercomp_postgres      Up (healthy)        0.0.0.0:5432->5432/tcp
# beercomp_pgadmin       Up                  0.0.0.0:5050->80/tcp
# beercomp_rabbitmq      Up (healthy)        0.0.0.0:5672->5672/tcp, 0.0.0.0:15672->15672/tcp
# beercomp_redis         Up (healthy)        0.0.0.0:6379->6379/tcp
# beercomp_keycloak      Up (healthy)        0.0.0.0:8080->8080/tcp

# 6. Access management interfaces:
# - pgAdmin (PostgreSQL UI): http://localhost:5050
#   Login: admin@beercomp.dev / admin
# - RabbitMQ Management: http://localhost:15672
#   Login: dev_user / dev_password
# - Keycloak Admin Console: http://localhost:8080/admin
#   Login: admin / admin
```

### Initial Database Configuration (pgAdmin)

1. Open pgAdmin at http://localhost:5050
2. **Add Server Connection**:
   - Right-click "Servers" → Register → Server
   - **General Tab**:
     - Name: `Beer Competition Local`
   - **Connection Tab**:
     - Host: `postgres` (Docker internal network)
     - Port: `5432`
     - Database: `beercomp`
     - Username: `dev_user`
     - Password: `dev_password`
   - Click "Save"

3. You should now see the `beercomp` database and can browse tables (future: after migrations)

### Keycloak Initial Setup (Future - Sprint 1)

Keycloak admin console will be configured in Sprint 1 (AUTH-001 issue) with:
- Realm: `beercomp`
- Clients: `backend-api`, `frontend-web`
- Roles: `ORGANIZER`, `JUDGE`, `ENTRANT`, `STEWARD`
- Test users with various roles

### Stopping Services

```powershell
# Stop all services (preserves data in volumes)
docker-compose stop

# Stop and remove containers (preserves data)
docker-compose down

# DANGER: Remove all data (reset to clean state)
docker-compose down -v
```

### Troubleshooting

#### Port Conflicts

If you get port binding errors, check if ports are already in use:

```powershell
# Check PostgreSQL port (5432)
netstat -ano | findstr :5432

# Check RabbitMQ port (5672)
netstat -ano | findstr :5672

# Check Keycloak port (8080)
netstat -ano | findstr :8080
```

**Solution**: Either:
1. Stop the conflicting service
2. Edit `.env` file to use different ports

#### Keycloak Takes Long to Start

Keycloak typically takes 60-90 seconds on first startup (database schema initialization). Check progress:

```powershell
docker logs beercomp_keycloak --follow
```

Wait until you see: `Listening on: http://0.0.0.0:8080`

#### PostgreSQL Connection Refused

Ensure PostgreSQL is healthy before connecting:

```powershell
docker-compose ps postgres

# Check logs if not healthy
docker logs beercomp_postgres
```

#### RabbitMQ Management UI Not Loading

Wait for health check to pass (20 seconds), then check:

```powershell
docker logs beercomp_rabbitmq

# Verify management plugin is enabled (should see in logs):
# "Server startup complete; 4 plugins started: [rabbitmq_management, ...]"
```

#### Docker Compose Version Issues

This project requires Docker Compose v2 (included in Docker Desktop 4.x). Verify:

```powershell
docker compose version
# Should show: Docker Compose version v2.x.x
```

If using standalone Docker Compose v1 (legacy), upgrade to Docker Desktop.

#### WSL 2 Performance on Windows

For best performance, ensure project files are in WSL 2 filesystem (not `/mnt/c/`):

```bash
# From WSL terminal
cd ~
git clone https://github.com/jesuscorral/beer-competition-saas.git
cd beer-competition-saas/infrastructure
docker compose up -d
```

### Running Tests (Future - Sprint 0 Issue #7)

```bash
# .NET unit and integration tests (uses Testcontainers)
dotnet test

# Python tests (Post-MVP)
cd services/analytics
pytest

# End-to-end tests (Cypress)
cd tests/e2e
npm run test:e2e
```

### Logs and Monitoring

```powershell
# View all service logs
docker-compose logs --follow

# View specific service logs
docker-compose logs postgres --follow
docker-compose logs rabbitmq --follow

# View last 100 lines
docker-compose logs --tail=100
```

---

## Documentation

### Architecture & Design
- [Architecture Overview](docs/architecture/ARCHITECTURE.md) - Complete system design: stack, services, patterns, data model, APIs, algorithms, deployment

### AI Agents & Development
- [AI Agents Guide](docs/agents/README.md) - GitHub Copilot agents, MCP configuration, workflows, examples

### Deployment & Operations
- [Deployment Guide](docs/deployment/DEPLOYMENT.md) - Docker Compose, Azure Bicep, GitHub Actions CI/CD

### Product & Planning
- [MVP Definition](docs/MVP_DEFINITION.md) - MVP features, acceptance criteria, phasing
- [Backlog](docs/BACKLOG.md) - Prioritized tasks grouped by epic (54 tasks: 47 MVP + 7 Post-MVP)

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

**Manual Deployment**:
```bash
# Build and push Docker images
docker build -t beercompregistry.azurecr.io/bff:latest services/bff
docker push beercompregistry.azurecr.io/bff:latest

# Restart Azure Container Instances
az container restart --name beercomp-bff --resource-group rg-beercomp-prod
```

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
