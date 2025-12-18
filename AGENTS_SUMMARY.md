# Resumen de Agentes para Visual Studio Code

Este documento proporciona un resumen ejecutivo de todos los agentes de IA recomendados para el desarrollo del sistema de competiciones de cerveza BJCP.

## 📊 Resumen por Categoría

| Categoría | Número de Agentes | Propósito |
|-----------|------------------|-----------|
| Planificación y Arquitectura | 3 | Diseño de sistema y planificación |
| Desarrollo | 4 | Implementación de código |
| Testing y Calidad | 3 | Aseguramiento de calidad |
| DevOps e Infraestructura | 3 | Despliegue y operaciones |
| Documentación | 2 | Documentación técnica y usuario |
| Revisión y Mantenimiento | 3 | Mejora continua |
| **TOTAL** | **18** | **Ciclo completo de desarrollo** |

---

## 📋 Lista Completa de Agentes

### 1️⃣ Planificación y Arquitectura

#### 1. System Architect Agent
- **Snippet**: `agent-architect`
- **Uso**: Decisiones arquitectónicas, diseño de microservicios
- **Cuándo usarlo**: Antes de implementar nuevas features grandes, al diseñar nuevos servicios
- **Expertise**: Microservicios, multi-tenancy, escalabilidad, patrones de diseño

#### 2. Data Modeling Agent
- **Snippet**: `agent-datamodel`
- **Uso**: Diseño de schemas, relaciones, índices
- **Cuándo usarlo**: Al crear nuevas tablas, optimizar queries, diseñar migraciones
- **Expertise**: PostgreSQL, normalización, índices, Prisma ORM

#### 3. Feature Planning Agent
- **Snippet**: `agent-planning`
- **Uso**: Descomposición de features en user stories y tareas
- **Cuándo usarlo**: Al inicio de cada sprint, al recibir nuevos requisitos
- **Expertise**: User stories, criterios de aceptación, estimación

---

### 2️⃣ Desarrollo

#### 4. Backend Developer Agent
- **Snippet**: `agent-backend`
- **Uso**: APIs, servicios, lógica de negocio
- **Cuándo usarlo**: Implementar endpoints, servicios, middleware
- **Expertise**: Node.js, TypeScript, Express, Prisma, autenticación JWT

#### 5. Frontend Developer Agent
- **Snippet**: `agent-frontend`
- **Uso**: Componentes React, UI/UX
- **Cuándo usarlo**: Crear interfaces, formularios, dashboards
- **Expertise**: React 18+, TypeScript, Material-UI, Redux Toolkit

#### 6. API Integration Agent
- **Snippet**: `agent-api`
- **Uso**: Conectar frontend con backend
- **Cuándo usarlo**: Integrar APIs, manejar estado, caching
- **Expertise**: RTK Query, Axios, JWT tokens, error handling

#### 7. Database Developer Agent
- **Snippet**: `agent-database`
- **Uso**: Migraciones, queries, optimización DB
- **Cuándo usarlo**: Crear migraciones, optimizar queries lentas, diseñar índices
- **Expertise**: PostgreSQL, Prisma migrations, query optimization

---

### 3️⃣ Testing y Calidad

#### 8. Automated Testing Agent
- **Snippet**: `agent-testing`
- **Uso**: Tests unitarios, integración, E2E
- **Cuándo usarlo**: Después de implementar features, antes de commits
- **Expertise**: Jest, Supertest, React Testing Library, Playwright

#### 9. Code Review Agent
- **Snippet**: `agent-review`
- **Uso**: Revisión de código para calidad
- **Cuándo usarlo**: Antes de PR, después de implementación
- **Expertise**: SOLID, Clean Code, performance, security basics

#### 10. Security Testing Agent
- **Snippet**: `agent-security`
- **Uso**: Identificar vulnerabilidades
- **Cuándo usarlo**: Antes de deploy, después de cambios en auth/authz
- **Expertise**: OWASP Top 10, SQL injection, XSS, CSRF, tenant isolation

---

### 4️⃣ DevOps e Infraestructura

#### 11. CI/CD Automation Agent
- **Snippet**: `agent-cicd`
- **Uso**: Pipelines de CI/CD
- **Cuándo usarlo**: Setup inicial, modificar pipelines, automatización
- **Expertise**: GitHub Actions, Docker, testing automation

