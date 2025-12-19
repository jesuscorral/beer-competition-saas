# Deployment & Infrastructure

## Overview

This document provides high-level deployment skeletons for the Beer Competition SaaS platform, including Docker Compose orchestration, GitHub Actions CI/CD pipeline, and Azure Bicep infrastructure-as-code snippets.

---

## 1. Docker Compose (Development & Small Production)

**File**: `infrastructure/docker-compose.yml`

```yaml
version: '3.9'

services:
  # Frontend - React SPA
  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    ports:
      - "3000:80"
    environment:
      - REACT_APP_API_URL=http://localhost:5000
      - REACT_APP_KEYCLOAK_URL=http://localhost:8080
    depends_on:
      - bff
    networks:
      - beercomp-network

  # BFF / API Gateway - .NET 10
  bff:
    build:
      context: ./services/bff
      dockerfile: Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__PostgreSQL=Host=postgres;Port=5432;Database=beercomp;Username=beercomp_user;Password=${DB_PASSWORD}
      - Keycloak__Authority=http://keycloak:8080/realms/beercomp
      - Keycloak__ClientId=bff-client
      - Keycloak__ClientSecret=${KEYCLOAK_CLIENT_SECRET}
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Port=5672
      - RabbitMQ__Username=guest
      - RabbitMQ__Password=${RABBITMQ_PASSWORD}
      - Redis__Host=redis
      - Redis__Port=6379
      - OpenTelemetry__Endpoint=http://otel-collector:4317
    depends_on:
      - postgres
      - rabbitmq
      - redis
      - keycloak
    networks:
      - beercomp-network

  # Keycloak - Identity Provider
  keycloak:
    image: quay.io/keycloak/keycloak:23.0
    command: start-dev
    ports:
      - "8080:8080"
    environment:
      - KEYCLOAK_ADMIN=admin
      - KEYCLOAK_ADMIN_PASSWORD=${KEYCLOAK_ADMIN_PASSWORD}
      - KC_DB=postgres
      - KC_DB_URL=jdbc:postgresql://postgres:5432/keycloak
      - KC_DB_USERNAME=keycloak_user
      - KC_DB_PASSWORD=${DB_PASSWORD}
    depends_on:
      - postgres
    networks:
      - beercomp-network

  # Competition Service - .NET 10 (Core business logic + Bottle check-in)
  service-competition:
    build:
      context: ./services/competition
      dockerfile: Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__PostgreSQL=Host=postgres;Port=5432;Database=beercomp;Username=beercomp_user;Password=${DB_PASSWORD}
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Port=5672
      - OpenTelemetry__Endpoint=http://otel-collector:4317
      - Stripe__SecretKey=${STRIPE_SECRET_KEY}
    depends_on:
      - postgres
      - rabbitmq
    networks:
      - beercomp-network

  # Judging Service - .NET 10 (Flights, Scoresheets, BOS)
  service-judging:
    build:
      context: ./services/judging
      dockerfile: Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__PostgreSQL=Host=postgres;Port=5432;Database=beercomp;Username=beercomp_user;Password=${DB_PASSWORD}
      - RabbitMQ__Host=rabbitmq
      - RabbitMQ__Port=5672
      - OpenTelemetry__Endpoint=http://otel-collector:4317
    depends_on:
      - postgres
      - rabbitmq
    networks:
      - beercomp-network

  # Analytics Service - Python (Post-MVP, optional for local dev)
  # service-analytics:
  #   build:
  #     context: ./services/analytics
  #     dockerfile: Dockerfile
  #   environment:
  #     - DATABASE_URL=postgresql://beercomp_user:${DB_PASSWORD}@postgres:5432/beercomp
  #     - RABBITMQ_HOST=rabbitmq
  #     - RABBITMQ_PORT=5672
  #     - OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4317
  #   depends_on:
  #     - postgres
  #     - rabbitmq
  #   networks:
  #     - beercomp-network

  # PostgreSQL - Primary Database
  postgres:
    image: postgres:16-alpine
    ports:
      - "5432:5432"
    environment:
      - POSTGRES_DB=beercomp
      - POSTGRES_USER=beercomp_user
      - POSTGRES_PASSWORD=${DB_PASSWORD}
    volumes:
      - postgres-data:/var/lib/postgresql/data
      - ./scripts/init-db.sql:/docker-entrypoint-initdb.d/init.sql
    networks:
      - beercomp-network

  # RabbitMQ - Event Bus
  rabbitmq:
    image: rabbitmq:3.13-management-alpine
    ports:
      - "5672:5672"
      - "15672:15672"  # Management UI
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=${RABBITMQ_PASSWORD}
    volumes:
      - rabbitmq-data:/var/lib/rabbitmq
    networks:
      - beercomp-network

  # Redis - Cache & Session Store
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    networks:
      - beercomp-network

  # OpenTelemetry Collector
  otel-collector:
    image: otel/opentelemetry-collector-contrib:0.91.0
    command: ["--config=/etc/otel-collector-config.yaml"]
    volumes:
      - ./infrastructure/otel-collector-config.yaml:/etc/otel-collector-config.yaml
    ports:
      - "4317:4317"  # OTLP gRPC
      - "4318:4318"  # OTLP HTTP
    environment:
      - AZURE_MONITOR_CONNECTION_STRING=${AZURE_MONITOR_CONNECTION_STRING}
    networks:
      - beercomp-network

volumes:
  postgres-data:
  rabbitmq-data:
  redis-data:

networks:
  beercomp-network:
    driver: bridge
```

