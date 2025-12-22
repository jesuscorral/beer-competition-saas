# Secrets Management Guide

## Overview

This document explains how secrets (passwords, API keys, connection strings) are managed in the Beer Competition SaaS platform across different environments.

---

## Local Development

### Configuration Files

```
infrastructure/
├── .env              # ← Your secrets (git-ignored)
├── .env.example      # ← Template (committed)
├── .gitignore        # ← Protects .env
└── validate-env.ps1  # ← Validation script
```

### Setup Process

1. **Copy template**:
   ```powershell
   Copy-Item .env.example .env
   ```

2. **Edit .env** - Replace all `<placeholder>` values:
   ```bash
   POSTGRES_PASSWORD=<your-secure-password>  # Replace this
   REDIS_PASSWORD=<your-secure-password>     # And this
   ```

3. **Validate**:
   ```powershell
   .\validate-env.ps1
   ```

4. **Start services**:
   ```powershell
   docker-compose up -d
   ```

### How Docker Compose Reads Secrets

Docker Compose **automatically reads** `.env` file in the same directory:

```yaml
# docker-compose.yml
services:
  postgres:
    environment:
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}  # ← Read from .env
```

**Variable Substitution**:
- `${VAR}` - Required, fails if not set
- `${VAR:-default}` - Optional, uses default if not set
- `${VAR-default}` - Uses default only if VAR is unset (not if empty)

### Security Best Practices

✅ **DO**:
- Use `.env` file for all secrets
- Copy from `.env.example` 
- Use strong passwords (even locally)
- Run `validate-env.ps1` before starting services
- Keep `.env` out of version control (already in `.gitignore`)

❌ **DON'T**:
- Hardcode secrets in `docker-compose.yml`
- Commit `.env` to git
- Share `.env` file via email/chat
- Use production secrets in local development
- Copy `.env` to production servers

### Troubleshooting

**Problem**: Variables not being substituted

```powershell
# Check if .env exists
Test-Path .env

# View resolved configuration
docker-compose config

# Check specific variable
docker-compose config | Select-String "POSTGRES_PASSWORD"
```

**Problem**: Services can't connect

```powershell
# Validate all variables are set
.\validate-env.ps1

# Check environment variables inside container
docker-compose exec postgres env | Select-String POSTGRES
```

---

## Production (Azure)

### Azure Key Vault

Production secrets are stored in **Azure Key Vault**:

```
beercomp-keyvault-prod
├── PostgreSQL-ConnectionString
├── Redis-ConnectionString
├── RabbitMQ-ConnectionString
├── ServiceBus-ConnectionString
├── ApplicationInsights-InstrumentationKey
└── Jwt-SigningKey
```

### Access via Managed Identity

Applications use **Azure Managed Identity** (no passwords needed):

```csharp
// appsettings.json
{
  "KeyVault": {
    "VaultUri": "https://beercomp-keyvault-prod.vault.azure.net/"
  }
}

// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri(builder.Configuration["KeyVault:VaultUri"]),
    new DefaultAzureCredential()
);

// Usage - secrets automatically loaded
var connectionString = builder.Configuration["PostgreSQL-ConnectionString"];
```

### Secret Rotation

Secrets are rotated every **90 days**:

1. Generate new secret in Azure Key Vault
2. Create new version (old version remains active)
3. Update applications to use new version
4. Verify all services working
5. Disable old version
6. Delete old version after 30 days

### Access Control

Access to Key Vault is restricted via **RBAC**:

- **Key Vault Secrets Officer**: DevOps team (read/write)
- **Key Vault Secrets User**: App Services (read-only via Managed Identity)
- **Key Vault Administrator**: Security team only

### Audit

All secret access is logged in **Azure Monitor**:

```kusto
AzureDiagnostics
| where ResourceType == "VAULTS"
| where OperationName == "SecretGet"
| project TimeGenerated, CallerIPAddress, identity_claim_appid_g, ResultSignature
```

---

## CI/CD Pipeline (GitHub Actions)