#### 12. Infrastructure Agent
- **Snippet**: `agent-infra`
- **Uso**: Configuración de infraestructura
- **Cuándo usarlo**: Setup inicial, configurar environments, Kubernetes
- **Expertise**: Docker Compose, Kubernetes, networking, databases

#### 13. Monitoring and Observability Agent
- **Snippet**: `agent-monitoring`
- **Uso**: Logging, métricas, alertas
- **Cuándo usarlo**: Implementar observabilidad, debugging producción
- **Expertise**: Winston, Prometheus, Grafana, structured logging

---

### 5️⃣ Documentación

#### 14. Technical Documentation Agent
- **Snippet**: `agent-docs-tech`
- **Uso**: Docs técnicas, APIs, arquitectura
- **Cuándo usarlo**: Documentar APIs, escribir ADRs, actualizar ARCHITECTURE.md
- **Expertise**: OpenAPI/Swagger, Markdown, diagramas, ADRs

#### 15. User Documentation Agent
- **Snippet**: N/A (uso manual)
- **Uso**: Guías de usuario, tutoriales
- **Cuándo usarlo**: Crear ayuda para usuarios finales
- **Expertise**: Manuales, FAQs, onboarding

---

### 6️⃣ Revisión y Mantenimiento

#### 16. Refactoring Agent
- **Snippet**: `agent-refactor`
- **Uso**: Mejora de código existente
- **Cuándo usarlo**: Código legacy, code smells, alta complejidad
- **Expertise**: Patrones de refactoring, SOLID, Clean Code

#### 17. Performance Optimization Agent
- **Snippet**: `agent-performance`
- **Uso**: Optimización de performance
- **Cuándo usarlo**: Queries lentas, problemas de performance, optimización
- **Expertise**: Query optimization, caching, frontend performance

#### 18. Migration and Upgrade Agent
- **Snippet**: N/A (uso manual)
- **Uso**: Migraciones de datos, upgrades
- **Cuándo usarlo**: Cambios de schema, actualizar dependencias
- **Expertise**: Data migrations, zero-downtime deployments

---

## 🎯 Workflows Recomendados

### Workflow 1: Implementar Nueva Feature

```
1. agent-planning → Planificar y descomponer feature
2. agent-architect → Validar diseño arquitectónico
3. agent-datamodel → Diseñar cambios en DB si es necesario
4. agent-backend → Implementar servicios y APIs
5. agent-frontend → Implementar UI
6. agent-api → Integrar frontend con backend
7. agent-testing → Crear tests
8. agent-security → Revisar seguridad
9. agent-review → Revisar calidad de código
10. agent-docs-tech → Documentar
```

### Workflow 2: Bug Fix

```
1. agent-testing → Crear test que reproduzca el bug
2. agent-backend o agent-frontend → Fix del bug
3. agent-testing → Verificar que tests pasen
4. agent-review → Revisar cambios
5. agent-security → Si afecta auth/authz
```

### Workflow 3: Optimización de Performance

```
1. agent-monitoring → Analizar métricas y logs
2. agent-performance → Identificar cuellos de botella
3. agent-database → Optimizar queries si es necesario
4. agent-backend → Optimizar lógica de servidor
5. agent-frontend → Optimizar UI si es necesario
6. agent-testing → Validar que optimizaciones funcionen
7. agent-monitoring → Verificar mejoras en métricas
```

### Workflow 4: Setup Inicial de Proyecto

```
1. agent-architect → Diseño de arquitectura general
2. agent-datamodel → Diseño de schema completo
3. agent-infra → Setup de Docker Compose para desarrollo
4. agent-cicd → Setup de GitHub Actions
5. agent-backend → Implementar servicios core
6. agent-frontend → Setup de proyecto React
7. agent-monitoring → Implementar logging básico
8. agent-docs-tech → Documentar setup
```

### Workflow 5: Pre-Deploy Checklist

```
1. agent-testing → Ejecutar suite completa de tests
2. agent-security → Scan de seguridad
3. agent-performance → Verificar performance metrics
4. agent-review → Code review final
5. agent-docs-tech → Actualizar changelog y docs
6. agent-cicd → Deploy a staging
7. agent-monitoring → Verificar métricas post-deploy
```

---

## 📊 Matriz de Uso por Rol