**Environment Variables** (`.env` file):
```bash
DB_PASSWORD=<secure-password>
KEYCLOAK_ADMIN_PASSWORD=<admin-password>
KEYCLOAK_CLIENT_SECRET=<client-secret>
RABBITMQ_PASSWORD=<rabbitmq-password>
AZURE_MONITOR_CONNECTION_STRING=<azure-app-insights-connection-string>
```

**Commands**:
```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f bff

# Stop all services
docker-compose down

# Rebuild and restart
docker-compose up -d --build
```

---

## 2. GitHub Actions CI/CD Pipeline

**File**: `.github/workflows/deploy.yml`

```yaml
name: CI/CD Pipeline

on:
  push:
    branches:
      - main
      - develop
  pull_request:
    branches:
      - main

env:
  AZURE_CONTAINER_REGISTRY: beercompregistry.azurecr.io
  RESOURCE_GROUP: rg-beercomp-prod
  AZURE_REGION: eastus

jobs:
  # Job 1: Build and Test
  build-and-test:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET 10
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Setup Python 3.12
        uses: actions/setup-python@v5
        with:
          python-version: '3.12'

      - name: Restore .NET dependencies
        run: |
          dotnet restore services/bff/bff.csproj
          dotnet restore services/competition/competition.csproj
          dotnet restore services/judging/judging.csproj

      - name: Build .NET services
        run: |
          dotnet build --configuration Release --no-restore services/bff/bff.csproj
          dotnet build --configuration Release --no-restore services/competition/competition.csproj
          dotnet build --configuration Release --no-restore services/judging/judging.csproj

      - name: Run .NET tests
        run: |
          dotnet test --no-build --configuration Release --verbosity normal tests/**/*.csproj

      - name: Install Python dependencies (Analytics - Post-MVP)
        working-directory: services/analytics
        run: |
          pip install -r requirements.txt
        if: false  # Disabled for MVP

      - name: Run Python tests (Analytics - Post-MVP)
        working-directory: services/analytics
        run: |
          pytest tests/
        if: false  # Disabled for MVP

  # Job 2: Build and Push Docker Images
  build-images:
    runs-on: ubuntu-latest
    needs: build-and-test
    if: github.ref == 'refs/heads/main'
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Log in to Azure Container Registry
        uses: docker/login-action@v3
        with:
          registry: ${{ env.AZURE_CONTAINER_REGISTRY }}
          username: ${{ secrets.ACR_USERNAME }}
          password: ${{ secrets.ACR_PASSWORD }}

      - name: Build and push BFF image
        uses: docker/build-push-action@v5
        with:
          context: ./services/bff
          push: true
          tags: ${{ env.AZURE_CONTAINER_REGISTRY }}/bff:${{ github.sha }},${{ env.AZURE_CONTAINER_REGISTRY }}/bff:latest

      - name: Build and push Competition Service image
        uses: docker/build-push-action@v5
        with:
          context: ./services/competition
          push: true
          tags: ${{ env.AZURE_CONTAINER_REGISTRY }}/competition:${{ github.sha }},${{ env.AZURE_CONTAINER_REGISTRY }}/competition:latest

      - name: Build and push Judging Service image
        uses: docker/build-push-action@v5
        with:
          context: ./services/judging
          push: true
          tags: ${{ env.AZURE_CONTAINER_REGISTRY }}/judging:${{ github.sha }},${{ env.AZURE_CONTAINER_REGISTRY }}/judging:latest

      # Analytics Service - Post-MVP
      # - name: Build and push Analytics Service image
      #   uses: docker/build-push-action@v5
      #   with:
      #     context: ./services/analytics
      #     push: true
      #     tags: ${{ env.AZURE_CONTAINER_REGISTRY }}/analytics:${{ github.sha }},${{ env.AZURE_CONTAINER_REGISTRY }}/analytics:latest

      - name: Build and push Frontend image
        uses: docker/build-push-action@v5
        with:
          context: ./frontend
          push: true
          tags: ${{ env.AZURE_CONTAINER_REGISTRY }}/frontend:${{ github.sha }},${{ env.AZURE_CONTAINER_REGISTRY }}/frontend:latest

  # Job 3: Run Database Migrations
  migrate-database:
    runs-on: ubuntu-latest
    needs: build-images
    if: github.ref == 'refs/heads/main'
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET 10
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Install EF Core CLI
        run: dotnet tool install --global dotnet-ef

      - name: Run migrations
        env:
          DATABASE_CONNECTION_STRING: ${{ secrets.PROD_DATABASE_CONNECTION_STRING }}
        run: |
          dotnet ef database update --project services/competition/competition.csproj --connection "$DATABASE_CONNECTION_STRING"
          dotnet ef database update --project services/judging/judging.csproj --connection "$DATABASE_CONNECTION_STRING"

  # Job 4: Deploy to Azure
  deploy-azure:
    runs-on: ubuntu-latest
    needs: migrate-database
    if: github.ref == 'refs/heads/main'
    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Azure Login
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Deploy to Azure Container Instances
        run: |
          az container restart --name beercomp-bff --resource-group ${{ env.RESOURCE_GROUP }}
          az container restart --name beercomp-competition --resource-group ${{ env.RESOURCE_GROUP }}
          az container restart --name beercomp-judging --resource-group ${{ env.RESOURCE_GROUP }}
          # beercomp-analytics not part of MVP

      - name: Deploy Frontend to Azure Static Web Apps
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
          repo_token: ${{ secrets.GITHUB_TOKEN }}
          action: "upload"
          app_location: "/frontend"
          output_location: "build"
```

