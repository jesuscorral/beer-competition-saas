# Beer Competition SaaS - Agentes de IA para Desarrollo

Este repositorio incluye una guía completa de agentes de IA especializados para el desarrollo del sistema de gestión de competiciones de cerveza según normas BJCP.

## 🎯 ¿Qué son los Agentes de IA?

Los agentes de IA son asistentes especializados configurados en Visual Studio Code usando GitHub Copilot. Cada agente tiene expertise en un área específica del desarrollo de software (backend, frontend, testing, seguridad, etc.) y está preconfigurado con el contexto del proyecto.

## 📁 Estructura del Proyecto

```
beer-competition-saas/
├── ARCHITECTURE.md              # Arquitectura técnica del sistema
├── AGENTS_GUIDE.md             # Guía completa de 18 agentes (35+ páginas)
├── AGENTS_SUMMARY.md           # Resumen ejecutivo de agentes
├── QUICKSTART_AGENTS.md        # Guía rápida para empezar
├── README.md                   # Este archivo
└── .vscode/
    ├── settings.json           # Configuración de Copilot con contexto
    └── beer-competition-agents.code-snippets  # Snippets de 16 agentes
```

## 🚀 Quick Start

### 1. Prerequisitos

- Visual Studio Code (v1.85+)
- GitHub Copilot (suscripción activa)
- Extensiones: "GitHub Copilot" y "GitHub Copilot Chat"

### 2. Instalación

1. Clona este repositorio
2. Abre el proyecto en VS Code
3. Instala las extensiones de GitHub Copilot
4. Autentica con tu cuenta de GitHub
5. ¡Listo! Los agentes están configurados automáticamente

### 3. Primer Uso

En cualquier archivo, escribe `agent-backend` y presiona `Tab`. Verás el contexto del Backend Developer Agent. Completa tu tarea específica y Copilot te ayudará con código contextualmente relevante.

📖 **Guía completa**: Ver [QUICKSTART_AGENTS.md](./QUICKSTART_AGENTS.md)

## 🤖 Agentes Disponibles

### 18 Agentes Especializados

| # | Agente | Snippet | Área |
|---|--------|---------|------|
| 1 | System Architect | `agent-architect` | Arquitectura |
| 2 | Data Modeling | `agent-datamodel` | Base de datos |
| 3 | Feature Planning | `agent-planning` | Planificación |
| 4 | Backend Developer | `agent-backend` | Desarrollo |
| 5 | Frontend Developer | `agent-frontend` | Desarrollo |
| 6 | API Integration | `agent-api` | Desarrollo |
| 7 | Database Developer | `agent-database` | Desarrollo |
| 8 | Automated Testing | `agent-testing` | Testing |
| 9 | Code Review | `agent-review` | Calidad |
| 10 | Security Testing | `agent-security` | Seguridad |
| 11 | CI/CD Automation | `agent-cicd` | DevOps |
| 12 | Infrastructure | `agent-infra` | DevOps |
| 13 | Monitoring | `agent-monitoring` | DevOps |
| 14 | Technical Docs | `agent-docs-tech` | Documentación |
| 15 | User Docs | N/A | Documentación |
| 16 | Refactoring | `agent-refactor` | Mantenimiento |
| 17 | Performance | `agent-performance` | Optimización |
| 18 | Migration/Upgrade | N/A | Mantenimiento |

📊 **Resumen completo**: Ver [AGENTS_SUMMARY.md](./AGENTS_SUMMARY.md)

## 📚 Documentación

### Para Empezar
- **[QUICKSTART_AGENTS.md](./QUICKSTART_AGENTS.md)** - Guía rápida con ejemplos prácticos
- **[AGENTS_SUMMARY.md](./AGENTS_SUMMARY.md)** - Resumen de todos los agentes

### Referencias Completas
- **[AGENTS_GUIDE.md](./AGENTS_GUIDE.md)** - Guía detallada (35+ páginas)
- **[ARCHITECTURE.md](./ARCHITECTURE.md)** - Arquitectura técnica del sistema

## 💡 Ejemplos de Uso

### Ejemplo 1: Crear un Endpoint de API

```typescript
// Escribe: agent-backend [Tab]
// Tarea: Crear endpoint POST /api/v1/competitions

// Copilot generará algo como:
import { Request, Response } from 'express';
import { z } from 'zod';
import { prisma } from '@/lib/prisma';
import { authenticate } from '@/middleware/auth';
import { hasRole } from '@/middleware/authorization';

const competitionSchema = z.object({
  name: z.string().min(3).max(255),
  description: z.string().optional(),
  start_date: z.string().datetime(),
  end_date: z.string().datetime(),
  location: z.string(),
  max_entries: z.number().int().positive(),
  entry_fee: z.number().positive()
});

export async function createCompetition(req: Request, res: Response) {
  try {
    const data = competitionSchema.parse(req.body);
    const { userId, organizationId } = req.user;
    
    const competition = await prisma.competition.create({
      data: {
        ...data,
        organization_id: organizationId,
        created_by: userId,
        status: 'draft'
      }
    });
    
    res.status(201).json(competition);
  } catch (error) {
    // Error handling...
  }
}
```

