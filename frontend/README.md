# Beer Competition Platform - Frontend

React 18 + TypeScript frontend application for managing BJCP-compliant homebrew beer competitions.

## 🚀 Quick Start

### Prerequisites
- Node.js 18+ 
- npm or yarn

### Installation

```bash
# Install dependencies
npm install

# Run development server
npm run dev
```

The application will be available at `http://localhost:5173`

## 📁 Project Structure

```
frontend/
├── src/
│   ├── api/
│   │   └── client.ts              # Axios API client configuration
│   ├── components/
│   │   ├── CompetitionForm.tsx    # Competition creation form
│   │   ├── Navbar.tsx             # Navigation component
│   │   └── LanguageSelector.tsx   # Language switcher component
│   ├── hooks/
│   │   └── useCompetitions.ts     # TanStack Query hooks for competitions
│   ├── locales/
│   │   ├── en/
│   │   │   └── translation.json   # English translations
│   │   └── es/
│   │       └── translation.json   # Spanish translations
│   ├── pages/
│   │   ├── Home.tsx               # Landing page
│   │   └── CompetitionCreate.tsx  # Competition creation page
│   ├── schemas/
│   │   └── competition.ts         # Zod validation schemas
│   ├── App.tsx                    # Root component with routing
│   ├── main.tsx                   # Application entry point
│   ├── i18n.ts                    # i18n configuration
│   ├── index.css                  # Global styles + Tailwind imports
│   └── vite-env.d.ts             # TypeScript definitions
├── docs/
│   └── I18N.md                    # Internationalization guide
├── public/                        # Static assets
├── index.html                     # HTML template
├── package.json                   # Dependencies and scripts
├── tsconfig.json                  # TypeScript configuration
├── vite.config.ts                # Vite configuration
├── tailwind.config.js            # Tailwind CSS configuration
├── postcss.config.js             # PostCSS configuration
├── .env.development              # Development environment variables
└── .env.production               # Production environment variables
```

## 🛠️ Tech Stack

- **React 18** - UI library
- **TypeScript** - Type safety
- **Vite** - Build tool and dev server
- **React Router** - Client-side routing
- **TanStack Query** - Server state management
- **React Hook Form** - Form handling
- **Zod** - Schema validation
- **Axios** - HTTP client
- **Tailwind CSS** - Styling
- **React Hot Toast** - Toast notifications
- **i18next** - Internationalization (i18n)
  - `react-i18next` - React bindings
  - `i18next-browser-languagedetector` - Automatic language detection

## 🌍 Internationalization

The application supports multiple languages out of the box:
- **English (en)** - Default
- **Spanish (es)** - Español

Users can switch languages using the language selector in the navbar. All user-facing text is stored in translation files for easy localization.

**See [docs/I18N.md](docs/I18N.md) for complete i18n guide and how to add new languages.**

## 🌐 Environment Variables

### Development (`.env.development`)
```bash
VITE_API_BASE_URL=http://localhost:7038
VITE_TENANT_ID=11111111-1111-1111-1111-111111111111
```

### Production (`.env.production`)
```bash
VITE_API_BASE_URL=https://api.beercomp.com
VITE_TENANT_ID=<your-tenant-id>
```

**Important**: 
- `VITE_TENANT_ID` must be set to a valid tenant ID for your environment
- The default development tenant ID (`11111111-1111-1111-1111-111111111111`) matches the tenant inserted by `Insert-DevelopmentTenant.ps1`
- Using the development tenant ID in production will trigger a console warning
- In the future, tenant ID will be extracted from the JWT token after authentication

## 📝 Available Scripts

```bash
# Start development server (port 5173)
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Run linter
npm run lint
```

## 🔗 API Integration

The frontend connects to the BFF (Backend-for-Frontend) API Gateway:
- **Development**: `http://localhost:5190` (BFF HTTP endpoint)
- **HTTPS Alternative**: `https://localhost:7038` (requires SSL cert trust)
- **Tenant ID**: Configured via `VITE_TENANT_ID` environment variable (defaults to `11111111-1111-1111-1111-111111111111` for local development)
- **BFF Gateway handles**:
  - Routing to microservices (Competition Service, Judging Service)
  - Authentication (Keycloak) - Coming soon
  - CORS configuration
  - Request/response transformation
  - Multi-tenancy enforcement via `X-Tenant-ID` header

## 🎯 Features (Issue #80)