**Secrets Configuration** (GitHub Repository Secrets):
- `ACR_USERNAME` / `ACR_PASSWORD`: Azure Container Registry credentials.
- `AZURE_CREDENTIALS`: Azure service principal JSON for `az login`.
- `PROD_DATABASE_CONNECTION_STRING`: PostgreSQL connection string.
- `AZURE_STATIC_WEB_APPS_API_TOKEN`: Deploy token for Static Web App.

---

## 3. Azure Bicep Infrastructure (Initial Version)

**File**: `infrastructure/bicep/main.bicep`

```bicep
targetScope = 'subscription'

@description('Environment name (dev, staging, prod)')
param environment string = 'prod'

@description('Azure region for resources')
param location string = 'eastus'

@description('Administrator password for PostgreSQL')
@secure()
param postgresAdminPassword string

// Resource Group
resource resourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: 'rg-beercomp-${environment}'
  location: location
}

// Azure Container Registry
module containerRegistry 'modules/container-registry.bicep' = {
  scope: resourceGroup
  name: 'containerRegistry'
  params: {
    name: 'beercompregistry${environment}'
    location: location
    sku: 'Standard'
  }
}

// Azure Database for PostgreSQL Flexible Server
module postgresServer 'modules/postgres-server.bicep' = {
  scope: resourceGroup
  name: 'postgresServer'
  params: {
    serverName: 'beercomp-postgres-${environment}'
    location: location
    administratorLogin: 'beercomp_admin'
    administratorPassword: postgresAdminPassword
    skuName: 'Standard_B2s'
    storageSizeGB: 128
  }
}

// Azure Container Instances (BFF)
module bffContainer 'modules/container-instance.bicep' = {
  scope: resourceGroup
  name: 'bffContainer'
  params: {
    name: 'beercomp-bff'
    location: location
    imageName: '${containerRegistry.outputs.loginServer}/bff:latest'
    cpu: 1
    memoryInGB: 2
    environmentVariables: [
      {
        name: 'ASPNETCORE_ENVIRONMENT'
        value: 'Production'
      }
      {
        name: 'ConnectionStrings__PostgreSQL'
        secureValue: postgresServer.outputs.connectionString
      }
    ]
    registryServer: containerRegistry.outputs.loginServer
    registryUsername: containerRegistry.outputs.username
    registryPassword: containerRegistry.outputs.password
  }
}

// Azure Container Instances (Competition Service)
module competitionServiceContainer 'modules/container-instance.bicep' = {
  scope: resourceGroup
  name: 'competitionServiceContainer'
  params: {
    name: 'beercomp-competition'
    location: location
    imageName: '${containerRegistry.outputs.loginServer}/competition:latest'
    cpu: 1
    memoryInGB: 2
    environmentVariables: [
      {
        name: 'ASPNETCORE_ENVIRONMENT'
        value: 'Production'
      }
      {
        name: 'ConnectionStrings__PostgreSQL'
        secureValue: postgresServer.outputs.connectionString
      }
    ]
    registryServer: containerRegistry.outputs.loginServer
    registryUsername: containerRegistry.outputs.username
    registryPassword: containerRegistry.outputs.password
  }
}

// Azure Container Instances (Judging Service)
module judgingServiceContainer 'modules/container-instance.bicep' = {
  scope: resourceGroup
  name: 'judgingServiceContainer'
  params: {
    name: 'beercomp-judging'
    location: location
    imageName: '${containerRegistry.outputs.loginServer}/judging:latest'
    cpu: 1
    memoryInGB: 2
    environmentVariables: [
      {
        name: 'ASPNETCORE_ENVIRONMENT'
        value: 'Production'
      }
      {
        name: 'ConnectionStrings__PostgreSQL'
        secureValue: postgresServer.outputs.connectionString
      }
    ]
    registryServer: containerRegistry.outputs.loginServer
    registryUsername: containerRegistry.outputs.username
    registryPassword: containerRegistry.outputs.password
  }
}

// Analytics Service - Post-MVP
// module analyticsServiceContainer 'modules/container-instance.bicep' = {
//   scope: resourceGroup
//   name: 'analyticsServiceContainer'
//   params: {
//     name: 'beercomp-analytics'
//     location: location
//     imageName: '${containerRegistry.outputs.loginServer}/analytics:latest'
//     cpu: 1
//     memoryInGB: 2
//     environmentVariables: [
//       {
//         name: 'ENVIRONMENT'
//         value: 'production'
//       }
//       {
//         name: 'DATABASE_URL'
//         secureValue: postgresServer.outputs.connectionString
//       }
//     ]
//     registryServer: containerRegistry.outputs.loginServer
//     registryUsername: containerRegistry.outputs.username
//     registryPassword: containerRegistry.outputs.password
//   }
// }

// Azure Static Web Apps (Frontend)
module staticWebApp 'modules/static-web-app.bicep' = {
  scope: resourceGroup
  name: 'staticWebApp'
  params: {
    name: 'beercomp-frontend-${environment}'
    location: location
    sku: 'Standard'
  }
}

// Azure Monitor / Application Insights
module appInsights 'modules/app-insights.bicep' = {
  scope: resourceGroup
  name: 'appInsights'
  params: {
    name: 'beercomp-insights-${environment}'
    location: location
  }
}

// Outputs
output acrLoginServer string = containerRegistry.outputs.loginServer
output postgresHost string = postgresServer.outputs.host
output staticWebAppUrl string = staticWebApp.outputs.defaultHostname
output appInsightsConnectionString string = appInsights.outputs.connectionString
```

