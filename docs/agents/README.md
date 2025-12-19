# AI Agents for GitHub Copilot

This directory contains specialized AI agents that work with GitHub Copilot in VS Code. Each agent is designed to be **generic and reusable across any project**, not tied to specific applications.

## 🤖 Available Agents

| Agent | Description | Tools Used |
|-------|-------------|------------|
| **backend.agent.md** | Backend development (.NET, Java, Python, Node.js) | filesystem, github, postgres, memory, fetch |
| **frontend.agent.md** | Frontend development (React, Vue, Angular) | filesystem, github, memory, fetch |
| **devops.agent.md** | Infrastructure & CI/CD (Azure, AWS, Docker, K8s) | filesystem, github, fetch, memory, azure-resources, azure-auth |
| **qa.agent.md** | Testing & quality assurance (unit, integration, E2E) | filesystem, github, postgres, memory, fetch |
| **data-science.agent.md** | ML & analytics (Python, FastAPI, scikit-learn) | filesystem, github, postgres, memory, fetch |
| **teacher.agent.md** | Pattern documentation & knowledge sharing | filesystem, github, memory, fetch |
| **product-owner.agent.md** | Product management, backlog, user stories, GitHub issues | filesystem, github, memory, fetch |

## 🎯 Agent Selection Decision Tree

```
┌─────────────────────────────────────────────────────────────────┐
│                    What task do you need to do?                 │
└─────────────────────────────────────────────────────────────────┘
                                  │
        ┌─────────────────────────┴─────────────────────────┐
        │                                                     │
        ▼                                                     ▼
Product/Feature Definition?                           Implementation?
        │                                                     │
        ▼                                                     ▼
Product Owner Agent                                    ┌───┴───┐
(User stories, backlog,                                │       │
 GitHub issues, PRs)                                Backend Frontend
                                                          │       │
                                                          ▼       ▼
                                                       .NET   React/PWA
                                                          │       │
                                                          ▼       ▼
                                                      Backend Frontend
                                                       Agent   Agent
    
        ┌─────────────────────────────────────────────────┐
        │                                                 │
        ▼                                                 ▼
Infrastructure/Deployment?                         Testing/Quality?
        │                                                 │
        ▼                                                 ▼
DevOps Agent                                        QA Agent
(Docker, Azure,                                     (Unit, Integration,
 CI/CD, IaC)                                        E2E, Security, Load)

        │                                                 │
        ▼                                                 ▼
Data/Analytics?                                    Review/Documentation?
        │                                                 │
        ▼                                                 ▼
Data Science Agent                                  Teacher Agent
(Python, ML, FastAPI,                               (Pattern docs, ADRs,
 Jupyter, Analytics)                                Knowledge sharing)
```

## 📋 How to Use Agents

### 1. Select an Agent in Copilot Chat

In VS Code, open GitHub Copilot Chat and type:

```
@backend help me create a REST API for user management
```

or

```
@frontend build a responsive navigation component
```

The agent will respond with implementation guidance specific to its expertise.

### 2. Agent Selection Guide

**Use @backend when:**
- Building REST APIs or GraphQL services
- Designing database schemas
- Implementing authentication
- Creating microservices
- Writing backend tests

**Use @frontend when:**
- Building UI components
- Creating responsive layouts
- Implementing state management
- Building Progressive Web Apps (PWAs)
- Writing frontend tests

**Use @devops when:**
- Setting up cloud infrastructure
- Creating Docker containers
- Writing CI/CD pipelines
- Configuring monitoring
- Managing secrets

**Use @qa when:**
- Designing test strategies
- Writing unit/integration/E2E tests
- Performing security testing
- Load testing
- Accessibility testing

**Use @data-science when:**
- Building ML models
- Creating analytics dashboards
- Implementing recommendation systems
- NLP tasks
- Time series forecasting

**Use @teacher when:**
- Reviewing pull requests
- Documenting architectural decisions
- Identifying design patterns
- Proposing knowledge-sharing content
- Creating ADRs

**Use @product-owner when:**
- Defining new features or capabilities
- Writing user stories with acceptance criteria
- Creating and prioritizing product backlog
- Breaking down epics into implementable tasks
- Creating GitHub issues for development teams
- Managing sprints and roadmap
- Reviewing and accepting completed features

## 🛠 MCP Tools Configuration

Each agent uses specific MCP (Model Context Protocol) tools. Ensure these are configured in your `mcp.json`:

### Configuration Location
`%APPDATA%\Code\User\globalStorage\github.copilot-mcp-config\mcp.json`

Full path: `C:\Users\[YourUsername]\AppData\Roaming\Code\User\globalStorage\github.copilot-mcp-config\mcp.json`

### Complete Configuration

