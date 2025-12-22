# Infrastructure - Local Development Setup

## Overview

This folder contains the Docker Compose configuration for running the complete Beer Competition SaaS platform infrastructure locally.

## 🚀 Quick Start

**First time setup:**

```powershell
# 1. Copy the example environment file
Copy-Item .env.example .env

# 2. Edit .env with your credentials (replace all <placeholder> values)
# Use any text editor or VS Code

# 3. Validate environment configuration
.\validate-env.ps1

# 4. Start all services
docker-compose up -d

# 5. Verify services are healthy
docker-compose ps
```

**⚠️ IMPORTANT**: Never commit the `.env` file to version control! It's already in `.gitignore`.

## Environment Configuration

### Files Structure

```
infrastructure/
├── .env                # ← Your local credentials (git-ignored)
├── .env.example        # ← Template file (committed to git)
├── .gitignore          # ← Protects .env from being committed
├── docker-compose.yml  # ← Service definitions
└── README.md
```

### How Docker Compose Reads .env

Docker Compose **automatically reads** the `.env` file in the same directory. Variables are substituted using `${VARIABLE_NAME}` syntax:

```yaml
# In docker-compose.yml
environment:
  POSTGRES_USER: ${POSTGRES_USER}        # ← Reads from .env
  POSTGRES_PASSWORD: ${POSTGRES_PASSWORD} # ← Reads from .env
```

### Security Best Practices

1. ✅ **Always use `.env` file** - Never hardcode secrets in `docker-compose.yml`
2. ✅ **Copy from `.env.example`** - Replace all `<placeholder>` values
3. ✅ **Use strong passwords** - Even in local development
4. ❌ **Never commit `.env`** - Already protected by `.gitignore`
5. 🔒 **Production uses Azure Key Vault** - No `.env` files in production

### Example .env Setup

```bash
# Copy the template
cp .env.example .env

# Edit with your values
POSTGRES_PASSWORD=my_secure_password_123!
PGADMIN_PASSWORD=my_pgadmin_pass
RABBITMQ_PASSWORD=my_rabbitmq_pass
REDIS_PASSWORD=my_redis_pass
KEYCLOAK_ADMIN_PASSWORD=my_keycloak_pass
```

## Services Included

### Core Services

1. **PostgreSQL 16** (`postgres`)
   - Port: 5432
   - Database: `beercomp`
   - Credentials: Configured in `.env` file
   - Features: Multi-tenant data isolation with Row-Level Security (future)

2. **pgAdmin 4** (`pgadmin`)
   - Port: 5050 → http://localhost:5050
   - Web-based PostgreSQL management interface
   - Credentials: Configured in `.env` file

3. **RabbitMQ 3.12** (`rabbitmq`)
   - AMQP Port: 5672
   - Management UI Port: 15672 → http://localhost:15672
   - Credentials: Configured in `.env` file
   - Features: Topic exchanges, durable queues, DLQ support

4. **Redis 7** (`redis`)
   - Port: 6379
   - Credentials: Configured in `.env` file
   - Purpose: Distributed caching, session storage

5. **Keycloak 23** (`keycloak`)
   - Port: 8080 → http://localhost:8080
   - Admin Console: http://localhost:8080/admin
   - Credentials: Configured in `.env` file
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

## Service Dependencies

Health checks ensure proper startup order:

1. **PostgreSQL** starts first (required by Keycloak and pgAdmin)
2. **pgAdmin** waits for PostgreSQL health check
3. **Keycloak** waits for PostgreSQL health check
4. **RabbitMQ** and **Redis** start independently

## Accessing Services

All service credentials are defined in your `.env` file:

| Service | URL | Default Credentials (.env) |
|---------|-----|----------------------------|
| PostgreSQL | `localhost:5432` | `${POSTGRES_USER}` / `${POSTGRES_PASSWORD}` |
| pgAdmin | http://localhost:5050 | `${PGADMIN_EMAIL}` / `${PGADMIN_PASSWORD}` |
| RabbitMQ AMQP | `localhost:5672` | `${RABBITMQ_USER}` / `${RABBITMQ_PASSWORD}` |
| RabbitMQ Management | http://localhost:15672 | Same as AMQP |
| Redis | `localhost:6379` | Password: `${REDIS_PASSWORD}` |
| Keycloak Admin | http://localhost:8080/admin | `${KEYCLOAK_ADMIN}` / `${KEYCLOAK_ADMIN_PASSWORD}` |

**Connection String Example** (for .NET applications):

```csharp
// PostgreSQL
"Host=localhost;Port=5432;Database=beer_competition;Username=<POSTGRES_USER>;Password=<POSTGRES_PASSWORD>"

// Redis
"localhost:6379,password=<REDIS_PASSWORD>"

// RabbitMQ
"amqp://<RABBITMQ_USER>:<RABBITMQ_PASSWORD>@localhost:5672/"
```

## Troubleshooting

### Services won't start

```powershell
# Check if .env file exists
Test-Path .env

# Verify environment variables are loaded
docker-compose config

# Check for port conflicts
netstat -ano | findstr "5432 5672 6379 8080"
```

### Permission issues with volumes

```powershell
# On Windows, ensure Docker has access to drives
# Docker Desktop → Settings → Resources → File Sharing
```

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

**Related Documentation:**
- [Secrets Management Guide](SECRETS_MANAGEMENT.md) - Comprehensive guide on managing secrets
- [ADR-001: Tech Stack Selection](../docs/architecture/decisions/ADR-001-tech-stack-selection.md)
- [ADR-002: Multi-Tenancy Strategy](../docs/architecture/decisions/ADR-002-multi-tenancy-strategy.md)
- [ADR-003: Event-Driven Architecture](../docs/architecture/decisions/ADR-003-event-driven-architecture.md)

**Sprint:** Sprint 0 - Foundation  
**Issue:** [#2 - Docker Compose Development Environment](https://github.com/jesuscorral/beer-competition-saas/issues/2)