**Module Example**: `infrastructure/bicep/modules/postgres-server.bicep`

```bicep
@description('PostgreSQL server name')
param serverName string

@description('Azure region')
param location string

@description('Administrator username')
param administratorLogin string

@description('Administrator password')
@secure()
param administratorPassword string

@description('SKU name (e.g., Standard_B2s)')
param skuName string = 'Standard_B2s'

@description('Storage size in GB')
param storageSizeGB int = 128

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = {
  name: serverName
  location: location
  sku: {
    name: skuName
    tier: 'GeneralPurpose'
  }
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorPassword
    version: '16'
    storage: {
      storageSizeGB: storageSizeGB
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
}

// Create default database
resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-03-01-preview' = {
  parent: postgresServer
  name: 'beercomp'
}

// Firewall rule to allow Azure services
resource firewallRule 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-03-01-preview' = {
  parent: postgresServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

output host string = postgresServer.properties.fullyQualifiedDomainName
output connectionString string = 'Host=${postgresServer.properties.fullyQualifiedDomainName};Port=5432;Database=beercomp;Username=${administratorLogin};Password=${administratorPassword};SslMode=Require'
```

**Deployment Commands**:
```bash
# Deploy infrastructure
az deployment sub create \
  --location eastus \
  --template-file infrastructure/bicep/main.bicep \
  --parameters environment=prod postgresAdminPassword='<secure-password>'

# Verify deployment
az deployment sub show --name main
```