```json
{
  "mcpServers": {
    "filesystem": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-filesystem", "YOUR_PROJECT_PATH"]
    },
    "github": {
      "command": "docker",
      "args": ["run", "-i", "--rm", "-e", "GITHUB_PERSONAL_ACCESS_TOKEN", "ghcr.io/modelcontextprotocol/servers/github:latest"],
      "env": {
        "GITHUB_PERSONAL_ACCESS_TOKEN": "YOUR_PAT_HERE"
      }
    },
    "postgres": {
      "command": "docker",
      "args": ["run", "-i", "--rm", "--network", "YOUR_NETWORK", "-e", "POSTGRES_CONNECTION_STRING=YOUR_CONNECTION", "ghcr.io/modelcontextprotocol/servers/postgres:latest"]
    },
    "memory": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-memory"]
    },
    "fetch": {
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-fetch"]
    }
  }
}
```

### MCP Servers Explained

#### 1. Filesystem MCP Server
**Purpose**: Access repository files for reading/editing code  
**Used by**: All agents  
**Capabilities**: 
- Read source code
- Create/edit files
- Search project structure

#### 2. GitHub MCP Server
**Purpose**: Manage GitHub Issues, PRs, branches, commits  
**Used by**: All agents  
**Capabilities**:
- Create/update Issues
- Create/review Pull Requests
- Read commit history
- Manage labels and milestones

**Setup**:
1. Create GitHub Personal Access Token (PAT)
   - Go to: https://github.com/settings/tokens
   - Generate new token (classic)
   - Scopes needed: `repo`, `workflow`, `write:packages`, `read:org`
2. Replace `YOUR_PAT_HERE` in config

#### 3. Postgres MCP Server
**Purpose**: Query database for analytics, testing, verification  
**Used by**: Backend Agent, Data Science Agent, QA Agent  
**Capabilities**:
- Execute SQL queries
- Inspect table schemas
- Verify data integrity

**Setup**: Ensure Docker network matches your `docker-compose.yml`

#### 4. Memory MCP Server
**Purpose**: Shared context between agents  
**Used by**: All agents  
**Capabilities**:
- Store agent decisions
- Share context across sessions
- Track long-term patterns

**Use cases**:
- Teacher Agent stores learned patterns
- Backend Agent shares API conventions
- DevOps Agent tracks deployment history

#### 5. Fetch MCP Server
**Purpose**: Retrieve web content (docs, Stack Overflow, GitHub issues)  
**Used by**: All agents when researching solutions  
**Capabilities**:
- Fetch external documentation
- Read GitHub issue discussions
- Access API documentation

### Installation Steps

1. **Install MCP CLI** (if not already installed)
   ```powershell
   npm install -g @modelcontextprotocol/cli
   ```

2. **Verify Docker is Running**
   ```powershell
   docker ps
   ```

3. **Copy Configuration**
   - Update paths and credentials in the JSON above
   - Save to your `mcp.json` location

4. **Restart VS Code**
   After updating `mcp.json`, restart VS Code for changes to take effect.

5. **Verify MCP Servers**
   In VS Code, open Copilot chat and try:
   - `@github list my repositories` → Should see your repos
   - Check if agents can access files in workspace

### Agent-Specific MCP Usage

| Agent | Primary Tools | Secondary Tools |
|-------|---------------|-----------------|
| **Backend** | filesystem, github, postgres | memory, fetch |
| **Frontend** | filesystem, github | memory, fetch |
| **DevOps** | filesystem, github, fetch | postgres, memory, azure-resources, azure-auth |
| **QA** | filesystem, github, postgres | memory, fetch |
| **Data Science** | postgres, filesystem, github | memory, fetch |
| **Teacher** | github, filesystem, memory | fetch |
| **Product Owner** | github, filesystem, memory | fetch |

### Troubleshooting MCP Servers

**GitHub MCP Not Working**
- Symptom: "GitHub authentication failed"
- Solution: Verify PAT has correct scopes and hasn't expired

**Postgres MCP Not Connecting**
- Symptom: "Cannot connect to database"
- Solution: Check Docker containers running with `docker ps`

**Filesystem MCP Not Seeing Files**
- Symptom: "File not found"
- Solution: Verify path uses proper format and restart VS Code

**Memory/Fetch MCP Not Loading**
- Symptom: "MCP server failed to start"
- Solution: Ensure Node.js installed, clear npx cache

## 💡 Example Conversations

### Backend Agent Example
```
You: @backend create a user registration endpoint with email verification

Agent: I'll create a user registration endpoint following REST best practices:

1. POST /api/users/register
   - Request: { email, password, name }
   - Validation: email format, password strength
   - Hash password with bcrypt
   - Generate email verification token
   - Store user with status=PENDING
   - Send verification email (background job)
   - Return 201 Created with user ID

2. POST /api/users/verify
   - Request: { token }
   - Validate token and expiry
   - Update user status=ACTIVE
   - Return 200 OK

[Detailed code implementation follows...]
```