| Rol del Desarrollador | Agentes Más Usados |
|----------------------|-------------------|
| **Full-Stack Developer** | backend, frontend, api, database, testing |
| **Backend Developer** | backend, database, testing, security, performance |
| **Frontend Developer** | frontend, api, testing, performance |
| **DevOps Engineer** | cicd, infra, monitoring, security |
| **QA Engineer** | testing, security, review |
| **Tech Lead** | architect, planning, review, docs-tech |
| **Product Owner** | planning, docs-user |

---

## 🎓 Nivel de Expertise Requerido

| Agente | Nivel | Notas |
|--------|-------|-------|
| Feature Planning | Beginner | Fácil de usar, buenos para empezar |
| Frontend Developer | Beginner | Bien estructurado, outputs predecibles |
| Backend Developer | Intermediate | Requiere entender arquitectura |
| Database Developer | Intermediate | Requiere conocer SQL y Prisma |
| API Integration | Intermediate | Requiere entender REST y state management |
| Testing | Intermediate | Requiere entender testing concepts |
| System Architect | Advanced | Para decisiones arquitectónicas importantes |
| Security Testing | Advanced | Requiere conocimiento de security |
| Performance | Advanced | Requiere analizar métricas y profiling |
| Infrastructure | Advanced | Requiere conocer DevOps y Kubernetes |
| CI/CD | Advanced | Requiere entender pipelines |
| Monitoring | Advanced | Requiere entender observabilidad |

---

## 🔄 Frecuencia de Uso

| Agente | Frecuencia | Cuándo |
|--------|-----------|--------|
| Backend Developer | Diaria | Cada feature/bug fix |
| Frontend Developer | Diaria | Cada feature/bug fix |
| Testing | Diaria | Después de cada cambio |
| Code Review | Diaria | Antes de cada commit/PR |
| API Integration | Frecuente | Al integrar nuevos endpoints |
| Database | Frecuente | Al cambiar schema o optimizar |
| Security | Semanal | Antes de deploys importantes |
| Planning | Por Sprint | Al inicio de sprint o epic |
| Architecture | Por Feature | Features grandes o nuevos servicios |
| Performance | Mensual | O cuando hay problemas |
| CI/CD | Por Setup | Setup inicial o cambios en pipeline |
| Infrastructure | Por Setup | Setup inicial o cambios mayores |
| Monitoring | Por Setup | Setup inicial o nuevos servicios |
| Refactoring | Ad-hoc | Cuando se identifica código problemático |
| Documentation | Por Release | Actualizar docs con releases |

---

## 💡 Tips para Maximizar Efectividad

### 1. Contexto Primero
Siempre proporciona contexto completo usando los snippets. No improvises el contexto.

### 2. Iteración
Los agentes mejoran con iteración. Primera respuesta puede no ser perfecta, pide refinamientos.

### 3. Validación Humana
Siempre revisa el output. Los agentes son asistentes, no reemplazos.

### 4. Consistencia
Usa los mismos agentes para tareas similares para mantener consistencia en el código.

### 5. Aprendizaje
Los agentes aprenden del código existente. Mantén código de alta calidad.

### 6. Combinación
Usa múltiples agentes en secuencia para tareas complejas.

### 7. Especialización
Usa el agente correcto para cada tarea. No uses backend agent para frontend.

---

## 📚 Referencias Rápidas

- **Guía Completa**: [AGENTS_GUIDE.md](./AGENTS_GUIDE.md)
- **Quick Start**: [QUICKSTART_AGENTS.md](./QUICKSTART_AGENTS.md)
- **Arquitectura**: [ARCHITECTURE.md](./ARCHITECTURE.md)
- **Snippets**: `.vscode/beer-competition-agents.code-snippets`
- **Config**: `.vscode/settings.json`

---

## ✅ Checklist de Setup

- [ ] VS Code instalado (v1.85+)
- [ ] GitHub Copilot extensión instalada
- [ ] GitHub Copilot Chat extensión instalada
- [ ] Autenticado en GitHub Copilot
- [ ] Proyecto abierto en VS Code
- [ ] `.vscode/settings.json` existe
- [ ] `.vscode/beer-competition-agents.code-snippets` existe
- [ ] Probado al menos un snippet (ej: `agent-backend`)
- [ ] Leído QUICKSTART_AGENTS.md
- [ ] Leído ARCHITECTURE.md

---

**¿Listo para empezar?** Ve a [QUICKSTART_AGENTS.md](./QUICKSTART_AGENTS.md) para comenzar a usar los agentes.

---

*Última actualización: 2025-12-18*  
*Versión: 1.0*
