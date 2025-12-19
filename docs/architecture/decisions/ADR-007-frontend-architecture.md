# ADR-007: Frontend Architecture

**Date**: 2025-12-19  
**Status**: Accepted  
**Deciders**: Architecture Team  
**Context**: Beer Competition SaaS Platform

---

## Context and Problem Statement

Our platform requires a **modern, responsive web application** that supports:
- **Multiple User Roles**: Organizers, judges, entrants, stewards (different workflows per role)
- **Offline Capability**: Judges must submit scoresheets without internet connectivity (competition venues often have poor WiFi)
- **Mobile-First**: Judges use tablets/phones during judging sessions
- **Real-Time Updates**: Competition progress, leaderboards, flight assignments
- **Complex State Management**: Multi-step forms, scoresheet entry, file uploads
- **Performance**: Fast loading, smooth interactions for 50+ concurrent judges

**Challenges:**
- Offline scoresheet entry requires **service workers + local storage**
- Complex judging workflows need **robust state management**
- Multi-tenant isolation must be enforced **client-side** (no cross-tenant data leaks)
- Mobile-first design requires **responsive UI** across screen sizes

How do we build a **scalable, maintainable frontend** that balances modern UX with offline reliability?

---

## Decision Drivers

- **Offline-First**: Judges must work without connectivity
- **Mobile Responsive**: Tablet/phone support for judging
- **Developer Experience**: Fast development, strong typing, good debugging
- **Performance**: Fast initial load, smooth interactions
- **State Management**: Handle complex client state (forms, offline queue, auth)
- **Component Reusability**: Shared UI components across features
- **SEO**: Not critical (SaaS behind login), but nice-to-have for public pages
- **Ecosystem**: Large community, mature libraries

---

## Considered Options

### 1. React + TypeScript
**Approach**: Component-based SPA with hooks and context

**Pros:**
- ✅ **Largest ecosystem**: Massive library support
- ✅ **Strong typing**: TypeScript integration excellent
- ✅ **Component reusability**: Easy to share components
- ✅ **PWA support**: Service workers via Workbox

**Cons:**
- ❌ **Bundle size**: Larger than Svelte/Preact
- ❌ **Boilerplate**: More setup than Vue/Svelte

---

### 2. Vue.js + TypeScript
**Approach**: Progressive framework with composition API

**Pros:**
- ✅ **Simpler learning curve**: Easier than React for beginners
- ✅ **Built-in state**: Pinia simpler than Redux

**Cons:**
- ❌ **Smaller ecosystem**: Fewer libraries than React
- ❌ **TypeScript support**: Good but not as mature as React

---

### 3. Angular
**Approach**: Full-featured framework with RxJS

**Pros:**
- ✅ **Batteries included**: Routing, forms, HTTP built-in
- ✅ **TypeScript native**: Designed for TypeScript

**Cons:**
- ❌ **Heavy**: Large framework, steep learning curve
- ❌ **Opinionated**: Less flexibility
- ❌ **Smaller ecosystem**: Compared to React

---

### 4. Svelte + SvelteKit
**Approach**: Compiler-based framework

**Pros:**
- ✅ **Tiny bundle size**: Compiles to vanilla JS
- ✅ **Simple syntax**: Less boilerplate

**Cons:**
- ❌ **Smaller ecosystem**: Fewer libraries
- ❌ **Less mature**: Younger framework
- ❌ **Hiring**: Harder to find Svelte developers

---

## Decision Outcome

**Chosen Option**: **React 18 + TypeScript + Progressive Web App (PWA)**