### GitHub Secrets

Pipeline secrets are stored in **GitHub Secrets**:

```
Repository → Settings → Secrets and variables → Actions
├── AZURE_CREDENTIALS
├── ACR_USERNAME
├── ACR_PASSWORD
├── AZURE_SUBSCRIPTION_ID
└── KEYVAULT_NAME
```

### Usage in Workflows

```yaml
# .github/workflows/deploy-backend.yml
jobs:
  deploy:
    steps:
      - name: Azure Login
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      
      - name: Get Secrets from Key Vault
        id: keyvault
        run: |
          CONNECTION_STRING=$(az keyvault secret show \
            --vault-name ${{ secrets.KEYVAULT_NAME }} \
            --name PostgreSQL-ConnectionString \
            --query value -o tsv)
          echo "::add-mask::$CONNECTION_STRING"
```

### Secret Masking

GitHub Actions **automatically masks** secrets in logs:

```bash
# Input
echo ${{ secrets.POSTGRES_PASSWORD }}

# Log output (masked)
echo ***
```

---

## Environment Comparison

| Aspect | Local Development | Production (Azure) |
|--------|-------------------|-------------------|
| **Storage** | `.env` file | Azure Key Vault |
| **Access** | File system | Managed Identity |
| **Rotation** | Manual | Automated (90 days) |
| **Audit** | None | Azure Monitor |
| **Encryption** | None | At rest + in transit |
| **Backup** | None | Geo-redundant |
| **Access Control** | File permissions | Azure RBAC |

---

## Common Patterns

### Connection Strings

**Local** (from `.env`):
```bash
# PostgreSQL
Host=localhost;Port=5432;Database=beer_competition;Username=dev_user;Password=dev_password

# Redis
localhost:6379,password=dev_password

# RabbitMQ
amqp://dev_user:dev_password@localhost:5672/
```

**Production** (from Key Vault):
```bash
# PostgreSQL (Azure Database)
Host=beercomp-db-prod.postgres.database.azure.com;Port=5432;Database=beer_competition;Username=app_user;Password=***;SslMode=Require

# Redis (Azure Cache)
beercomp-redis-prod.redis.cache.windows.net:6380,password=***,ssl=True

# Service Bus (replaces RabbitMQ)
Endpoint=sb://beercomp-sb-prod.servicebus.windows.net/;SharedAccessKeyName=***;SharedAccessKey=***
```

### Environment-Specific Configuration

```csharp
// appsettings.Development.json (local)
{
  "ConnectionStrings": {
    "PostgreSQL": "Host=localhost;Port=5432;..."  // Read from .env
  }
}

// appsettings.Production.json (Azure)
{
  "ConnectionStrings": {
    "PostgreSQL": "@Microsoft.KeyVault(SecretUri=https://...)"  // Key Vault reference
  }
}
```

---

## Security Checklist

### Local Development

- [ ] `.env` file exists and is configured
- [ ] `.env` is in `.gitignore`
- [ ] `validate-env.ps1` passes without errors
- [ ] No secrets in `docker-compose.yml`
- [ ] Strong passwords used (even locally)

### Production

- [ ] All secrets in Azure Key Vault
- [ ] Managed Identity configured for all services
- [ ] RBAC permissions correctly set
- [ ] Secret rotation policy active (90 days)
- [ ] Audit logging enabled in Azure Monitor
- [ ] No hardcoded secrets in code or config files

### CI/CD

- [ ] GitHub Secrets configured
- [ ] Azure credentials have minimum required permissions
- [ ] Secrets are masked in logs
- [ ] No secrets in repository files
- [ ] Deployment scripts use Key Vault references

---

## Related Documentation

- [ADR-004: Authentication & Authorization](../../docs/architecture/decisions/ADR-004-authentication-authorization.md)
- [Deployment Guide](../../docs/deployment/DEPLOYMENT.md)
- [Infrastructure README](README.md)

---

**Last Updated**: 2025-12-22  
**Owner**: DevOps Team
