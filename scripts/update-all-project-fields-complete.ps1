#!/usr/bin/env pwsh
# Script para actualizar TODOS los campos del proyecto GitHub para todos los issues
# Incluye: Sprint, Epic, Priority, Complexity, Size, Agent, Estimate, Start date, Target date

$projectId = "PVT_kwHOAFw6AM4BK9-n"
$owner = "jesuscorral"

# Field IDs
$sprintFieldId = "PVTSSF_lAHOAFw6AM4BK9-nzg6rstQ"
$epicFieldId = "PVTSSF_lAHOAFw6AM4BK9-nzg6rstU"
$priorityFieldId = "PVTSSF_lAHOAFw6AM4BK9-nzg6rstg"
$complexityFieldId = "PVTSSF_lAHOAFw6AM4BK9-nzg6rstW"
$sizeFieldId = "PVTSSF_lAHOAFw6AM4BK9-nzg6rstY"
$agentFieldId = "PVTSSF_lAHOAFw6AM4BK9-nzg6rsta"
$estimateFieldId = "PVTF_lAHOAFw6AM4BK9-nzg6rstc"
$startDateFieldId = "PVTF_lAHOAFw6AM4BK9-nzg6rstg"
$targetDateFieldId = "PVTF_lAHOAFw6AM4BK9-nzg6rstk"

# Sprint IDs
$sprintIds = @{
    "Sprint 0" = "f75ad846"
    "Sprint 1" = "8cd07f41"
    "Sprint 2" = "18c5f923"
    "Sprint 3" = "16f79d13"
    "Sprint 4" = "19f9e39b"
    "Sprint 5" = "f784b110"
    "Sprint 6" = "8a038ec3"
    "Post-MVP" = "47fc9ee4"
}

# Epic IDs
$epicIds = @{
    "Infrastructure" = "f75ad846"
    "Competitions" = "8cd07f41"
    "Entries" = "18c5f923"
    "Flights" = "16f79d13"
    "Scoring" = "19f9e39b"
    "Best of Show" = "f784b110"
    "Authentication" = "8a038ec3"
    "UI/Frontend" = "47fc9ee4"
    "Observability" = "f784b110"
}

# Priority IDs
$priorityIds = @{
    "P0" = "79628723"
    "P1" = "0a877460"
    "P2" = "da944a9c"
}

# Complexity IDs
$complexityIds = @{
    "S" = "f75ad846"
    "M" = "47fc9ee4"
    "L" = "98236657"
}

# Size IDs
$sizeIds = @{
    "XS" = "6c6483d2"
    "S" = "f784b110"
    "M" = "7515a9f1"
    "L" = "817d0097"
    "XL" = "db339eb2"
}

# Agent IDs
$agentIds = @{
    "Backend" = "f75ad846"
    "Frontend" = "47fc9ee4"
    "DevOps" = "98236657"
    "QA" = "8cd07f41"
    "Data Science" = "18c5f923"
    "Product Owner" = "16f79d13"
}