### Technology Stack
- **React 18**: Component library (hooks, concurrent features)
- **TypeScript**: Type safety and better DX
- **Vite**: Build tool (fast HMR, optimized builds)
- **TanStack Query**: Server state management (caching, refetching, optimistic updates)
- **Zustand**: Client state management (auth, UI state)
- **React Router**: Client-side routing
- **Tailwind CSS**: Utility-first styling
- **Service Workers (Workbox)**: Offline support
- **IndexedDB (Dexie.js)**: Offline data storage
- **React Hook Form**: Form validation
- **Zod**: Schema validation

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     React PWA (SPA)                         │
│                                                             │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────┐  │
│  │   Pages      │  │  Components  │  │   Hooks         │  │
│  │  (Routes)    │  │  (Reusable)  │  │  (Logic)        │  │
│  └──────────────┘  └──────────────┘  └─────────────────┘  │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │            State Management                          │  │
│  │  ┌─────────────────┐  ┌──────────────────────────┐  │  │
│  │  │  TanStack Query │  │  Zustand (Client State)  │  │  │
│  │  │ (Server State)  │  │  - Auth                  │  │  │
│  │  │ - Competitions  │  │  - Theme                 │  │  │
│  │  │ - Flights       │  │  - Offline Queue         │  │  │
│  │  │ - Scoresheets   │  │                          │  │  │
│  │  └─────────────────┘  └──────────────────────────┘  │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │            Offline Support (PWA)                     │  │
│  │  ┌────────────────┐  ┌──────────────────────────┐   │  │
│  │  │ Service Worker │  │  IndexedDB (Dexie.js)    │   │  │
│  │  │   (Workbox)    │  │  - Scoresheets Queue     │   │  │
│  │  │ - Cache API    │  │  - Flight Assignments    │   │  │
│  │  │ - Fetch Proxy  │  │  - User Preferences      │   │  │
│  │  └────────────────┘  └──────────────────────────┘   │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
└─────────────────────────────────────────────────────────────┘
                          │
                          ▼
                  ┌───────────────┐
                  │  BFF / API    │
                  │   Gateway     │
                  └───────────────┘
```

---

## Implementation Details

### 1. Project Structure

```
frontend/
├── public/
│   ├── manifest.json          # PWA manifest
│   ├── service-worker.js       # Service worker (generated by Workbox)
│   └── icons/                  # App icons (multiple sizes)
├── src/
│   ├── pages/                  # Route components
│   │   ├── CompetitionListPage.tsx
│   │   ├── JudgingFlightPage.tsx
│   │   ├── ScoresheetEntryPage.tsx
│   │   └── LeaderboardPage.tsx
│   ├── components/             # Reusable UI components
│   │   ├── common/
│   │   │   ├── Button.tsx
│   │   │   ├── Card.tsx
│   │   │   └── Modal.tsx
│   │   ├── forms/
│   │   │   ├── ScoresheetForm.tsx
│   │   │   └── EntryForm.tsx
│   │   └── layout/
│   │       ├── Header.tsx
│   │       ├── Sidebar.tsx
│   │       └── Layout.tsx
│   ├── hooks/                  # Custom hooks
│   │   ├── useAuth.ts
│   │   ├── useCompetitions.ts
│   │   ├── useOfflineSync.ts
│   │   └── useScoresheetSubmit.ts
│   ├── api/                    # API client
│   │   ├── client.ts           # Axios/Fetch wrapper
│   │   ├── competitions.ts
│   │   ├── scoresheets.ts
│   │   └── types.ts            # API response types
│   ├── store/                  # Zustand stores
│   │   ├── authStore.ts
│   │   ├── offlineStore.ts
│   │   └── uiStore.ts
│   ├── db/                     # IndexedDB (Dexie)
│   │   └── schema.ts
│   ├── utils/
│   │   ├── validation.ts
│   │   └── formatters.ts
│   └── App.tsx
├── vite.config.ts
├── tsconfig.json
└── package.json
```

---

### 2. Progressive Web App (PWA) Configuration

#### Manifest (public/manifest.json)
```json
{
  "name": "Beer Competition Manager",
  "short_name": "BeerComp",
  "description": "Manage BJCP competitions with offline scoresheet entry",
  "start_url": "/",
  "display": "standalone",
  "theme_color": "#f59e0b",
  "background_color": "#ffffff",
  "icons": [
    {
      "src": "/icons/icon-192x192.png",
      "sizes": "192x192",
      "type": "image/png",
      "purpose": "any maskable"
    },
    {
      "src": "/icons/icon-512x512.png",
      "sizes": "512x512",
      "type": "image/png",
      "purpose": "any maskable"
    }
  ],
  "orientation": "portrait-primary",
  "categories": ["productivity", "business"]
}
```

#### Service Worker (Workbox)
```javascript
// vite.config.ts
import { VitePWA } from 'vite-plugin-pwa';