### ✅ Implemented
- [x] React 18 + TypeScript project setup
- [x] Vite build configuration
- [x] Tailwind CSS styling
- [x] React Router navigation
- [x] Competition creation form
- [x] Competition list page with responsive grid
- [x] Form validation with Zod
- [x] API client with Axios (tenant_id header auto-injection)
- [x] TanStack Query for data fetching
- [x] Toast notifications
- [x] Responsive design
- [x] Internationalization (i18n) with English and Spanish support
- [x] Language selector component with flag emojis
- [x] Automatic language detection (browser + localStorage)
- [x] Validation messages externalized to translation files
- [x] Date format handling (ISO 8601 conversion)
- [x] BFF integration with proper CORS configuration

### 🚧 Coming Soon
- [ ] Authentication with Keycloak
- [ ] User login/registration
- [ ] Competition listing
- [ ] Entry submission forms
- [ ] Judge dashboard
- [ ] Offline PWA capabilities

## 🐛 Debugging in VSCode

### Quick Start
1. **Press F5** to start debugging (opens Chrome automatically)
2. Set breakpoints by clicking on line numbers
3. Debug panel shows variables, call stack, and more

### Available Debug Configurations
- **Launch Chrome (Frontend)** ⭐ - Auto-starts server + opens Chrome with debugger
- **Attach to Chrome** - Connects to running Chrome instance  
- **Launch Edge (Frontend)** - Same as Chrome but uses Edge
- **Debug Vite Server** - Debug Vite configuration
- **Full Stack Debug** - Starts both server and debugger together

### Common Debugging Scenarios
```typescript
// Set breakpoint in form submission
const onSubmit = (data: CompetitionFormData) => {
  debugger; // Pause execution here
  createCompetitionMutation.mutate(data);
};

// Debug API calls
mutationFn: async (data) => {
  debugger; // Inspect request data
  const response = await apiClient.post('/api/competitions', data);
  debugger; // Inspect response
  return response.data;
}
```

**See [`.vscode/README.md`](../.vscode/README.md) for complete debugging guide**

## 🧪 Testing the Application

### Manual E2E Test
1. Start the BFF Gateway: `cd backend/BFF.ApiGateway && dotnet run`
2. Start the backend service: `cd backend/Host/BeerCompetition.Host && dotnet run`
3. Start the frontend: `npm run dev`
4. Navigate to `http://localhost:5173/competitions/create`
5. Fill in the form:
   - Competition Name: "Spring Classic 2025"
   - Description: "Annual spring beer competition"
   - Registration Deadline: (future date)
   - Judging Date: (date after registration deadline)
6. Click "Create Competition"
7. Verify success toast message appears
8. Check backend logs for successful creation

## 🐛 Troubleshooting

### CORS Errors
- Ensure BFF Gateway is running on port 7038
- Check `appsettings.json` has `http://localhost:5173` in `AllowedOrigins`

### API Connection Failed
- Verify BFF Gateway is running: `https://localhost:7038/api/competitions`
- Check browser console for detailed error messages
- Verify `.env.development` has correct `VITE_API_BASE_URL`

### Build Errors
```bash
# Clear node_modules and reinstall
rm -rf node_modules
npm install

# Clear Vite cache
rm -rf node_modules/.vite
```

## 📚 Code Style Guidelines

### Components
- Use functional components with TypeScript
- Export named components (not default)
- Use hooks for state and side effects

### Naming Conventions
- Components: PascalCase (`CompetitionForm.tsx`)
- Hooks: camelCase with `use` prefix (`useCompetitions.ts`)
- Utilities: camelCase (`apiClient.ts`)

### Import Order
1. React imports
2. External libraries
3. Internal components/hooks
4. Types/schemas
5. Styles

## 🔐 Security Notes

### Current State (MVP)
- **Authentication disabled** temporarily for testing
- BFF Gateway allows unauthenticated requests to `/api/competitions`
- ⚠️ **DO NOT deploy to production without authentication**

### Future Implementation
- Keycloak integration for JWT tokens
- Protected routes requiring authentication
- Role-based access control (Organizer, Judge, Entrant)

## 📖 Related Documentation

- [Architecture Documentation](../docs/architecture/ARCHITECTURE.md)
- [ADR-007: Frontend Architecture](../docs/architecture/decisions/ADR-007-frontend-architecture.md)
- [Issue #80: Bootstrap React Frontend Project](https://github.com/jesuscorral/beer-competition-saas/issues/80)

## 🤝 Contributing

Follow the project's contribution guidelines:
1. Create feature branch: `git checkout -b <issue-number>-<short-description>`
2. Write code in **English** (comments, commits, documentation)
3. Follow TypeScript strict mode (no `any` types)
4. Test manually before committing
5. Write descriptive commit messages: `feat: implement competition form (#80)`

## 📄 License

This project is part of the Beer Competition SaaS Platform.