# Configuración completa de issues con TODOS los campos
$issuesConfig = @(
    # Sprint 0: Infrastructure Setup (Semana del 6-12 enero 2025)
    @{ Number=2; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BRA"; Sprint="Sprint 0"; Epic="Infrastructure"; Priority="P0"; Complexity="M"; Size="M"; Agent="DevOps"; Estimate=3; StartDate="2025-01-06"; TargetDate="2025-01-12" }
    @{ Number=6; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BUI"; Sprint="Sprint 0"; Epic="Infrastructure"; Priority="P0"; Complexity="L"; Size="L"; Agent="DevOps"; Estimate=5; StartDate="2025-01-06"; TargetDate="2025-01-12" }
    @{ Number=7; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BUY"; Sprint="Sprint 0"; Epic="Infrastructure"; Priority="P0"; Complexity="L"; Size="L"; Agent="Backend"; Estimate=5; StartDate="2025-01-06"; TargetDate="2025-01-12" }
    @{ Number=8; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BU4"; Sprint="Sprint 0"; Epic="Infrastructure"; Priority="P0"; Complexity="M"; Size="M"; Agent="DevOps"; Estimate=3; StartDate="2025-01-06"; TargetDate="2025-01-12" }
    @{ Number=13; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BYU"; Sprint="Sprint 0"; Epic="Infrastructure"; Priority="P1"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-01-06"; TargetDate="2025-01-12" }

    # Sprint 1: Auth & Competition Core (Semana del 13-19 enero 2025)
    @{ Number=3; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BSs"; Sprint="Sprint 1"; Epic="Authentication"; Priority="P0"; Complexity="L"; Size="L"; Agent="Backend"; Estimate=5; StartDate="2025-01-13"; TargetDate="2025-01-19" }
    @{ Number=9; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BVg"; Sprint="Sprint 1"; Epic="Authentication"; Priority="P0"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-01-13"; TargetDate="2025-01-19" }
    @{ Number=10; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BWM"; Sprint="Sprint 1"; Epic="Authentication"; Priority="P0"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-01-13"; TargetDate="2025-01-19" }
    @{ Number=5; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BTs"; Sprint="Sprint 1"; Epic="Competitions"; Priority="P0"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-01-13"; TargetDate="2025-01-19" }
    @{ Number=4; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BTA"; Sprint="Sprint 1"; Epic="Competitions"; Priority="P0"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-01-13"; TargetDate="2025-01-19" }
    @{ Number=14; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BYs"; Sprint="Sprint 1"; Epic="Competitions"; Priority="P1"; Complexity="S"; Size="S"; Agent="Backend"; Estimate=2; StartDate="2025-01-13"; TargetDate="2025-01-19" }
    @{ Number=15; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BZI"; Sprint="Sprint 1"; Epic="Competitions"; Priority="P1"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-01-13"; TargetDate="2025-01-19" }
    @{ Number=11; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BXQ"; Sprint="Sprint 1"; Epic="Observability"; Priority="P1"; Complexity="M"; Size="M"; Agent="DevOps"; Estimate=3; StartDate="2025-01-13"; TargetDate="2025-01-19" }
    @{ Number=12; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BXw"; Sprint="Sprint 1"; Epic="Observability"; Priority="P1"; Complexity="S"; Size="S"; Agent="DevOps"; Estimate=2; StartDate="2025-01-13"; TargetDate="2025-01-19" }

    # Sprint 2: Entries (Semana del 20-26 enero 2025)
    @{ Number=16; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BZo"; Sprint="Sprint 2"; Epic="Entries"; Priority="P0"; Complexity="L"; Size="L"; Agent="Backend"; Estimate=5; StartDate="2025-01-20"; TargetDate="2025-01-26" }
    @{ Number=17; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BZ8"; Sprint="Sprint 2"; Epic="Entries"; Priority="P0"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-01-20"; TargetDate="2025-01-26" }
    @{ Number=18; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BaU"; Sprint="Sprint 2"; Epic="Entries"; Priority="P0"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-01-20"; TargetDate="2025-01-26" }
    @{ Number=38; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Bi4"; Sprint="Sprint 2"; Epic="UI/Frontend"; Priority="P1"; Complexity="M"; Size="M"; Agent="Frontend"; Estimate=3; StartDate="2025-01-20"; TargetDate="2025-01-26" }

    # Sprint 3: Flights (Semana del 27 enero - 2 febrero 2025)
    @{ Number=20; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Ba0"; Sprint="Sprint 3"; Epic="Flights"; Priority="P0"; Complexity="L"; Size="XL"; Agent="Backend"; Estimate=8; StartDate="2025-01-27"; TargetDate="2025-02-02" }
    @{ Number=19; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BaU"; Sprint="Sprint 3"; Epic="Flights"; Priority="P0"; Complexity="M"; Size="L"; Agent="Backend"; Estimate=5; StartDate="2025-01-27"; TargetDate="2025-02-02" }
    @{ Number=21; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Ba8"; Sprint="Sprint 3"; Epic="Flights"; Priority="P0"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-01-27"; TargetDate="2025-02-02" }
    @{ Number=24; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BcI"; Sprint="Sprint 3"; Epic="Flights"; Priority="P1"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-01-27"; TargetDate="2025-02-02" }
    @{ Number=22; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Bbc"; Sprint="Sprint 3"; Epic="Flights"; Priority="P1"; Complexity="S"; Size="S"; Agent="Backend"; Estimate=2; StartDate="2025-01-27"; TargetDate="2025-02-02" }
    @{ Number=23; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Bbw"; Sprint="Sprint 3"; Epic="Flights"; Priority="P2"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-01-27"; TargetDate="2025-02-02" }
    @{ Number=41; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BkY"; Sprint="Sprint 3"; Epic="UI/Frontend"; Priority="P1"; Complexity="M"; Size="M"; Agent="Frontend"; Estimate=3; StartDate="2025-01-27"; TargetDate="2025-02-02" }

    # Sprint 4: Scoring (Semana del 3-9 febrero 2025)
    @{ Number=25; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Bcc"; Sprint="Sprint 4"; Epic="Scoring"; Priority="P0"; Complexity="L"; Size="XL"; Agent="Frontend"; Estimate=8; StartDate="2025-02-03"; TargetDate="2025-02-09" }
    @{ Number=26; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BdA"; Sprint="Sprint 4"; Epic="Scoring"; Priority="P0"; Complexity="M"; Size="L"; Agent="Backend"; Estimate=5; StartDate="2025-02-03"; TargetDate="2025-02-09" }
    @{ Number=27; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Bdg"; Sprint="Sprint 4"; Epic="Scoring"; Priority="P0"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-02-03"; TargetDate="2025-02-09" }
    @{ Number=30; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Bew"; Sprint="Sprint 4"; Epic="Scoring"; Priority="P0"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-02-03"; TargetDate="2025-02-09" }
    @{ Number=28; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Bd4"; Sprint="Sprint 4"; Epic="Scoring"; Priority="P1"; Complexity="S"; Size="S"; Agent="Frontend"; Estimate=2; StartDate="2025-02-03"; TargetDate="2025-02-09" }
    @{ Number=29; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BeQ"; Sprint="Sprint 4"; Epic="Scoring"; Priority="P1"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-02-03"; TargetDate="2025-02-09" }
    @{ Number=39; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Bjw"; Sprint="Sprint 4"; Epic="UI/Frontend"; Priority="P1"; Complexity="M"; Size="M"; Agent="Frontend"; Estimate=3; StartDate="2025-02-03"; TargetDate="2025-02-09" }
    @{ Number=42; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Bk0"; Sprint="Sprint 4"; Epic="Competitions"; Priority="P1"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-02-03"; TargetDate="2025-02-09" }
    @{ Number=43; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Bk8"; Sprint="Sprint 4"; Epic="Competitions"; Priority="P1"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-02-03"; TargetDate="2025-02-09" }

    # Sprint 5: Best of Show (Semana del 10-16 febrero 2025)
    @{ Number=33; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Bf4"; Sprint="Sprint 5"; Epic="Best of Show"; Priority="P0"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-02-10"; TargetDate="2025-02-16" }
    @{ Number=34; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BgM"; Sprint="Sprint 5"; Epic="Best of Show"; Priority="P0"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-02-10"; TargetDate="2025-02-16" }
    @{ Number=32; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Bfg"; Sprint="Sprint 5"; Epic="Best of Show"; Priority="P0"; Complexity="M"; Size="M"; Agent="Frontend"; Estimate=3; StartDate="2025-02-10"; TargetDate="2025-02-16" }
    @{ Number=31; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BfM"; Sprint="Sprint 5"; Epic="Best of Show"; Priority="P0"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-02-10"; TargetDate="2025-02-16" }
    @{ Number=36; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BiA"; Sprint="Sprint 5"; Epic="Best of Show"; Priority="P1"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-02-10"; TargetDate="2025-02-16" }
    @{ Number=37; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Bic"; Sprint="Sprint 5"; Epic="Best of Show"; Priority="P1"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-02-10"; TargetDate="2025-02-16" }
    @{ Number=35; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Bhg"; Sprint="Sprint 5"; Epic="Best of Show"; Priority="P1"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-02-10"; TargetDate="2025-02-16" }
    @{ Number=40; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BjU"; Sprint="Sprint 5"; Epic="UI/Frontend"; Priority="P1"; Complexity="M"; Size="M"; Agent="Frontend"; Estimate=3; StartDate="2025-02-10"; TargetDate="2025-02-16" }

    # Sprint 6: Testing & Refinement (Semana del 17-23 febrero 2025)
    @{ Number=44; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0BlM"; Sprint="Sprint 6"; Epic="Observability"; Priority="P0"; Complexity="M"; Size="M"; Agent="DevOps"; Estimate=3; StartDate="2025-02-17"; TargetDate="2025-02-23" }
    @{ Number=45; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Blc"; Sprint="Sprint 6"; Epic="Authentication"; Priority="P1"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-02-17"; TargetDate="2025-02-23" }
    @{ Number=46; ItemId="PVTI_lAHOAFw6AM4BK9-nzgi0Bl4"; Sprint="Sprint 6"; Epic="Authentication"; Priority="P1"; Complexity="M"; Size="M"; Agent="Backend"; Estimate=3; StartDate="2025-02-17"; TargetDate="2025-02-23" }
)

function Update-SingleSelectField {
    param($itemId, $fieldId, $optionId, $fieldName)
    
    $query = @"
mutation {
  updateProjectV2ItemFieldValue(
    input: {
      projectId: "$projectId"
      itemId: "$itemId"
      fieldId: "$fieldId"
      value: { singleSelectOptionId: "$optionId" }
    }
  ) {
    projectV2Item { id }
  }
}
"@
    
    try {
        gh api graphql -f query=$query 2>$null | Out-Null
        Write-Host "  ✓ $fieldName actualizado" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ Error actualizando $fieldName" -ForegroundColor Red
    }
}

function Update-NumberField {
    param($itemId, $fieldId, $number, $fieldName)
    
    $query = @"
mutation {
  updateProjectV2ItemFieldValue(
    input: {
      projectId: "$projectId"
      itemId: "$itemId"
      fieldId: "$fieldId"
      value: { number: $number }
    }
  ) {
    projectV2Item { id }
  }
}
"@
    
    try {
        gh api graphql -f query=$query 2>$null | Out-Null
        Write-Host "  ✓ $fieldName actualizado a $number" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ Error actualizando $fieldName" -ForegroundColor Red
    }
}

function Update-DateField {
    param($itemId, $fieldId, $date, $fieldName)
    
    $query = @"
mutation {
  updateProjectV2ItemFieldValue(
    input: {
      projectId: "$projectId"
      itemId: "$itemId"
      fieldId: "$fieldId"
      value: { date: "$date" }
    }
  ) {
    projectV2Item { id }
  }
}
"@
    
    try {
        gh api graphql -f query=$query 2>$null | Out-Null
        Write-Host "  ✓ $fieldName actualizado a $date" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ Error actualizando $fieldName" -ForegroundColor Red
    }
}

Write-Host "🚀 Actualizando todos los campos de los issues..." -ForegroundColor Cyan
Write-Host ""

$total = $issuesConfig.Count
$current = 0

foreach ($issue in $issuesConfig) {
    $current++
    $percent = [math]::Round(($current / $total) * 100)
    
    Write-Host "[$current/$total] Issue #$($issue.Number) ($percent%)" -ForegroundColor Yellow
    
    # Update Sprint
    if ($issue.Sprint -and $sprintIds[$issue.Sprint]) {
        Update-SingleSelectField -itemId $issue.ItemId -fieldId $sprintFieldId -optionId $sprintIds[$issue.Sprint] -fieldName "Sprint ($($issue.Sprint))"
    }
    
    # Update Epic
    if ($issue.Epic -and $epicIds[$issue.Epic]) {
        Update-SingleSelectField -itemId $issue.ItemId -fieldId $epicFieldId -optionId $epicIds[$issue.Epic] -fieldName "Epic ($($issue.Epic))"
    }
    
    # Update Priority
    if ($issue.Priority -and $priorityIds[$issue.Priority]) {
        Update-SingleSelectField -itemId $issue.ItemId -fieldId $priorityFieldId -optionId $priorityIds[$issue.Priority] -fieldName "Priority ($($issue.Priority))"
    }
    
    # Update Complexity
    if ($issue.Complexity -and $complexityIds[$issue.Complexity]) {
        Update-SingleSelectField -itemId $issue.ItemId -fieldId $complexityFieldId -optionId $complexityIds[$issue.Complexity] -fieldName "Complexity ($($issue.Complexity))"
    }
    
    # Update Size
    if ($issue.Size -and $sizeIds[$issue.Size]) {
        Update-SingleSelectField -itemId $issue.ItemId -fieldId $sizeFieldId -optionId $sizeIds[$issue.Size] -fieldName "Size ($($issue.Size))"
    }
    
    # Update Agent
    if ($issue.Agent -and $agentIds[$issue.Agent]) {
        Update-SingleSelectField -itemId $issue.ItemId -fieldId $agentFieldId -optionId $agentIds[$issue.Agent] -fieldName "Agent ($($issue.Agent))"
    }
    
    # Update Estimate
    if ($issue.Estimate) {
        Update-NumberField -itemId $issue.ItemId -fieldId $estimateFieldId -number $issue.Estimate -fieldName "Estimate"
    }
    
    # Update Start Date
    if ($issue.StartDate) {
        Update-DateField -itemId $issue.ItemId -fieldId $startDateFieldId -date $issue.StartDate -fieldName "Start Date"
    }
    
    # Update Target Date
    if ($issue.TargetDate) {
        Update-DateField -itemId $issue.ItemId -fieldId $targetDateFieldId -date $issue.TargetDate -fieldName "Target Date"
    }
    
    Write-Host ""
}

Write-Host "✅ Actualización completada: $total issues procesados" -ForegroundColor Green