export default defineConfig({
  plugins: [
    VitePWA({
      registerType: 'autoUpdate',
      workbox: {
        globPatterns: ['**/*.{js,css,html,ico,png,svg,woff2}'],
        runtimeCaching: [
          {
            urlPattern: /^https:\/\/api\.beercompetition\.com\/api\/.*/,
            handler: 'NetworkFirst',  // Try network, fallback to cache
            options: {
              cacheName: 'api-cache',
              expiration: {
                maxEntries: 100,
                maxAgeSeconds: 60 * 60 * 24  // 24 hours
              },
              networkTimeoutSeconds: 10
            }
          },
          {
            urlPattern: /^https:\/\/.*\.(png|jpg|jpeg|svg|gif)$/,
            handler: 'CacheFirst',  // Images always from cache
            options: {
              cacheName: 'image-cache',
              expiration: {
                maxEntries: 50,
                maxAgeSeconds: 60 * 60 * 24 * 30  // 30 days
              }
            }
          }
        ]
      }
    })
  ]
});
```

---

### 3. Offline Scoresheet Entry

#### IndexedDB Schema (Dexie.js)
```typescript
// src/db/schema.ts
import Dexie, { Table } from 'dexie';

export interface OfflineScoresheet {
  id?: number;  // Auto-increment
  flightId: string;
  entryId: string;
  judgingNumber: number;
  aroma: number;
  appearance: number;
  flavor: number;
  mouthfeel: number;
  overall: number;
  comments: string;
  createdAt: Date;
  syncStatus: 'pending' | 'syncing' | 'synced' | 'failed';
}

export class BeerCompDB extends Dexie {
  scoresheets!: Table<OfflineScoresheet>;

  constructor() {
    super('BeerCompDB');
    this.version(1).stores({
      scoresheets: '++id, flightId, syncStatus'
    });
  }
}

export const db = new BeerCompDB();
```

#### Offline Sync Hook
```typescript
// src/hooks/useOfflineSync.ts
import { useEffect } from 'react';
import { db } from '../db/schema';
import { apiClient } from '../api/client';

export const useOfflineSync = () => {
  useEffect(() => {
    const syncPendingScoresheets = async () => {
      if (!navigator.onLine) return;

      const pending = await db.scoresheets
        .where('syncStatus')
        .equals('pending')
        .toArray();

      for (const scoresheet of pending) {
        try {
          // Mark as syncing
          await db.scoresheets.update(scoresheet.id!, { syncStatus: 'syncing' });

          // Send to API
          await apiClient.post('/api/scoresheets', scoresheet);

          // Mark as synced
          await db.scoresheets.update(scoresheet.id!, { 
            syncStatus: 'synced',
            syncedAt: new Date()
          });
        } catch (error) {
          // Mark as failed (retry later)
          await db.scoresheets.update(scoresheet.id!, { syncStatus: 'failed' });
        }
      }
    };

    // Sync when online
    window.addEventListener('online', syncPendingScoresheets);
    syncPendingScoresheets();  // Initial sync

    return () => window.removeEventListener('online', syncPendingScoresheets);
  }, []);
};
```

#### Scoresheet Entry Component
```typescript
// src/pages/ScoresheetEntryPage.tsx
import { useMutation } from '@tanstack/react-query';
import { db } from '../db/schema';
import { apiClient } from '../api/client';