### Frontend Agent Example
```
You: @frontend create a responsive navigation component with mobile menu

Agent: I'll create a responsive navigation component following best practices:

1. Desktop: Horizontal navigation bar
2. Mobile: Hamburger menu with slide-out drawer
3. Accessibility: Keyboard navigation, ARIA labels
4. Performance: Lazy load menu items
5. State: Zustand for menu open/close state

[Detailed React code with TypeScript follows...]
```

### DevOps Agent Example
```
You: @devops setup a CI/CD pipeline for a Node.js app

Agent: I'll create a GitHub Actions CI/CD pipeline:

1. Build stage:
   - Install dependencies
   - Run linting (ESLint)
   - Run tests (Jest)
   - Build application

2. Deploy stage (production):
   - Build Docker image
   - Push to container registry
   - Deploy to Azure Container Apps
   - Run smoke tests

[Detailed workflow YAML follows...]
```

## 🎯 Best Practices

### 1. Be Specific
❌ "Help me with authentication"
✅ "@backend implement JWT authentication with refresh tokens using .NET"

### 2. Provide Context
Include:
- Tech stack you're using
- Any constraints (e.g., "must work offline")
- Existing patterns in your codebase

### 3. Iterate
Agents can refine their responses:
```
You: @backend can you add rate limiting to that endpoint?
Agent: I'll add rate limiting using the AspNetCoreRateLimit package...
```

### 4. Ask for Alternatives
```
You: @backend what are alternatives to using JWT for auth?
Agent: Here are 3 alternatives:
1. Session-based auth with cookies
2. OAuth2 with external providers (Auth0, Keycloak)
3. API keys for service-to-service
[Detailed comparison follows...]
```

## 📚 Documentation Standards

All agents follow these documentation principles:

**Code Documentation:**
- Clear comments for complex logic
- XML/JSDoc for public APIs
- README updates for new features

**Architecture Decisions:**
- Create ADRs for significant choices
- Document trade-offs
- Explain alternatives considered

**Testing:**
- All code includes tests
- Minimum 80% coverage
- Document test strategies

**Knowledge Sharing:**
- Teacher agent proposes blog posts
- Document lessons learned
- Share patterns with team

## 🔄 Agent Collaboration

Agents can work together on complex tasks:

```
You: I need to build a feature where users upload images, which are processed by ML, and results displayed in the UI

Agent workflow:
1. @backend: Create image upload API with validation
2. @data-science: Build ML model for image processing
3. @frontend: Create upload UI with progress indicators
4. @devops: Setup storage (Azure Blob) and ML inference service
5. @qa: Test end-to-end flow
6. @teacher: Document the ML pipeline architecture
```

### Agent Communication Patterns

**Agents DO NOT communicate directly**. Instead, they communicate through:
- **GitHub Issues** (questions, blockers, discussions)
- **Pull Request comments** (code review, clarifications)
- **Commit messages** (context for changes)
- **MCP Memory Server** (shared patterns, conventions)

### Example: Frontend Needs Backend API

**Step 1: Frontend Agent creates GitHub Issue**
```markdown
Title: [API] Need endpoint for bulk entry status update
Labels: Backend, API, Question
@backend-agent Can you implement POST /api/entries/bulk-update-status?

Expected:
POST /api/entries/bulk-update-status
Body: { entryIds: string[], status: EntryStatus }
Response: { updated: number, errors: [] }
```

**Step 2: Backend Agent responds in Issue comments**
```markdown
@frontend-agent I can implement that. I'll follow the existing
pattern from COMP-006 (single entry status update).

Estimated: 2 hours
Will create PR linked to COMP-008.
```

**Step 3: Backend Agent implements and creates PR**
- Creates PR linked to original Issue
- Tags Frontend Agent for review

**Step 4: Teacher Agent reviews both PRs**
- Identifies **Bulk Operation Pattern**
- Updates pattern documentation
- Suggests LinkedIn article: "Optimizing Bulk Updates in REST APIs"

## 📝 Development Workflow Example

### Feature: Offline Scoresheet Entry (Example)

**Step 1: Product Owner defines feature**
```
@product-owner Define offline scoresheet entry feature for judges

Creates:
- User stories with acceptance criteria
- GitHub issues assigned to @frontend and @backend
- Updates BACKLOG.md with priority P0
```

**Step 2: Frontend Agent implementation**
- Creates feature branch: `feature/offline-scoresheet`
- Implements IndexedDB storage with Dexie.js
- Implements Background Sync API
- Creates Storybook stories
- Writes Vitest unit tests
- Updates documentation

