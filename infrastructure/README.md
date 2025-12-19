# Infrastructure - Local Development Setup

## Overview

This folder contains the Docker Compose configuration for running the complete Beer Competition SaaS platform infrastructure locally.

## Services Included

### Core Services

1. **PostgreSQL 17** (`postgres`)
   - Port: 5432
   - Database: `beer_competition`
   - User: `dev_user`
   - Features: Multi-tenant data isolation with Row-Level Security (future)

2. **pgAdmin 4** (`pgadmin`)
   - Port: 5050 → http://localhost:5050
   - Web-based PostgreSQL management interface
   - Login: `admin@beercomp.local` / `admin`

3. **RabbitMQ 3.12** (`rabbitmq`)
   - AMQP Port: 5672
   - Management UI Port: 15672 → http://localhost:15672
   - User: `dev_user` / `dev_password`
   - Features: Topic exchanges, durable queues, DLQ support

4. **Redis 7** (`redis`)
   - Port: 6379
   - Password: `dev_password`
   - Purpose: Distributed caching, session storage

5. **Keycloak 23** (`keycloak`)
   - Port: 8080 → http://localhost:8080
   - Admin Console: http://localhost:8080/admin
   - Admin User: `admin` / `admin`
   - Database: Stores config in PostgreSQL (shared with main app)

## Quick Commands

```powershell
# Start all services
docker-compose up -d

# Check service health
docker-compose ps

# View logs (all services)
docker-compose logs --follow

# View logs (specific service)
docker-compose logs postgres --follow
docker-compose logs rabbitmq --follow

# Stop services (keep data)
docker-compose stop

# Stop and remove containers (keep data)
docker-compose down

# Reset all data (DANGER: deletes volumes)
docker-compose down -v

# Restart single service
docker-compose restart postgres

# Rebuild and restart
docker-compose up -d --build
```

## Data Persistence

All data is stored in Docker volumes:

- `beercomp_postgres_data` - PostgreSQL database files
- `beercomp_pgadmin_data` - pgAdmin configuration
- `beercomp_rabbitmq_data` - RabbitMQ message store
- `beercomp_rabbitmq_log` - RabbitMQ logs
- `beercomp_redis_data` - Redis snapshots

**To inspect volumes:**

```powershell
# List volumes
docker volume ls | Select-String beercomp

# Inspect specific volume
docker volume inspect beercomp_postgres_data

# Remove all volumes (DANGER: data loss)
docker volume rm beercomp_postgres_data beercomp_pgadmin_data beercomp_rabbitmq_data beercomp_rabbitmq_log beercomp_redis_data
```

## Network Configuration

All services communicate via the `beercomp-network` bridge network. This allows:

- Services to resolve each other by container name (e.g., `postgres`, `rabbitmq`)
- Future application services to connect without exposing ports externally

## Environment Variables

**Configuration files:**

- `.env` - Active environment variables (git-ignored)

**Security Note:** Never commit `.env` to version control.

## Service Dependencies

Health checks ensure proper startup order:

1. **PostgreSQL** starts first (required by Keycloak and pgAdmin)
2. **pgAdmin** waits for PostgreSQL health check
3. **Keycloak** waits for PostgreSQL health check
4. **RabbitMQ** and **Redis** start independently

## Accessing Services

| Service | URL | Credentials |
|---------|-----|-------------|
| PostgreSQL | `localhost:5432` | `dev_user` / `dev_password` |
| pgAdmin | http://localhost:5050 | `admin@beercomp.dev` / `admin` |
| RabbitMQ AMQP | `localhost:5672` | `dev_user` / `dev_password` |
| RabbitMQ Management | http://localhost:15672 | `dev_user` / `dev_password` |
| Redis | `localhost:6379` | Password: `dev_password` |
| Keycloak Admin | http://localhost:8080/admin | `admin` / `admin` |

### Reset Everything

```powershell
# Nuclear option: remove all containers and volumes
docker-compose down -v

# Remove stopped containers
docker container prune -f

# Remove unused volumes
docker volume prune -f

# Start fresh
docker-compose up -d
```

## Production Differences

This Docker Compose setup is for **LOCAL DEVELOPMENT ONLY**. Production deployment uses:

- **Azure Database for PostgreSQL** (Flexible Server)
- **Azure Service Bus** or **self-hosted RabbitMQ cluster** (HA)
- **Azure Cache for Redis**
- **Azure AD B2C** or **Keycloak on Azure Container Instances**
- **TLS/SSL** for all connections
- **Azure Key Vault** for secrets management
- **Managed identities** instead of passwords

See [docs/deployment/DEPLOYMENT.md](../docs/deployment/DEPLOYMENT.md) for production setup.


---

**Related ADRs:**
- [ADR-001: Tech Stack Selection](../docs/architecture/decisions/ADR-001-tech-stack-selection.md)
- [ADR-002: Multi-Tenancy Strategy](../docs/architecture/decisions/ADR-002-multi-tenancy-strategy.md)
- [ADR-003: Event-Driven Architecture](../docs/architecture/decisions/ADR-003-event-driven-architecture.md)

**Sprint:** Sprint 0 - Foundation  
**Issue:** [#2 - Docker Compose Development Environment](https://github.com/jesuscorral/beer-competition-saas/issues/2)