### Ejemplo 2: Crear Componente React

```typescript
// Escribe: agent-frontend [Tab]
// Tarea: Crear componente ScoringForm

// Copilot generará el componente completo con:
// - React Hook Form
// - Validación Zod
// - Material-UI
// - TypeScript types
// - Error handling
```

### Ejemplo 3: Crear Tests

```typescript
// Escribe: agent-testing [Tab]
// Tarea: Tests para endpoint POST /api/v1/competitions

// Copilot generará suite de tests completa
```

## 🎯 Workflows Recomendados

### Nueva Feature Completa

```
1. agent-planning     → Descomponer en user stories
2. agent-architect    → Validar diseño
3. agent-datamodel    → Cambios en DB
4. agent-backend      → Implementar API
5. agent-frontend     → Implementar UI
6. agent-testing      → Crear tests
7. agent-security     → Revisar seguridad
8. agent-docs-tech    → Documentar
```

### Bug Fix Rápido

```
1. agent-testing      → Test que reproduce bug
2. agent-backend      → Fix
3. agent-review       → Revisar cambios
```

### Optimización

```
1. agent-monitoring   → Analizar métricas
2. agent-performance  → Optimizar
3. agent-testing      → Validar
```

## 🏗️ Sobre el Proyecto

Este es un sistema SaaS para gestión de competiciones de cerveza siguiendo las normas BJCP (Beer Judge Certification Program).

### Stack Tecnológico

**Backend:**
- Node.js 18+ con TypeScript
- Express.js para APIs
- Prisma ORM + PostgreSQL
- Redis para cache
- RabbitMQ para message queue
- JWT para autenticación

**Frontend:**
- React 18+ con TypeScript
- Redux Toolkit + RTK Query
- Material-UI v5
- React Hook Form + Zod

**Infraestructura:**
- Docker + Docker Compose
- Kubernetes para producción
- GitHub Actions para CI/CD
- Prometheus + Grafana para monitoring

### Características Principales

- **Multi-tenancy**: Soporte para múltiples organizaciones con aislamiento de datos
- **RBAC**: Roles (Owner, Admin, Organizer, Judge, Participant)
- **Competiciones BJCP**: Gestión completa de competiciones según normas oficiales
- **Scoring System**: Sistema de puntuación según guidelines BJCP
- **Leaderboards**: Rankings y resultados en tiempo real
- **Notificaciones**: Sistema de notificaciones por email/SMS

Ver arquitectura completa en [ARCHITECTURE.md](./ARCHITECTURE.md).

## 🎓 Niveles de Expertise

| Nivel | Agentes Recomendados |
|-------|---------------------|
| **Beginner** | planning, frontend, testing |
| **Intermediate** | backend, database, api, review |
| **Advanced** | architect, security, performance, infra, cicd |

## 📈 Beneficios

### Para Desarrolladores
- ✅ Aumenta productividad 3-5x
- ✅ Reduce errores comunes
- ✅ Mantiene consistencia en el código
- ✅ Aprende mejores prácticas
- ✅ Acelera onboarding de nuevos miembros

### Para el Proyecto
- ✅ Código más consistente y mantenible
- ✅ Mejor cobertura de tests
- ✅ Menos bugs en producción
- ✅ Documentación siempre actualizada
- ✅ Velocidad de desarrollo más rápida

## 🔒 Seguridad

Los agentes están configurados para enfatizar seguridad:
- Tenant isolation en todas las queries
- Validación de inputs
- Autenticación y autorización
- OWASP Top 10 considerations
- SQL injection prevention
- XSS prevention

## 🤝 Contribuir

Para sugerencias sobre los agentes o mejoras:
1. Revisa la documentación existente
2. Propón cambios específicos
3. Testea los agentes modificados
4. Documenta los cambios

## 📞 Soporte

- **Documentación**: Ver archivos AGENTS_*.md
- **Problemas**: Ver sección "Solución de Problemas" en QUICKSTART_AGENTS.md
- **GitHub Copilot Docs**: https://docs.github.com/en/copilot

## 🗺️ Roadmap

- [x] Definición de 18 agentes especializados
- [x] Configuración de snippets en VS Code
- [x] Documentación completa en español
- [x] Guías de uso con ejemplos prácticos
- [ ] Video tutoriales de uso
- [ ] Más ejemplos prácticos
- [ ] Plantillas de código adicionales
- [ ] Integración con otros IDEs

## 📄 Licencia

Este proyecto y su documentación están disponibles para uso educativo y de desarrollo.

## ✨ Créditos

Documentación y agentes diseñados específicamente para el sistema de competiciones de cerveza BJCP.

---

**¿Listo para empezar?**

1. 📖 Lee [QUICKSTART_AGENTS.md](./QUICKSTART_AGENTS.md)
2. 🎯 Prueba tu primer snippet: `agent-backend`
3. 🚀 Empieza a desarrollar con IA

---

*Última actualización: 2025-12-18*  
*Versión: 1.0*