---

## 4. Terraform (Future Migration)

**Purpose**: Multi-cloud flexibility; richer state management and module ecosystem.

**Planned Structure**:
```
infrastructure/
  terraform/
    main.tf
    variables.tf
    outputs.tf
    modules/
      postgres/
      container-registry/
      aks/  # Future Kubernetes migration
      networking/
```

**Note**: Terraform will replace Bicep once platform matures beyond single-cloud Azure deployment.

---

## 5. OpenTelemetry Collector Configuration

**File**: `infrastructure/otel-collector-config.yaml`

```yaml
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317
      http:
        endpoint: 0.0.0.0:4318

processors:
  batch:
    timeout: 10s
    send_batch_size: 1024

exporters:
  azuremonitor:
    connection_string: "${AZURE_MONITOR_CONNECTION_STRING}"

  logging:
    loglevel: debug

service:
  pipelines:
    traces:
      receivers: [otlp]
      processors: [batch]
      exporters: [azuremonitor, logging]
    metrics:
      receivers: [otlp]
      processors: [batch]
      exporters: [azuremonitor, logging]
```

---

## 6. Database Migration Scripts

**Initial Schema** (Example): `scripts/init-db.sql`

```sql
-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create Tenants table
CREATE TABLE Tenants (
    tenant_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    organization_name VARCHAR(255) NOT NULL,
    contact_email VARCHAR(255) NOT NULL,
    subscription_tier VARCHAR(50) DEFAULT 'FREE',
    status VARCHAR(50) DEFAULT 'ACTIVE',
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Create Users table
CREATE TABLE Users (
    user_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    tenant_id UUID NOT NULL REFERENCES Tenants(tenant_id),
    keycloak_subject VARCHAR(255) UNIQUE NOT NULL,
    email VARCHAR(255) NOT NULL,
    full_name VARCHAR(255),
    roles JSONB,
    bjcp_rank VARCHAR(50),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    UNIQUE (tenant_id, email)
);

-- Additional tables... (abbreviated for brevity)
```

**Migrations via EF Core**:
```bash
# Generate migration
dotnet ef migrations add InitialSchema --project services/bjcp-entries

# Apply migration
dotnet ef database update --project services/bjcp-entries
```

---

## 7. Monitoring & Alerting

**Azure Monitor Alerts** (configured via Bicep or Portal):
- **High Error Rate**: Alert if HTTP 5xx responses > 5% of total requests.
- **Database Connection Pool Exhaustion**: Alert if connection count > 90% of max pool size.
- **RabbitMQ Queue Depth**: Alert if any queue depth > 1000 messages.
- **High Latency**: Alert if p95 request latency > 2 seconds.

**Dashboards**:
- Real-time request rate, error rate, latency (p50, p95, p99).
- Service health status (up/down).
- Database query performance (slow queries > 1s).
- Event bus throughput and lag.

---

**Document Version**: 1.0  
**Last Updated**: 2025-12-18