export const ScoresheetEntryPage = () => {
  const submitScoresheet = useMutation({
    mutationFn: async (scoresheet: OfflineScoresheet) => {
      if (navigator.onLine) {
        // Online: Send directly to API
        return apiClient.post('/api/scoresheets', scoresheet);
      } else {
        // Offline: Save to IndexedDB
        return db.scoresheets.add({ ...scoresheet, syncStatus: 'pending' });
      }
    },
    onSuccess: () => {
      toast.success(
        navigator.onLine 
          ? 'Scoresheet submitted!' 
          : 'Saved offline. Will sync when online.'
      );
    }
  });

  const handleSubmit = (data: ScoresheetFormData) => {
    submitScoresheet.mutate({
      flightId: data.flightId,
      entryId: data.entryId,
      judgingNumber: data.judgingNumber,
      aroma: data.aroma,
      appearance: data.appearance,
      flavor: data.flavor,
      mouthfeel: data.mouthfeel,
      overall: data.overall,
      comments: data.comments,
      createdAt: new Date(),
      syncStatus: 'pending'
    });
  };

  return (
    <form onSubmit={handleSubmit}>
      {/* Scoresheet form fields */}
      {!navigator.onLine && (
        <div className="bg-yellow-100 p-4 rounded">
          ⚠️ You are offline. Scoresheets will be saved locally and synced when online.
        </div>
      )}
      <button type="submit" disabled={submitScoresheet.isPending}>
        {submitScoresheet.isPending ? 'Saving...' : 'Submit Scoresheet'}
      </button>
    </form>
  );
};
```

---

### 4. State Management

#### Server State (TanStack Query)
```typescript
// src/hooks/useCompetitions.ts
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '../api/client';

export const useCompetitions = () => {
  return useQuery({
    queryKey: ['competitions'],
    queryFn: async () => {
      const response = await apiClient.get('/api/competitions');
      return response.data;
    },
    staleTime: 5 * 60 * 1000,  // 5 minutes
    gcTime: 10 * 60 * 1000,  // 10 minutes
    refetchOnWindowFocus: true
  });
};

export const useCompetition = (id: string) => {
  return useQuery({
    queryKey: ['competition', id],
    queryFn: async () => {
      const response = await apiClient.get(`/api/competitions/${id}`);
      return response.data;
    },
    enabled: !!id  // Only fetch if ID exists
  });
};
```

#### Client State (Zustand)
```typescript
// src/store/authStore.ts
import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface AuthState {
  accessToken: string | null;
  user: User | null;
  tenantId: string | null;
  login: (token: string, user: User, tenantId: string) => void;
  logout: () => void;
  isAuthenticated: () => boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      accessToken: null,
      user: null,
      tenantId: null,
      login: (token, user, tenantId) => {
        set({ accessToken: token, user, tenantId });
      },
      logout: () => {
        set({ accessToken: null, user: null, tenantId: null });
      },
      isAuthenticated: () => !!get().accessToken
    }),
    {
      name: 'auth-storage',  // LocalStorage key
      partialize: (state) => ({  // Only persist token
        accessToken: state.accessToken,
        tenantId: state.tenantId
      })
    }
  )
);
```

---

### 5. Responsive Design (Tailwind CSS)

#### Mobile-First Component
```typescript
// src/components/ScoresheetForm.tsx
export const ScoresheetForm = () => {
  return (
    <div className="max-w-4xl mx-auto p-4">
      {/* Mobile: Vertical layout */}
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <div>
          <label className="block text-sm font-medium text-gray-700">
            Aroma (0-12)
          </label>
          <input
            type="number"
            min="0"
            max="12"
            className="mt-1 block w-full rounded-md border-gray-300 
                       focus:border-amber-500 focus:ring-amber-500
                       text-lg"  {/* Large touch targets */}
          />
        </div>
        
        {/* Tablet: 2 columns */}
        <div className="md:col-span-1">
          <label className="block text-sm font-medium text-gray-700">
            Flavor (0-20)
          </label>
          <input
            type="number"
            min="0"
            max="20"
            className="mt-1 block w-full rounded-md"
          />
        </div>
      </div>

      {/* Comments: Full width on all screens */}
      <div className="mt-4">
        <label>Comments</label>
        <textarea 
          rows={6}
          className="mt-1 block w-full rounded-md"
        />
      </div>

      {/* Sticky button on mobile */}
      <div className="sticky bottom-0 bg-white p-4 border-t md:static md:mt-6">
        <button className="w-full md:w-auto px-6 py-3 bg-amber-600 text-white rounded-lg">
          Submit Scoresheet
        </button>
      </div>
    </div>
  );
};
```

---

### 6. Authentication Flow

```typescript
// src/api/client.ts
import axios from 'axios';
import { useAuthStore } from '../store/authStore';

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  timeout: 10000
});

