# Guía Rápida: Uso de Agentes de IA en VS Code

Esta guía te ayudará a configurar y usar los agentes de IA especializados para el desarrollo del sistema de competiciones de cerveza BJCP.

## 📋 Tabla de Contenidos

- [Requisitos Previos](#requisitos-previos)
- [Instalación y Configuración](#instalación-y-configuración)
- [Uso de Agentes](#uso-de-agentes)
- [Ejemplos Prácticos](#ejemplos-prácticos)
- [Mejores Prácticas](#mejores-prácticas)
- [Solución de Problemas](#solución-de-problemas)

## 🔧 Requisitos Previos

1. **Visual Studio Code** (versión 1.85 o superior)
2. **GitHub Copilot** (suscripción activa)
3. **Extensiones necesarias:**
   - GitHub Copilot
   - GitHub Copilot Chat

## 🚀 Instalación y Configuración

### Paso 1: Instalar Extensiones

1. Abre Visual Studio Code
2. Ve a la vista de Extensiones (`Ctrl+Shift+X` o `Cmd+Shift+X` en Mac)
3. Busca "GitHub Copilot" e instala ambas extensiones:
   - **GitHub Copilot** (autocompletado de código)
   - **GitHub Copilot Chat** (chat interactivo)
4. Reinicia VS Code si es necesario

### Paso 2: Autenticación

1. Haz clic en el icono de GitHub Copilot en la barra de estado (esquina inferior derecha)
2. Selecciona "Sign in to GitHub"
3. Autoriza la extensión en tu navegador
4. Vuelve a VS Code y confirma la autenticación

### Paso 3: Verificar Configuración

1. Abre este repositorio en VS Code
2. Los archivos de configuración ya están incluidos:
   - `.vscode/settings.json` - Configuración de Copilot con contexto del proyecto
   - `.vscode/beer-competition-agents.code-snippets` - Snippets de agentes
3. La configuración se aplicará automáticamente

## 🤖 Uso de Agentes

### Método 1: Snippets (Recomendado para Principiantes)

Los snippets te permiten invocar agentes rápidamente escribiendo un atajo:

1. En cualquier archivo, escribe el prefix del agente (ejemplo: `agent-backend`)
2. Presiona `Tab` o `Enter` para expandir el snippet
3. El snippet te mostrará el contexto del agente
4. Completa tu tarea específica al final del snippet (donde dice `Tarea: `)

**Snippets disponibles:**

| Prefix | Agente | Uso |
|--------|--------|-----|
| `agent-backend` | Backend Developer | Desarrollo de APIs y servicios |
| `agent-frontend` | Frontend Developer | Componentes React y UI |
| `agent-api` | API Integration | Integración frontend-backend |
| `agent-database` | Database Developer | Schema y queries SQL |
| `agent-testing` | Testing Agent | Tests unitarios e integración |
| `agent-security` | Security Testing | Revisión de seguridad |
| `agent-review` | Code Review | Revisión de código |
| `agent-architect` | System Architect | Decisiones arquitectónicas |
| `agent-cicd` | CI/CD Agent | Pipelines y automatización |
| `agent-infra` | Infrastructure | Docker, Kubernetes, setup |
| `agent-datamodel` | Data Modeling | Diseño de schemas |
| `agent-performance` | Performance | Optimización |
| `agent-docs-tech` | Technical Docs | Documentación técnica |
| `agent-monitoring` | Monitoring | Logs, métricas, alertas |
| `agent-refactor` | Refactoring | Mejora de código |
| `agent-planning` | Feature Planning | Planificación de features |

### Método 2: GitHub Copilot Chat (Recomendado para Usuarios Avanzados)

1. Abre el panel de Copilot Chat (`Ctrl+Alt+I` o mediante el icono de chat)
2. Usa el comando `@workspace` para dar contexto del proyecto
3. Invoca un agente manualmente con su descripción

**Ejemplo:**
```
@workspace Actúa como Backend Developer Agent especializado en Node.js/TypeScript.
Stack: Node.js 18+, Express, Prisma, PostgreSQL, Redis.
Proyecto: Sistema SaaS de competiciones de cerveza BJCP con multi-tenancy.

Necesito crear el endpoint POST /api/v1/competitions que:
1. Permita crear una nueva competición
2. Valide que el usuario tenga rol de organizer o admin
3. Asegure isolation por organization_id
4. Valide los campos requeridos con Zod
5. Retorne 201 con el objeto creado
```

### Método 3: Inline Chat

1. Selecciona código en el editor
2. Presiona `Ctrl+I` (o `Cmd+I` en Mac)
3. Escribe tu instrucción con el contexto del agente
4. Copilot sugerirá cambios directamente en el código

## 📚 Ejemplos Prácticos

### Ejemplo 1: Crear un Endpoint de API

**Paso 1: Invocar el agente**
```
# En un archivo .ts nuevo, escribe:
agent-backend [Tab]
```

**Paso 2: Completar la tarea**
```
Tarea: Crear el endpoint POST /api/v1/entries/{entryId}/scores
que permita a un juez enviar un score para una entry.

Requisitos:
- Validar que el usuario sea un juez asignado a esa entry
- Validar scores según reglas BJCP (aroma: 0-50, appearance: 0-3, flavor: 0-50, mouthfeel: 0-10)
- Calcular total_score automáticamente
- Retornar el score creado con 201
```

**Paso 3: Copilot generará el código**

### Ejemplo 2: Crear un Componente React

**Paso 1:**
```
agent-frontend [Tab]
```

**Paso 2:**
```
Tarea: Crear componente ScoringForm que permita a un juez ingresar scores.

Features:
- Campos: aroma (slider 0-50), appearance (slider 0-3), flavor (0-50), mouthfeel (0-10), overall_impression (textarea)
- Validación con React Hook Form + Zod
- Mostrar total calculado en tiempo real
- Loading state mientras envía
- Error handling con snackbar
- Botón submit deshabilitado si hay errores
```

### Ejemplo 3: Crear Tests

**Paso 1:**
```
agent-testing [Tab]
```

**Paso 2:**
```
Tarea: Crear tests para el endpoint POST /api/v1/entries/{entryId}/scores

Tests a incluir:
1. ✅ Score creado correctamente con datos válidos
2. ✅ 401 si no está autenticado
3. ✅ 403 si no es un juez asignado
4. ✅ 400 si los scores están fuera de rango
5. ✅ 404 si la entry no existe
6. ✅ Total score calculado correctamente
7. ✅ Tenant isolation (no puede scorear entries de otra organización)
```

### Ejemplo 4: Revisión de Seguridad

**Paso 1:**
```
agent-security [Tab]
```

**Paso 2:**
```
Tarea: Revisar el siguiente endpoint por vulnerabilidades de seguridad:

[pega aquí el código del endpoint]

Verifica especialmente:
- Tenant isolation (organization_id en queries)
- Validación de inputs
- Autorización correcta
- SQL injection
- Rate limiting
```

### Ejemplo 5: Planificar una Feature

**Paso 1:**
```
agent-planning [Tab]
```

**Paso 2:**
```
Tarea: Descomponer la feature "Sistema de Notificaciones por Email" en user stories.

La feature debe permitir:
- Enviar email cuando una competición es publicada
- Enviar email cuando un score es recibido
- Enviar email con resultados finales
- Preferencias de notificaciones por usuario
```

## 💡 Mejores Prácticas

### 1. Contexto es Clave

Siempre proporciona contexto específico:
- ¿Qué estás tratando de lograr?
- ¿Qué restricciones existen?
- ¿Qué tecnologías específicas usar?
- ¿Hay algún patrón o estándar del proyecto a seguir?

### 2. Usa @workspace

En Copilot Chat, siempre inicia con `@workspace` para que Copilot tenga contexto del proyecto completo:
```
@workspace [tu pregunta]
```

### 3. Itera y Refina

Si la primera respuesta no es perfecta:
- Pide refinamientos específicos
- Da feedback sobre lo que falta
- Pide alternativas

Ejemplo:
```
"Agrega validación de inputs"
"Optimiza esta query"
"Agrega manejo de errores más robusto"
"Refactoriza para mejor legibilidad"
```

### 4. Combina Agentes

Para tareas complejas, usa múltiples agentes en secuencia:

```
1. agent-architect → Diseña la arquitectura
2. agent-backend → Implementa el backend
3. agent-frontend → Implementa el frontend
4. agent-testing → Crea los tests
5. agent-security → Revisa seguridad
6. agent-review → Revisa calidad del código
```

### 5. Valida Todo

Los agentes son asistentes, no reemplazos:
- Siempre revisa el código generado
- Ejecuta los tests
- Verifica que cumple los requisitos
- Asegura que sigue los estándares del proyecto

### 6. Un Agente a la Vez

No mezcles contextos de diferentes agentes en la misma conversación. Usa un agente específico para cada tarea.

## 🐛 Solución de Problemas

### Problema: Copilot no sugiere código

**Solución:**
1. Verifica que la extensión esté habilitada (icono en barra de estado)
2. Verifica tu suscripción en GitHub
3. Reinicia VS Code
4. Cierra sesión y vuelve a autenticarte

### Problema: Los snippets no se expanden

**Solución:**
1. Verifica que el archivo `.vscode/beer-competition-agents.code-snippets` exista
2. Asegúrate de escribir el prefix completo (ej: `agent-backend`)
3. Presiona `Tab` o `Enter` después de escribir el prefix
4. Si no funciona, recarga VS Code

### Problema: Copilot genera código incorrecto

**Solución:**
1. Proporciona más contexto específico
2. Usa el agente correcto para la tarea
3. Pide refinamientos iterativamente
4. Verifica que `.vscode/settings.json` esté correctamente configurado

### Problema: Copilot no entiende el contexto del proyecto

**Solución:**
1. Usa `@workspace` en Copilot Chat
2. Verifica que `.vscode/settings.json` tenga las instrucciones de código
3. Abre archivos relevantes (ARCHITECTURE.md) en el editor para dar contexto
4. Proporciona contexto explícito en tu prompt

### Problema: Respuestas genéricas

**Solución:**
1. Sé más específico en tu prompt
2. Menciona tecnologías específicas del stack
3. Incluye ejemplos de lo que esperas
4. Usa los snippets de agentes para dar contexto completo

## 📖 Recursos Adicionales

- **Documentación Completa**: Ver [AGENTS_GUIDE.md](./AGENTS_GUIDE.md)
- **Arquitectura del Sistema**: Ver [ARCHITECTURE.md](./ARCHITECTURE.md)
- **GitHub Copilot Docs**: https://docs.github.com/en/copilot
- **BJCP Style Guidelines**: https://www.bjcp.org/style/

## 🎯 Próximos Pasos

1. ✅ Configura VS Code con Copilot
2. ✅ Prueba algunos snippets de agentes
3. ✅ Lee la documentación de arquitectura (ARCHITECTURE.md)
4. ✅ Empieza con features simples usando agentes de planning
5. ✅ Itera y aprende qué agentes funcionan mejor para cada tarea

## 💬 Obtener Ayuda

Si tienes dudas sobre los agentes o cómo usarlos:
1. Revisa la documentación completa en [AGENTS_GUIDE.md](./AGENTS_GUIDE.md)
2. Experimenta con diferentes prompts y agentes
3. Usa `@workspace /help` en Copilot Chat para ayuda contextual

---

**¡Feliz Codificación con Agentes de IA!** 🎉🍺

*Última actualización: 2025-12-18*
