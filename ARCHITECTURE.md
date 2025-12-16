# Beer Competition SaaS - Technical Architecture

## Table of Contents
1. [System Overview](#system-overview)
2. [Technology Stack](#technology-stack)
3. [System Design](#system-design)
4. [Database Schema](#database-schema)
5. [API Structure](#api-structure)
6. [Authentication Flow](#authentication-flow)
7. [Deployment Architecture](#deployment-architecture)
8. [Security Considerations](#security-considerations)

---

## System Overview

### Purpose
The Beer Competition SaaS platform is a comprehensive system designed to manage, organize, and execute beer competitions at scale. It provides tools for competition organizers, judges, participants, and administrators to collaborate efficiently.

### Key Features
- Competition management and scheduling
- Participant and beer entry registration
- Judge assignment and scoring
- Real-time leaderboards and results
- Detailed analytics and reporting
- Multi-tenant support with isolated data per organization
- Role-based access control (RBAC)

### High-Level Architecture
```
┌─────────────────────────────────────────────────────────┐
│                    Client Layer                          │
│  (Web App, Mobile, Admin Dashboard)                      │
└────────────────────┬────────────────────────────────────┘
                     │ HTTPS/REST/GraphQL
┌────────────────────▼────────────────────────────────────┐
│              API Gateway / Load Balancer                 │
│              (Rate Limiting, Authentication)             │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│              Microservices Layer                          │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐    │
│  │ Competition  │ │   Scoring    │ │  Reporting   │    │
│  │   Service    │ │   Service    │ │   Service    │    │
│  └──────────────┘ └──────────────┘ └──────────────┘    │
│  ┌──────────────┐ ┌──────────────┐ ┌──────────────┐    │
│  │   User &     │ │  Notification│ │   Analytics  │    │
│  │    Auth      │ │   Service    │ │   Service    │    │
│  └──────────────┘ └──────────────┘ └──────────────┘    │
└────────────────────┬────────────────────────────────────┘
                     │
        ┌────────────┼────────────┐
        │            │            │
┌───────▼──┐  ┌──────▼──┐  ┌─────▼────┐
│ Primary  │  │  Cache  │  │  Message  │
│ Database │  │ (Redis) │  │  Queue    │
│          │  │         │  │(RabbitMQ) │
└──────────┘  └─────────┘  └───────────┘
```

---

## Technology Stack

### Backend
- **Runtime**: Node.js 18+ or Python 3.10+
- **Framework**: Express.js or FastAPI
- **Language**: JavaScript/TypeScript or Python
- **Package Manager**: npm/yarn or pip

### Frontend
- **Framework**: React 18+ with TypeScript
- **State Management**: Redux Toolkit or Zustand
- **UI Library**: Material-UI or Tailwind CSS
- **Build Tool**: Vite or Webpack
- **Testing**: Jest, React Testing Library

### Database
- **Primary**: PostgreSQL 14+ (relational data)
- **Cache**: Redis 6+ (session management, caching)
- **Search**: Elasticsearch 8+ (optional, for advanced search)

### Infrastructure & DevOps
- **Containerization**: Docker
- **Orchestration**: Kubernetes or Docker Compose
- **CI/CD**: GitHub Actions, GitLab CI, or Jenkins
- **Monitoring**: Prometheus, Grafana, ELK Stack
- **Logging**: Winston, Bunyan, or ELK Stack
- **Message Queue**: RabbitMQ or Redis Streams

### External Services
- **Email**: SendGrid, AWS SES, or Mailgun
- **SMS**: Twilio (for notifications)
- **Storage**: AWS S3 or Google Cloud Storage (for certificates, badges)
- **Authentication**: OAuth2 providers (Google, GitHub)

---

## System Design

### Architecture Patterns

#### 1. Microservices Architecture
The system is divided into independent, deployable services:

```
┌─────────────────────────────────────────────────────┐
│ API Gateway (Express/Kong)                          │
│ - Route management                                   │
│ - Authentication middleware                          │
│ - Rate limiting                                      │
│ - Request logging                                    │
└─────────────────────────────────────────────────────┘
         │         │         │         │
    ┌────▼─┐  ┌────▼─┐  ┌────▼─┐  ┌──▼────┐
    │Auth  │  │User  │  │Comp. │  │Score  │
    │Svc   │  │Svc   │  │Svc   │  │Svc    │
    └──────┘  └──────┘  └──────┘  └───────┘
```

#### 2. Service Responsibilities

| Service | Responsibilities |
|---------|------------------|
| **Authentication Service** | OAuth/JWT management, token validation, password reset |
| **User Service** | Profile management, organization membership, RBAC |
| **Competition Service** | Competition CRUD, scheduling, category management |
| **Entry Service** | Beer entry registration, validation, category assignment |
| **Scoring Service** | Judge assignments, score recording, result calculation |
| **Reporting Service** | Report generation, analytics, leaderboard computation |
| **Notification Service** | Email/SMS delivery, notification preferences |
| **Analytics Service** | Event tracking, metrics collection, dashboards |

#### 3. Communication Patterns

**Synchronous (REST/GraphQL)**
```
POST /api/v1/competitions
├── Headers: Authorization: Bearer {jwt}
├── Body: { name, description, date, location }
└── Response: { id, status, createdAt }
```

**Asynchronous (Message Queue)**
```
Event: competition.created
├── Publish to event stream
├── Subscribers: Notification Service, Analytics Service
└── Actions: Send notifications, record metrics
```

#### 4. Tenant Isolation
```javascript
// Every request includes tenant context
{
  tenantId: "org_123",
  userId: "user_456",
  roles: ["admin", "organizer"]
}

// Database queries automatically filtered
WHERE organization_id = :tenantId
```

---

## Database Schema

### Core Tables

#### 1. Organizations (Multi-Tenancy)
```sql
CREATE TABLE organizations (
  id UUID PRIMARY KEY,
  name VARCHAR(255) NOT NULL,
  slug VARCHAR(255) UNIQUE NOT NULL,
  description TEXT,
  website_url VARCHAR(255),
  logo_url VARCHAR(255),
  subscription_plan VARCHAR(50), -- free, pro, enterprise
  max_competitions INTEGER,
  max_judges INTEGER,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  is_active BOOLEAN DEFAULT true
);
```

#### 2. Users
```sql
CREATE TABLE users (
  id UUID PRIMARY KEY,
  email VARCHAR(255) UNIQUE NOT NULL,
  password_hash VARCHAR(255),
  first_name VARCHAR(100),
  last_name VARCHAR(100),
  avatar_url VARCHAR(255),
  phone VARCHAR(20),
  country VARCHAR(2),
  external_provider VARCHAR(50), -- google, github
  external_id VARCHAR(255),
  email_verified BOOLEAN DEFAULT false,
  is_active BOOLEAN DEFAULT true,
  last_login TIMESTAMP,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_external_provider ON users(external_provider, external_id);
```

#### 3. Organization Memberships
```sql
CREATE TABLE organization_members (
  id UUID PRIMARY KEY,
  organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  role VARCHAR(50) NOT NULL, -- owner, admin, organizer, judge, participant
  joined_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  invited_by UUID REFERENCES users(id),
  is_active BOOLEAN DEFAULT true,
  UNIQUE(organization_id, user_id)
);

CREATE INDEX idx_org_members_org_id ON organization_members(organization_id);
CREATE INDEX idx_org_members_user_id ON organization_members(user_id);
```

#### 4. Competitions
```sql
CREATE TABLE competitions (
  id UUID PRIMARY KEY,
  organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
  name VARCHAR(255) NOT NULL,
  description TEXT,
  status VARCHAR(50), -- draft, scheduled, active, completed, cancelled
  start_date TIMESTAMP NOT NULL,
  end_date TIMESTAMP NOT NULL,
  location VARCHAR(255),
  max_entries INTEGER,
  entry_fee DECIMAL(10, 2),
  created_by UUID NOT NULL REFERENCES users(id),
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_competitions_org_id ON competitions(organization_id);
CREATE INDEX idx_competitions_status ON competitions(status);
```

#### 5. Beer Categories
```sql
CREATE TABLE beer_categories (
  id UUID PRIMARY KEY,
  competition_id UUID NOT NULL REFERENCES competitions(id) ON DELETE CASCADE,
  name VARCHAR(255) NOT NULL,
  description TEXT,
  estimated_og DECIMAL(5, 3),
  estimated_ibu DECIMAL(6, 1),
  estimated_abv DECIMAL(4, 2),
  max_entries INTEGER,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  UNIQUE(competition_id, name)
);

CREATE INDEX idx_categories_comp_id ON beer_categories(competition_id);
```

#### 6. Beer Entries
```sql
CREATE TABLE beer_entries (
  id UUID PRIMARY KEY,
  competition_id UUID NOT NULL REFERENCES competitions(id) ON DELETE CASCADE,
  category_id UUID NOT NULL REFERENCES beer_categories(id) ON DELETE RESTRICT,
  brewer_id UUID NOT NULL REFERENCES users(id),
  brewer_name VARCHAR(255),
  beer_name VARCHAR(255) NOT NULL,
  description TEXT,
  og DECIMAL(5, 3),
  ibu DECIMAL(6, 1),
  abv DECIMAL(4, 2),
  entry_number VARCHAR(50),
  status VARCHAR(50), -- pending, registered, checked_in, disqualified
  registered_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_entries_comp_id ON beer_entries(competition_id);
CREATE INDEX idx_entries_brewer_id ON beer_entries(brewer_id);
CREATE INDEX idx_entries_status ON beer_entries(status);
```

#### 7. Judges
```sql
CREATE TABLE judges (
  id UUID PRIMARY KEY,
  competition_id UUID NOT NULL REFERENCES competitions(id) ON DELETE CASCADE,
  user_id UUID NOT NULL REFERENCES users(id),
  certification_level VARCHAR(50), -- apprentice, certified, master
  judging_experience_years INTEGER,
  assigned_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  is_active BOOLEAN DEFAULT true,
  UNIQUE(competition_id, user_id)
);

CREATE INDEX idx_judges_comp_id ON judges(competition_id);
```

#### 8. Judge Assignments (Flight/Round Assignments)
```sql
CREATE TABLE judge_assignments (
  id UUID PRIMARY KEY,
  competition_id UUID NOT NULL REFERENCES competitions(id) ON DELETE CASCADE,
  judge_id UUID NOT NULL REFERENCES judges(id) ON DELETE CASCADE,
  flight_number INTEGER,
  round_number INTEGER,
  assigned_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_judge_assignments_comp_id ON judge_assignments(competition_id);
CREATE INDEX idx_judge_assignments_judge_id ON judge_assignments(judge_id);
```

#### 9. Scores
```sql
CREATE TABLE scores (
  id UUID PRIMARY KEY,
  entry_id UUID NOT NULL REFERENCES beer_entries(id) ON DELETE CASCADE,
  judge_id UUID NOT NULL REFERENCES judges(id) ON DELETE CASCADE,
  aroma_score INTEGER CHECK (aroma_score BETWEEN 0 AND 50),
  appearance_score INTEGER CHECK (appearance_score BETWEEN 0 AND 3),
  flavor_score INTEGER CHECK (flavor_score BETWEEN 0 AND 50),
  mouthfeel_score INTEGER CHECK (mouthfeel_score BETWEEN 0 AND 10),
  overall_impression VARCHAR(1000),
  total_score DECIMAL(5, 2) GENERATED ALWAYS AS (aroma_score + appearance_score + flavor_score + mouthfeel_score) STORED,
  judged_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_scores_entry_id ON scores(entry_id);
CREATE INDEX idx_scores_judge_id ON scores(judge_id);
CREATE INDEX idx_scores_total ON scores(total_score DESC);
```

#### 10. Results/Rankings
```sql
CREATE TABLE results (
  id UUID PRIMARY KEY,
  competition_id UUID NOT NULL REFERENCES competitions(id) ON DELETE CASCADE,
  category_id UUID NOT NULL REFERENCES beer_categories(id) ON DELETE CASCADE,
  entry_id UUID NOT NULL REFERENCES beer_entries(id) ON DELETE CASCADE,
  rank INTEGER,
  average_score DECIMAL(5, 2),
  medal VARCHAR(50), -- gold, silver, bronze, none
  score_count INTEGER,
  calculated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_results_comp_id ON results(competition_id);
CREATE INDEX idx_results_category_id ON results(category_id);
CREATE INDEX idx_results_rank ON results(rank);
```

#### 11. Audit Logs
```sql
CREATE TABLE audit_logs (
  id UUID PRIMARY KEY,
  organization_id UUID NOT NULL REFERENCES organizations(id),
  user_id UUID REFERENCES users(id),
  entity_type VARCHAR(100),
  entity_id UUID,
  action VARCHAR(50), -- create, update, delete, view
  changes JSONB,
  ip_address INET,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_audit_logs_org_id ON audit_logs(organization_id);
CREATE INDEX idx_audit_logs_user_id ON audit_logs(user_id);
```

### Entity Relationship Diagram
```
organizations
    ├── organization_members (users)
    ├── competitions
    │   ├── beer_categories
    │   │   └── beer_entries
    │   │       └── scores
    │   │           └── judges
    │   ├── judges
    │   └── results
    └── audit_logs

users
    ├── organization_members
    ├── beer_entries (as brewer)
    └── scores (historical)
```

---

## API Structure

### Base URL
```
https://api.beer-competition.com/api/v1
```

### Authentication Header
```
Authorization: Bearer {jwt_token}
Content-Type: application/json
X-Tenant-ID: {organization_id}
```

### API Endpoints Overview

#### Authentication Endpoints
```
POST   /auth/register              - Register new user
POST   /auth/login                 - User login
POST   /auth/refresh               - Refresh JWT token
POST   /auth/logout                - Logout user
POST   /auth/password-reset        - Request password reset
POST   /auth/password-reset/verify - Verify reset token
POST   /auth/oauth/callback        - OAuth callback handler
```

#### User Management
```
GET    /users/me                   - Get current user profile
PUT    /users/me                   - Update profile
POST   /users/me/avatar            - Upload avatar
GET    /users/{userId}             - Get user by ID
PUT    /users/{userId}             - Update user (admin)
DELETE /users/{userId}             - Delete user (admin)
```

#### Organization Management
```
GET    /organizations              - List user's organizations
POST   /organizations              - Create new organization
GET    /organizations/{orgId}      - Get organization details
PUT    /organizations/{orgId}      - Update organization
DELETE /organizations/{orgId}      - Delete organization
GET    /organizations/{orgId}/members - List members
POST   /organizations/{orgId}/members - Invite member
PUT    /organizations/{orgId}/members/{memberId} - Update member role
DELETE /organizations/{orgId}/members/{memberId} - Remove member
```

#### Competitions
```
GET    /competitions               - List competitions
POST   /competitions               - Create competition
GET    /competitions/{compId}      - Get competition details
PUT    /competitions/{compId}      - Update competition
DELETE /competitions/{compId}      - Delete competition
POST   /competitions/{compId}/start - Start competition
POST   /competitions/{compId}/finish - Finish competition
```

#### Beer Categories
```
GET    /competitions/{compId}/categories           - List categories
POST   /competitions/{compId}/categories           - Create category
PUT    /competitions/{compId}/categories/{catId}   - Update category
DELETE /competitions/{compId}/categories/{catId}   - Delete category
```

#### Beer Entries
```
GET    /competitions/{compId}/entries              - List entries
POST   /competitions/{compId}/entries              - Register entry
GET    /competitions/{compId}/entries/{entryId}    - Get entry details
PUT    /competitions/{compId}/entries/{entryId}    - Update entry
DELETE /competitions/{compId}/entries/{entryId}    - Delete entry
POST   /competitions/{compId}/entries/{entryId}/checkin - Check-in entry
```

#### Judging
```
GET    /competitions/{compId}/judges               - List judges
POST   /competitions/{compId}/judges               - Assign judge
PUT    /competitions/{compId}/judges/{judgeId}     - Update judge info
DELETE /competitions/{compId}/judges/{judgeId}     - Remove judge
GET    /competitions/{compId}/judge-assignments    - Get assignments
POST   /competitions/{compId}/judge-assignments    - Create assignment
```

#### Scoring
```
GET    /competitions/{compId}/scores               - List scores
POST   /entries/{entryId}/scores                   - Submit score
PUT    /scores/{scoreId}                           - Update score
GET    /competitions/{compId}/results              - Get results
GET    /competitions/{compId}/leaderboard          - Get leaderboard
```

### Request/Response Examples

#### Create Competition
```json
POST /competitions
Authorization: Bearer {jwt}
X-Tenant-ID: org_123

Request:
{
  "name": "2025 Regional Beer Competition",
  "description": "Annual regional competition",
  "start_date": "2025-06-15T09:00:00Z",
  "end_date": "2025-06-15T17:00:00Z",
  "location": "Denver, Colorado",
  "max_entries": 200,
  "entry_fee": 25.00
}

Response (201 Created):
{
  "id": "comp_123",
  "organization_id": "org_123",
  "name": "2025 Regional Beer Competition",
  "status": "draft",
  "start_date": "2025-06-15T09:00:00Z",
  "end_date": "2025-06-15T17:00:00Z",
  "location": "Denver, Colorado",
  "max_entries": 200,
  "entry_fee": 25.00,
  "created_by": "user_456",
  "created_at": "2025-12-16T18:53:09Z",
  "updated_at": "2025-12-16T18:53:09Z"
}
```

#### Submit Score
```json
POST /entries/entry_789/scores
Authorization: Bearer {jwt}

Request:
{
  "judge_id": "judge_555",
  "aroma_score": 45,
  "appearance_score": 3,
  "flavor_score": 48,
  "mouthfeel_score": 9,
  "overall_impression": "Excellent balance of flavors, well-crafted IPA"
}

Response (201 Created):
{
  "id": "score_111",
  "entry_id": "entry_789",
  "judge_id": "judge_555",
  "aroma_score": 45,
  "appearance_score": 3,
  "flavor_score": 48,
  "mouthfeel_score": 9,
  "total_score": 105,
  "overall_impression": "Excellent balance of flavors, well-crafted IPA",
  "judged_at": "2025-12-16T18:53:09Z",
  "created_at": "2025-12-16T18:53:09Z"
}
```

#### Error Response
```json
{
  "status": 400,
  "code": "VALIDATION_ERROR",
  "message": "Validation failed",
  "errors": [
    {
      "field": "aroma_score",
      "message": "Score must be between 0 and 50"
    }
  ],
  "timestamp": "2025-12-16T18:53:09Z"
}
```

### Pagination
```json
GET /competitions?page=2&limit=10&sort=-created_at

Response:
{
  "data": [...],
  "pagination": {
    "page": 2,
    "limit": 10,
    "total": 45,
    "total_pages": 5,
    "has_next": true,
    "has_prev": true
  }
}
```

---

## Authentication Flow

### Authentication Architecture

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │
       ├─────────────────────────────────────┐
       │                                     │
    ┌──▼───────────────────┐     ┌──────────▼──┐
    │ Local Auth           │     │ OAuth2 Flow │
    │ (Email/Password)     │     │ (Google/GH) │
    └──┬─────────────────┬─┘     └──────────┬──┘
       │                 │                  │
    ┌──▼──────────┐  ┌───▼──────────┐  ┌───▼──────────┐
    │ Credentials │  │ Credentials  │  │ Auth Code    │
    │ Exchange    │  │ Verification │  │ Exchange     │
    └──┬──────────┘  └───┬──────────┘  └───┬──────────┘
       │                 │                  │
       └─────────────────┼──────────────────┘
                         │
                    ┌────▼─────────┐
                    │ Auth Service │
                    └────┬─────────┘
                         │
       ┌─────────────────┼─────────────────┐
       │                 │                 │
    ┌──▼──────────┐  ┌───▼──────────┐ ┌───▼──────────┐
    │ Validate    │  │ Generate     │ │ Store in     │
    │ Identity    │  │ JWT Tokens   │ │ Cache/DB     │
    └──┬──────────┘  └───┬──────────┘ └───┬──────────┘
       │                 │                 │
       └─────────────────┼─────────────────┘
                         │
                    ┌────▼─────────┐
                    │ Return Tokens │
                    │ (JWT + Refresh)│
                    └────┬─────────┘
                         │
                    ┌────▼──────────────┐
                    │ Client Stores     │
                    │ Tokens (Memory/   │
                    │ Secure Storage)   │
                    └────┬──────────────┘
                         │
                    ┌────▼──────────────┐
                    │ Include JWT in    │
                    │ Authorization     │
                    │ Header for API    │
                    │ Requests          │
                    └───────────────────┘
```

### 1. Local Authentication (Email/Password)

#### Registration
```javascript
// POST /auth/register
{
  "email": "brewer@example.com",
  "password": "SecurePassword123!",
  "first_name": "John",
  "last_name": "Brewster"
}

// Response
{
  "user": {
    "id": "user_123",
    "email": "brewer@example.com",
    "first_name": "John"
  },
  "access_token": "eyJhbGciOiJIUzI1NiIs...",
  "refresh_token": "eyJhbGciOiJIUzI1NiIs...",
  "expires_in": 3600
}
```

**Process:**
1. Client sends email, password, name
2. Server validates email format and password strength
3. Server checks if email exists (unique constraint)
4. Server hashes password with bcrypt (12 rounds)
5. Server creates user record
6. Server generates JWT and refresh token
7. Server returns tokens to client

#### Login
```javascript
// POST /auth/login
{
  "email": "brewer@example.com",
  "password": "SecurePassword123!"
}

// Response
{
  "access_token": "eyJhbGciOiJIUzI1NiIs...",
  "refresh_token": "eyJhbGciOiJIUzI1NiIs...",
  "expires_in": 3600,
  "user": { ... }
}
```

**Process:**
1. Client sends email and password
2. Server queries user by email
3. Server compares password with bcrypt hash
4. If invalid: return 401 Unauthorized
5. If valid: generate JWT and refresh token
6. Update user's last_login timestamp
7. Return tokens to client

### 2. JWT Token Structure

```javascript
// Access Token (expires in 1 hour)
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "user_123",           // Subject (user ID)
    "email": "brewer@example.com",
    "tenant_id": "org_123",      // Multi-tenant context
    "roles": ["participant"],    // User roles
    "permissions": ["read:entries", "create:entries"],
    "iat": 1671283989,           // Issued at
    "exp": 1671287589,           // Expiration (1 hour)
    "jti": "jwt_id_123"          // JWT ID (for revocation)
  },
  "signature": "HMAC256(...)"
}

// Refresh Token (expires in 30 days)
{
  "payload": {
    "sub": "user_123",
    "type": "refresh",
    "iat": 1671283989,
    "exp": 1673875989,           // 30 days
    "jti": "rtk_id_456"
  },
  "signature": "HMAC256(...)"
}
```

### 3. Token Refresh Flow

```javascript
// POST /auth/refresh
{
  "refresh_token": "eyJhbGciOiJIUzI1NiIs..."
}

// Response
{
  "access_token": "eyJhbGciOiJIUzI1NiIs...",
  "refresh_token": "eyJhbGciOiJIUzI1NiIs...",
  "expires_in": 3600
}
```

**Process:**
1. Client sends refresh token
2. Server validates refresh token signature
3. Server checks if token is expired
4. Server checks if token is revoked (Redis/DB)
5. Server checks if associated user still exists
6. Generate new access token
7. Optionally rotate refresh token
8. Return new tokens

### 4. OAuth2 Flow (Google/GitHub)

```
┌─────────────┐                              ┌──────────────┐
│   Client    │                              │ Google/GitHub│
└──────┬──────┘                              └──────────────┘
       │                                             │
       │ 1. Redirect to OAuth provider               │
       ├─────────────────────────────────────────────>
       │                                             │
       │    2. User authenticates & grants consent   │
       │ <─────────────────────────────────────────────
       │                                             │
       │ 3. Redirect to callback with auth code      │
       │ <─────────────────────────────────────────────
       │                                             │
    ┌──▼──────────────────┐                         │
    │ Auth Service        │                         │
    │ /auth/oauth/callback│                         │
    └──┬─────────────────┬┘                         │
       │ 4. Exchange auth code for tokens           │
       ├─────────────────────────────────────────────>
       │                                             │
       │ 5. Return access token                      │
       │ <─────────────────────────────────────────────
       │                                             │
       │ 6. Get user profile                         │
       ├─────────────────────────────────────────────>
       │                                             │
       │ 7. Return user info                         │
       │ <─────────────────────────────────────────────
```

**Process:**
1. Client redirects to `https://accounts.google.com/o/oauth2/v2/auth?client_id=...&redirect_uri=...&scope=...`
2. User authenticates with Google/GitHub
3. Provider redirects to `https://api.beer-competition.com/auth/oauth/callback?code=...`
4. Server exchanges code for provider tokens
5. Server retrieves user profile from provider
6. Server checks if user exists by external_id
   - If exists: update profile, generate JWT
   - If new: create user record, generate JWT
7. Server returns tokens and redirects to client

### 5. Authorization (RBAC)

**Role Hierarchy:**
```
Owner (most permissions)
  ├── Admin
  │   ├── Organizer
  │   └── Judge
  └── Participant (fewest permissions)
```

**Permission Matrix:**
```
┌──────────────────┬───────┬──────┬───────────┬──────┬─────────────┐
│ Resource         │ Owner │Admin │ Organizer │Judge │ Participant │
├──────────────────┼───────┼──────┼───────────┼──────┼─────────────┤
│ Organization     │ CRUD  │ CRU  │ R         │ R    │ -           │
│ Competitions     │ CRUD  │ CRUD │ CRUD      │ R    │ R           │
│ Beer Entries     │ CRUD  │ CRUD │ CRUD      │ R    │ CR          │
│ Judge Assign     │ CRUD  │ CRUD │ CRUD      │ -    │ -           │
│ Scores           │ CRU   │ CRU  │ R         │ CRU  │ -           │
│ Results          │ R     │ R    │ R         │ R    │ R           │
│ Members          │ CRUD  │ CRUD │ R         │ -    │ -           │
└──────────────────┴───────┴──────┴───────────┴──────┴─────────────┘

C = Create, R = Read, U = Update, D = Delete, - = No access
```

### 6. Session Management

**Session Storage:**
```
Redis Key: session:{sessionId}
Value: {
  userId: "user_123",
  tenantId: "org_123",
  tokenFamily: "rtk_family_789",  // For refresh token rotation
  createdAt: 1671283989,
  lastActivity: 1671283989,
  ipAddress: "192.168.1.1",
  userAgent: "Mozilla/5.0..."
}

TTL: 30 days (refresh token lifetime)
```

**Token Revocation:**
```
Redis Key: token_revocation:{jti}
Value: true
TTL: Token expiration time
```

### 7. Security Measures

**Password Security:**
- Minimum 12 characters
- Must include uppercase, lowercase, number, special character
- Bcrypt hashing with 12 rounds
- Password expiration every 90 days (optional)
- Prevent reuse of last 5 passwords

**Token Security:**
- HTTPS only transmission
- Secure HttpOnly cookies for tokens
- Token rotation on refresh
- JTI (JWT ID) for revocation tracking
- Short-lived access tokens (1 hour)
- Long-lived refresh tokens (30 days)

**Request Validation:**
- CORS whitelist for allowed origins
- CSRF token for state-changing operations
- Rate limiting per user/IP
- Request signing for sensitive operations

**Session Security:**
- Session fixation prevention
- Device fingerprinting for anomaly detection
- IP address validation
- User-Agent validation
- Concurrent session limits

---

## Deployment Architecture

### Development Environment
```
Docker Compose Stack:
├── Frontend (React)
├── API Gateway (Express)
├── Auth Service
├── Competition Service
├── Scoring Service
├── PostgreSQL
├── Redis
├── RabbitMQ
└── Mailhog (Email Testing)
```

### Production Environment
```
Kubernetes Cluster:
├── Ingress Controller (Nginx)
├── API Gateway (Pod replicas)
├── Microservices (Pod replicas)
├── PostgreSQL (StatefulSet)
├── Redis (StatefulSet)
├── RabbitMQ (StatefulSet)
├── Elasticsearch (StatefulSet)
└── Persistent Volumes
```

### CI/CD Pipeline
```
Code Push → GitHub Actions → Tests → Build → Push to Registry → Deploy to K8s
```

---

## Security Considerations

### Data Protection
- Encryption at rest (AES-256 for sensitive data)
- Encryption in transit (TLS 1.3)
- Field-level encryption for PII
- Regular backups with encryption
- GDPR compliance (data retention, right to be forgotten)

### Access Control
- Multi-factor authentication (MFA)
- IP whitelisting for admin operations
- API key management with rotation
- Audit logging for all sensitive operations
- Regular access reviews

### Infrastructure Security
- Network segmentation (DMZ, internal network)
- Web Application Firewall (WAF)
- DDoS protection
- Regular security scanning
- Penetration testing

### Compliance
- SOC 2 Type II compliance
- GDPR and CCPA compliance
- PCI DSS compliance (for payment processing)
- Data residency requirements

---

## Monitoring & Observability

### Metrics
- Request latency (p50, p95, p99)
- Error rates by service
- Database query performance
- Cache hit rates
- Queue processing times

### Logging
- Structured logging (JSON format)
- Centralized logging (ELK Stack)
- Log retention: 30 days (hot), 1 year (cold)
- PII redaction in logs

### Alerting
- Critical errors
- High error rates
- Performance degradation
- Resource exhaustion
- Security incidents

---

## Disaster Recovery

### Backup Strategy
- Daily automated backups
- Point-in-time recovery capability
- Backup replication to secondary region
- Regular backup restoration tests

### High Availability
- Multi-region deployment
- Database replication
- Cache failover
- Load balancing with health checks
- Auto-scaling policies

### Recovery Time Objectives (RTO) & Recovery Point Objectives (RPO)
- RTO: 1 hour
- RPO: 15 minutes

---

## Performance Optimization

### Caching Strategy
- Database query results (Redis)
- API response caching
- Browser caching for static assets
- CDN for global content delivery

### Database Optimization
- Query optimization and indexing
- Connection pooling
- Read replicas for scaling reads
- Partitioning for large tables

### Frontend Optimization
- Code splitting and lazy loading
- Image optimization
- Gzip compression
- Service Worker caching

---

## Future Considerations

### Planned Enhancements
- Real-time scoring updates via WebSockets
- Mobile applications (iOS/Android)
- Advanced analytics and ML-based predictions
- Integration with brewery management systems
- Video judging capabilities
- Regional federation support

### Scalability Roadmap
- Horizontal pod autoscaling
- Database sharding strategy
- Microservice decomposition
- Event sourcing for audit trails
- CQRS pattern implementation

---

## Contributing

For questions or suggestions regarding this architecture, please open an issue or contact the development team.

**Last Updated:** 2025-12-16
**Version:** 1.0