// Request interceptor: Add JWT token
apiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Response interceptor: Handle 401 (token expired)
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      // Token expired, redirect to login
      useAuthStore.getState().logout();
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);
```

---

## Consequences

### Positive
✅ **Offline Capability**: Judges work without connectivity (service workers + IndexedDB)  
✅ **Mobile-First**: Responsive design works on all devices  
✅ **Type Safety**: TypeScript catches errors at compile-time  
✅ **Performance**: Vite provides fast HMR, optimized builds  
✅ **State Management**: TanStack Query handles server caching, Zustand for client state  
✅ **Developer Experience**: React ecosystem mature, large community  
✅ **PWA Benefits**: Installable app, app-like experience  

### Negative
❌ **Bundle Size**: React larger than Svelte (~150KB gzipped)  
❌ **Offline Sync Complexity**: Conflict resolution when multiple judges edit same entry  
❌ **IndexedDB Quota**: iOS Safari limits to ~50MB (acceptable for scoresheets)  
❌ **PWA Limitations on iOS**: Service worker restrictions, no push notifications  

### Risks
⚠️ **Offline Sync Conflicts**: Last-write-wins strategy may lose data (mitigated with timestamps)  
⚠️ **iOS PWA Limitations**: Service workers restricted on iOS (still functional, just limited)  
⚠️ **LocalStorage XSS**: Access token in localStorage vulnerable (mitigated with short expiry + refresh tokens)  

---

## Alternatives Considered

### Why not Next.js (SSR)?
- ❌ **Server-side rendering**: Not needed (SaaS behind login, no SEO)
- ❌ **Complexity**: SSR adds unnecessary overhead
- ✅ **Vite**: Simpler, faster for SPA use case

### Why not Vue.js?
- ✅ **React ecosystem**: Larger library selection
- ✅ **TypeScript**: Better React integration
- ✅ **Hiring**: Easier to find React developers

### Why not Angular?
- ❌ **Heavy**: Large framework, steep learning curve
- ❌ **Opinionated**: Less flexibility than React
- ✅ **React**: More flexible, smaller bundle

---

## Related Decisions

- [ADR-001: Tech Stack Selection](ADR-001-tech-stack-selection.md) (React choice)
- [ADR-004: Authentication & Authorization](ADR-004-authentication-authorization.md) (JWT flow)

---

## References

- [React Documentation](https://react.dev/)
- [Vite Documentation](https://vitejs.dev/)
- [TanStack Query](https://tanstack.com/query/latest)
- [Zustand](https://github.com/pmndrs/zustand)
- [Workbox (Service Workers)](https://developer.chrome.com/docs/workbox/)
- [Dexie.js (IndexedDB)](https://dexie.org/)
- [Tailwind CSS](https://tailwindcss.com/)
- [PWA Best Practices](https://web.dev/progressive-web-apps/)
