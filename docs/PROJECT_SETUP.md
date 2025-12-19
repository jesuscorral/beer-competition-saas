# 📋 GitHub Project Configuration Guide

## ✅ Estado Actual

Tu proyecto "Beer competition" ya tiene:

### Campos Personalizados Configurados:
- ✅ **Status**: Backlog → Ready → In Progress → In Review → Done
- ✅ **Priority**: P0, P1, P2
- ✅ **Sprint**: Sprint 0, Sprint 1, Sprint 2, Sprint 3, Sprint 4, Sprint 5, Sprint 6, Post-MVP
- ✅ **Epic**: Infrastructure, Competitions, Entries, Flights, Scoring, Best of Show, Authentication, UI/Frontend, Observability
- ✅ **Complexity**: S (Small), M (Medium), L (Large)
- ✅ **Agent**: Backend, Frontend, DevOps, QA, Data Science, Product Owner
- ✅ **Size**: (campo existente)
- ✅ **Estimate**: (campo existente)
- ✅ **Start date**: (campo existente)
- ✅ **Target date**: (campo existente)

### Issues Importados:
- ✅ **45 issues** (#2-#46) añadidos al proyecto

---

## 🎯 Configurar Vistas Personalizadas (Manual)

### Paso 1: Acceder al Proyecto
1. Ve a: https://github.com/users/jesuscorral/projects/9
2. Click en la pestaña del proyecto

### Paso 2: Crear Vistas

#### Vista 1: 🔥 Priority P0 (Critical)
1. Click en el botón **"+ New view"** (esquina superior derecha)
2. Nombre: `🔥 Priority P0 (Critical)`
3. Layout: **Board** (Tablero)
4. Group by: **Status**
5. **Filtros**:
   - Click en el icono de filtro (⚡)
   - Añadir: `priority:P0`
6. **Ordenar**: Por **Priority** (descendente)

#### Vista 2: ⚡ Priority P1 (High)
1. Click en **"+ New view"**
2. Nombre: `⚡ Priority P1 (High)`
3. Layout: **Board**
4. Group by: **Status**
5. **Filtros**: `priority:P1`
6. **Ordenar**: Por **Complexity** (descendente)

#### Vista 3: 🗺️ Roadmap (Timeline)
1. Click en **"+ New view"**
2. Nombre: `🗺️ Roadmap`
3. Layout: **Roadmap** (Vista de línea de tiempo)
4. **Configuración**:
   - Start date field: **Start date**
   - Target date field: **Target date**
5. Group by: **Sprint**
6. Color by: **Priority**

#### Vista 4: 📊 Sprint Board
1. Click en **"+ New view"**
2. Nombre: `📊 Sprint Board`
3. Layout: **Board**
4. Group by: **Status**
5. **Filtros**: `sprint:"Sprint 0"` (cambia el sprint según necesites)
6. **Ordenar**: Por **Priority** → **Complexity**

#### Vista 5: 🏗️ By Epic
1. Click en **"+ New view"**
2. Nombre: `🏗️ By Epic`
3. Layout: **Table**
4. Group by: **Epic**
5. Mostrar columnas: Title, Status, Priority, Complexity, Sprint, Agent
6. **Ordenar**: Por **Epic** → **Priority**

#### Vista 6: 👥 By Agent
1. Click en **"+ New view"**
2. Nombre: `👥 By Agent`
3. Layout: **Board**
4. Group by: **Agent**
5. **Ordenar**: Por **Priority** → **Sprint**

---

## 🤖 Automatizaciones Recomendadas

### Configurar Workflows Automáticos:

1. **Auto-move to "In Progress"** cuando se asigna:
   ```
   When: Issue assigned
   Then: Set Status → "In Progress"
   ```

2. **Auto-move to "In Review"** cuando se abre PR:
   ```
   When: Pull request opened
   Then: Set Status → "In Review"
   ```

3. **Auto-move to "Done"** cuando se cierra issue:
   ```
   When: Issue closed
   Then: Set Status → "Done"
   ```

4. **Auto-set Sprint** cuando se añade label:
   ```
   When: Label "sprint-0" added
   Then: Set Sprint → "Sprint 0"
   ```

### Cómo configurar:
1. Ve al proyecto → Settings (⚙️)
2. Click en **Workflows** (en el menú lateral)
3. Click **+ Create workflow**
4. Selecciona el trigger y la acción

---

## 📋 Asignación Automática de Campos

Para asignar campos masivamente basándote en labels existentes, usa este script:

```powershell
# Asignar Sprint basado en labels
$issues = gh api "repos/jesuscorral/beer-competition-saas/issues?state=open&per_page=100" | ConvertFrom-Json

foreach ($issue in $issues) {
    $issueNumber = $issue.number
    $labels = $issue.labels | ForEach-Object { $_.name }
    
    # Mapeo de labels a campos
    $sprint = ""
    $priority = ""
    $complexity = ""
    $epic = ""
    
    # Sprint
    if ($labels -contains "sprint-0") { $sprint = "Sprint 0" }
    elseif ($labels -contains "sprint-1") { $sprint = "Sprint 1" }
    elseif ($labels -contains "sprint-2") { $sprint = "Sprint 2" }
    elseif ($labels -contains "sprint-3") { $sprint = "Sprint 3" }
    elseif ($labels -contains "sprint-4") { $sprint = "Sprint 4" }
    elseif ($labels -contains "sprint-5") { $sprint = "Sprint 5" }
    elseif ($labels -contains "sprint-6") { $sprint = "Sprint 6" }
    elseif ($labels -contains "post-mvp") { $sprint = "Post-MVP" }
    
    # Priority
    if ($labels -contains "priority:P0" -or $labels -contains "P0") { $priority = "P0" }
    elseif ($labels -contains "priority:P1" -or $labels -contains "P1") { $priority = "P1" }
    
    # Complexity
    if ($labels -contains "complexity:S") { $complexity = "S" }
    elseif ($labels -contains "complexity:M") { $complexity = "M" }
    elseif ($labels -contains "complexity:L") { $complexity = "L" }
    
    # Epic
    if ($labels -contains "epic:infrastructure") { $epic = "Infrastructure" }
    elseif ($labels -contains "epic:competitions") { $epic = "Competitions" }
    elseif ($labels -contains "epic:entries") { $epic = "Entries" }
    elseif ($labels -contains "epic:flights") { $epic = "Flights" }
    elseif ($labels -contains "epic:scoring") { $epic = "Scoring" }
    elseif ($labels -contains "epic:best-of-show") { $epic = "Best of Show" }
    elseif ($labels -contains "epic:authentication") { $epic = "Authentication" }
    elseif ($labels -contains "epic:observability") { $epic = "Observability" }
    
    Write-Host "Issue #$issueNumber → Sprint: $sprint, Priority: $priority, Complexity: $complexity, Epic: $epic"
}
```

---

## 🎨 Personalización Adicional

### Colores Recomendados:

**Priority:**
- 🔴 P0: Rojo (#d73a4a)
- 🟠 P1: Naranja (#f97316)
- 🟡 P2: Amarillo (#fbbf24)

**Status:**
- 📋 Backlog: Gris (#6b7280)
- 📝 Ready: Azul claro (#60a5fa)
- 🚧 In Progress: Amarillo (#fbbf24)
- 👀 In Review: Morado (#a855f7)
- ✅ Done: Verde (#10b981)

**Epic (por color):**
- 🏗️ Infrastructure: Gris oscuro
- 🏆 Competitions: Dorado
- 📦 Entries: Verde
- ✈️ Flights: Azul
- 📊 Scoring: Púrpura
- 🏅 Best of Show: Oro
- 🔐 Authentication: Rojo
- 📱 UI/Frontend: Rosa
- 📡 Observability: Cyan

---

## 📊 Métricas y KPIs Recomendados

### Insights a Crear:

1. **Velocity Chart**:
   - Mide cuántos issues completas por sprint
   - Configura en: Project → Insights → Create chart

2. **Burndown Chart**:
   - Rastrea progreso hacia MVP
   - Agrupa por Sprint

3. **Cycle Time**:
   - Tiempo promedio desde "Ready" → "Done"
   - Identifica cuellos de botella

4. **WIP Limits**:
   - Máximo 5 issues "In Progress" simultáneamente
   - Previene sobrecarga

---

## 🚀 Próximos Pasos

1. ✅ **Campos configurados** (hecho automáticamente)
2. ⏳ **Crear vistas** (manual - sigue la guía arriba)
3. ⏳ **Configurar workflows** (automático - opcional)
4. ⏳ **Asignar campos** (ejecuta script PowerShell)
5. ⏳ **Crear insights** (métricas y reportes)

---

## 🔗 Enlaces Útiles

- **Proyecto**: https://github.com/users/jesuscorral/projects/9
- **Repositorio**: https://github.com/jesuscorral/beer-competition-saas
- **Documentación GitHub Projects**: https://docs.github.com/en/issues/planning-and-tracking-with-projects

---

## ❓ Comandos Útiles

```bash
# Ver todos los issues en el proyecto
gh project item-list 9 --owner jesuscorral

# Añadir issue al proyecto
gh project item-add 9 --owner jesuscorral --url https://github.com/jesuscorral/beer-competition-saas/issues/N

# Ver campos del proyecto
gh project field-list 9 --owner jesuscorral

# Ver información del proyecto
gh project view 9 --owner jesuscorral
```

---

**¡Tu proyecto está listo para empezar a trabajar de forma eficiente! 🎉**