**Step 3: Backend Agent integration (if needed)**
- Updates API to handle conflict resolution
- Implements optimistic concurrency control
- Writes xUnit integration tests

**Step 4: QA Agent verification**
- Runs Cypress E2E tests
- Tests offline scenarios (Service Worker devtools)
- Verifies IndexedDB queries
- Checks accessibility (axe-core)
- Load tests with k6

**Step 5: DevOps Agent CI/CD**
- Ensures GitHub Actions build passes
- Verifies Docker images build correctly
- Checks Lighthouse PWA score

**Step 6: Teacher Agent review (MANDATORY)**
- Identifies patterns used:
  - Offline-First Architecture (IndexedDB + Service Worker)
  - Background Sync Pattern (queue API calls)
  - Optimistic UI (update UI before server confirms)
- Creates ADR: `ADR-015-offline-scoresheet-storage-strategy.md`
- Proposes LinkedIn article: "Building Offline-First PWAs for Field Competitions"
- Updates BACKLOG: `- [x] SCORE-003`

**Step 7: Create Pull Request**
- Title: `[SCORE-003] Implement Offline Scoresheet Entry`
- Description includes Teacher Agent's implementation summary
- Links to ADR-015
- All tests passing
- Documentation updated

## 🚀 Getting Started

1. **Setup MCP Tools**: Configure `mcp.json` with required tools
2. **Choose Agent**: Select based on task (see selection guide above)
3. **Provide Context**: Be specific about requirements and tech stack
4. **Iterate**: Refine implementation with follow-up questions
5. **Review**: Have @teacher review for patterns and knowledge sharing

## ❓ Troubleshooting

**Agent doesn't understand task:**
- Provide more context and specifics
- Reference your tech stack explicitly
- Break down complex tasks into smaller pieces

**Agent suggests wrong technology:**
- Correct it: "Actually, we're using PostgreSQL, not MongoDB"
- Agents will adapt to your tech stack

**Need to switch agents:**
- Start new conversation with different agent
- Example: "@devops now help me deploy this backend service"

**MCP server not working:**
- Check Docker is running: `docker ps`
- Verify paths in `mcp.json` are correct
- Restart VS Code after config changes
- Test with: `@github list my repositories`

**Tests failing in CI/CD:**
- Check Docker containers: `docker ps`
- Verify database schema is up to date
- Review test logs in GitHub Actions
- Run tests locally first: `dotnet test`

**Documentation out of sync:**
- Run `git status` to see uncommitted changes
- Check if PR includes doc updates
- Teacher Agent should flag missing docs

## ✅ Pre-PR Checklist (All Agents)

Before creating ANY pull request:

- [ ] All tests pass (unit, integration, E2E where applicable)
- [ ] Code coverage > 80%
- [ ] API documentation updated (if adding/modifying endpoints)
- [ ] Event model updated (if adding/modifying events)
- [ ] Data model updated (if modifying database schema)
- [ ] ADR created (for significant architectural decisions)
- [ ] BACKLOG.md updated: `- [x] TASK-XXX`
- [ ] GitHub Issue updated with completion notes
- [ ] Teacher Agent reviewed and documented patterns
- [ ] No compiler warnings or errors
- [ ] Code follows project conventions (linting, formatting)

## 🎓 Getting Started Checklist

### 1. Setup MCP Servers
- [ ] Copy configuration to your `mcp.json`
- [ ] Add GitHub Personal Access Token
- [ ] Start Docker infrastructure: `docker-compose up -d`
- [ ] Restart VS Code
- [ ] Test: `@github list my repositories`

### 2. Test Each Agent
- [ ] **Backend Agent**: `@backend list all .cs files in the project`
- [ ] **Frontend Agent**: `@frontend what React patterns should I use?`
- [ ] **DevOps Agent**: `@devops show me the docker-compose.yml`
- [ ] **QA Agent**: `@qa what's our testing strategy?`
- [ ] **Teacher Agent**: `@teacher what agents are available?`
- [ ] **Product Owner**: `@product-owner help me write a user story`

### 3. First Development Task
- [ ] Pick a small P0 task from BACKLOG
- [ ] Have @product-owner create GitHub issue
- [ ] Assign to appropriate agent (@backend, @frontend, etc.)
- [ ] Agent implements + creates PR
- [ ] @qa tests implementation
- [ ] @teacher reviews patterns
- [ ] Merge PR
- [ ] Update BACKLOG.md: `- [x] TASK-XXX`

## 📖 Further Reading

- [GitHub Copilot Documentation](https://docs.github.com/en/copilot)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [Agent Best Practices](https://docs.github.com/en/copilot/using-github-copilot/agents)

---

**Remember**: These agents are tools to augment your development, not replace your judgment. Always review generated code, validate suggestions, and adapt to your specific context.
