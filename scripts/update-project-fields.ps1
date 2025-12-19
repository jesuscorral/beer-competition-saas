# ============================================================================
# 📋 Beer Competition SaaS - Bulk Project Fields Update
# ============================================================================
# Purpose: Update all GitHub issues with Sprint, Epic, Complexity, Priority
#          and Agent fields based on issue labels and ISSUES_CREATION_STATUS.md
# 
# Prerequisites:
#   - GitHub CLI installed (gh)
#   - Authenticated: gh auth login
#   - Project ID: 9 (Beer competition)
# ============================================================================

param(
    [switch]$DryRun = $false
)

$ErrorActionPreference = "Stop"

# Configuration
$OWNER = "jesuscorral"
$REPO = "beer-competition-saas"
$PROJECT_NUMBER = 9

Write-Host "🚀 Beer Competition SaaS - Bulk Project Fields Update" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

if ($DryRun) {
    Write-Host "⚠️  DRY RUN MODE - No changes will be applied" -ForegroundColor Yellow
    Write-Host ""
}

# ============================================================================
# Issue-to-Field Mapping (based on ISSUES_CREATION_STATUS.md)
# ============================================================================

$issueMapping = @{
    # Sprint 0 - Infrastructure
    "6"  = @{ Sprint = "Sprint 0"; Epic = "Infrastructure"; Complexity = "M"; Priority = "P0"; Agent = "DevOps" }
    "2"  = @{ Sprint = "Sprint 0"; Epic = "Infrastructure"; Complexity = "L"; Priority = "P0"; Agent = "DevOps" }
    "7"  = @{ Sprint = "Sprint 0"; Epic = "Infrastructure"; Complexity = "M"; Priority = "P0"; Agent = "Backend" }
    "8"  = @{ Sprint = "Sprint 0"; Epic = "Infrastructure"; Complexity = "M"; Priority = "P0"; Agent = "DevOps" }
    "13" = @{ Sprint = "Sprint 0"; Epic = "Infrastructure"; Complexity = "S"; Priority = "P0"; Agent = "Backend" }
    "11" = @{ Sprint = "Sprint 0"; Epic = "Observability"; Complexity = "S"; Priority = "P0"; Agent = "Backend" }
    "12" = @{ Sprint = "Sprint 0"; Epic = "Observability"; Complexity = "S"; Priority = "P0"; Agent = "Backend" }
    
    # Sprint 1 - Authentication & Competitions
    "3"  = @{ Sprint = "Sprint 1"; Epic = "Authentication"; Complexity = "L"; Priority = "P0"; Agent = "Backend" }
    "9"  = @{ Sprint = "Sprint 1"; Epic = "Authentication"; Complexity = "M"; Priority = "P0"; Agent = "Backend" }
    "10" = @{ Sprint = "Sprint 1"; Epic = "Authentication"; Complexity = "M"; Priority = "P0"; Agent = "Backend" }
    "5"  = @{ Sprint = "Sprint 1"; Epic = "Competitions"; Complexity = "M"; Priority = "P0"; Agent = "Backend" }
    "4"  = @{ Sprint = "Sprint 1"; Epic = "Competitions"; Complexity = "L"; Priority = "P0"; Agent = "Backend" }
    "15" = @{ Sprint = "Sprint 1"; Epic = "Competitions"; Complexity = "M"; Priority = "P0"; Agent = "Backend" }
    "14" = @{ Sprint = "Sprint 1"; Epic = "Competitions"; Complexity = "S"; Priority = "P0"; Agent = "Backend" }
    "44" = @{ Sprint = "Sprint 1"; Epic = "Authentication"; Complexity = "M"; Priority = "P1"; Agent = "Frontend" }
    
    # Sprint 1-2 - Frontend Foundation
    # Note: UI-001 and UI-002 might have different issue numbers if created separately
    # Adjust these numbers based on actual GitHub issue IDs
    
    # Sprint 2 - Entry Management
    "16" = @{ Sprint = "Sprint 2"; Epic = "Entries"; Complexity = "M"; Priority = "P0"; Agent = "Backend" }
    "17" = @{ Sprint = "Sprint 2"; Epic = "Entries"; Complexity = "M"; Priority = "P0"; Agent = "Backend" }
    "18" = @{ Sprint = "Sprint 2"; Epic = "Entries"; Complexity = "M"; Priority = "P0"; Agent = "Backend" }
    "38" = @{ Sprint = "Sprint 2"; Epic = "UI/Frontend"; Complexity = "M"; Priority = "P0"; Agent = "Frontend" }
    "39" = @{ Sprint = "Sprint 2"; Epic = "UI/Frontend"; Complexity = "M"; Priority = "P0"; Agent = "Frontend" }
    "42" = @{ Sprint = "Sprint 2"; Epic = "Competitions"; Complexity = "M"; Priority = "P1"; Agent = "Backend" }
    
    # Sprint 3 - Flight Management
    "20" = @{ Sprint = "Sprint 3"; Epic = "Flights"; Complexity = "L"; Priority = "P0"; Agent = "Backend" }
    "19" = @{ Sprint = "Sprint 3"; Epic = "Flights"; Complexity = "M"; Priority = "P0"; Agent = "Backend" }
    "21" = @{ Sprint = "Sprint 3"; Epic = "Flights"; Complexity = "M"; Priority = "P0"; Agent = "Backend" }
    "40" = @{ Sprint = "Sprint 3"; Epic = "UI/Frontend"; Complexity = "M"; Priority = "P0"; Agent = "Frontend" }
    "22" = @{ Sprint = "Sprint 3"; Epic = "Flights"; Complexity = "S"; Priority = "P1"; Agent = "Backend" }
    
    # Sprint 4 - Scoring & Offline PWA
    "25" = @{ Sprint = "Sprint 4"; Epic = "Scoring"; Complexity = "L"; Priority = "P0"; Agent = "Frontend" }
    "26" = @{ Sprint = "Sprint 4"; Epic = "Scoring"; Complexity = "L"; Priority = "P0"; Agent = "Backend" }
    "27" = @{ Sprint = "Sprint 4"; Epic = "Scoring"; Complexity = "M"; Priority = "P0"; Agent = "Frontend" }
    "29" = @{ Sprint = "Sprint 4"; Epic = "Scoring"; Complexity = "S"; Priority = "P1"; Agent = "Frontend" }
    
    # Sprint 5 - Best of Show
    "31" = @{ Sprint = "Sprint 5"; Epic = "Best of Show"; Complexity = "M"; Priority = "P0"; Agent = "Backend" }
    "32" = @{ Sprint = "Sprint 5"; Epic = "Best of Show"; Complexity = "S"; Priority = "P0"; Agent = "Backend" }
    "33" = @{ Sprint = "Sprint 5"; Epic = "Best of Show"; Complexity = "M"; Priority = "P0"; Agent = "Frontend" }
    "34" = @{ Sprint = "Sprint 5"; Epic = "Best of Show"; Complexity = "S"; Priority = "P0"; Agent = "Backend" }
    "28" = @{ Sprint = "Sprint 5"; Epic = "Scoring"; Complexity = "M"; Priority = "P1"; Agent = "Backend" }
    
    # Sprint 6 - Results & Polish
    "30" = @{ Sprint = "Sprint 6"; Epic = "Scoring"; Complexity = "M"; Priority = "P1"; Agent = "Backend" }
    "35" = @{ Sprint = "Sprint 6"; Epic = "Best of Show"; Complexity = "M"; Priority = "P1"; Agent = "Backend" }
    "36" = @{ Sprint = "Sprint 6"; Epic = "Best of Show"; Complexity = "S"; Priority = "P1"; Agent = "Backend" }
    "37" = @{ Sprint = "Sprint 6"; Epic = "Best of Show"; Complexity = "M"; Priority = "P1"; Agent = "Backend" }
    "41" = @{ Sprint = "Sprint 6"; Epic = "UI/Frontend"; Complexity = "M"; Priority = "P1"; Agent = "Frontend" }
    
    # Post-MVP
    "23" = @{ Sprint = "Post-MVP"; Epic = "Flights"; Complexity = "S"; Priority = "P1"; Agent = "Backend" }
    "24" = @{ Sprint = "Post-MVP"; Epic = "Flights"; Complexity = "M"; Priority = "P1"; Agent = "Backend" }
    "43" = @{ Sprint = "Post-MVP"; Epic = "Competitions"; Complexity = "S"; Priority = "P1"; Agent = "Backend" }
    "45" = @{ Sprint = "Post-MVP"; Epic = "Authentication"; Complexity = "M"; Priority = "P1"; Agent = "Backend" }
    "46" = @{ Sprint = "Post-MVP"; Epic = "Observability"; Complexity = "L"; Priority = "P1"; Agent = "DevOps" }
}

# ============================================================================
# Get Project Details
# ============================================================================

Write-Host "📊 Fetching project details..." -ForegroundColor Cyan

$projectQuery = @"
query {
  user(login: "$OWNER") {
    projectV2(number: $PROJECT_NUMBER) {
      id
      title
      fields(first: 20) {
        nodes {
          ... on ProjectV2Field {
            id
            name
          }
          ... on ProjectV2SingleSelectField {
            id
            name
            options {
              id
              name
            }
          }
        }
      }
    }
  }
}
"@

$projectData = gh api graphql -f query=$projectQuery | ConvertFrom-Json
$project = $projectData.data.user.projectV2
$projectId = $project.id

Write-Host "✅ Project: $($project.title) (ID: $projectId)" -ForegroundColor Green
Write-Host ""

# Build field lookups
$fieldLookup = @{}
$optionLookup = @{}

foreach ($field in $project.fields.nodes) {
    $fieldLookup[$field.name] = $field.id
    
    if ($field.options) {
        $optionLookup[$field.name] = @{}
        foreach ($option in $field.options) {
            $optionLookup[$field.name][$option.name] = $option.id
        }
    }
}

Write-Host "📋 Available Fields:" -ForegroundColor Cyan
$fieldLookup.Keys | ForEach-Object { Write-Host "  - $_" -ForegroundColor Gray }
Write-Host ""

# ============================================================================
# Update Issues
# ============================================================================

$totalIssues = $issueMapping.Count
$currentIndex = 0
$successCount = 0
$errorCount = 0

Write-Host "🔄 Processing $totalIssues issues..." -ForegroundColor Cyan
Write-Host ""

foreach ($issueNumber in $issueMapping.Keys | Sort-Object { [int]$_ }) {
    $currentIndex++
    $fields = $issueMapping[$issueNumber]
    
    Write-Host "[$currentIndex/$totalIssues] Issue #$issueNumber" -ForegroundColor Yellow
    Write-Host "  📌 Sprint: $($fields.Sprint)" -ForegroundColor Gray
    Write-Host "  🏗️  Epic: $($fields.Epic)" -ForegroundColor Gray
    Write-Host "  📊 Complexity: $($fields.Complexity)" -ForegroundColor Gray
    Write-Host "  🔥 Priority: $($fields.Priority)" -ForegroundColor Gray
    Write-Host "  👤 Agent: $($fields.Agent)" -ForegroundColor Gray
    
    if ($DryRun) {
        Write-Host "  ⏭️  Skipping (dry run)" -ForegroundColor DarkGray
        $successCount++
        Write-Host ""
        continue
    }
    
    try {
        # Get project item ID for this issue
        $itemQuery = @"
query {
  repository(owner: "$OWNER", name: "$REPO") {
    issue(number: $issueNumber) {
      projectItems(first: 10) {
        nodes {
          id
          project {
            id
          }
        }
      }
    }
  }
}
"@
        
        $itemData = gh api graphql -f query=$itemQuery | ConvertFrom-Json
        $projectItem = $itemData.data.repository.issue.projectItems.nodes | Where-Object { $_.project.id -eq $projectId }
        
        if (-not $projectItem) {
            Write-Host "  ⚠️  Issue not in project, skipping..." -ForegroundColor Yellow
            $errorCount++
            Write-Host ""
            continue
        }
        
        $itemId = $projectItem.id
        
        # Update each field
        foreach ($fieldName in @("Sprint", "Epic", "Complexity", "Priority", "Agent")) {
            $fieldId = $fieldLookup[$fieldName]
            $fieldValue = $fields[$fieldName]
            $optionId = $optionLookup[$fieldName][$fieldValue]
            
            if (-not $fieldId -or -not $optionId) {
                Write-Host "  ⚠️  Warning: Field '$fieldName' or option '$fieldValue' not found" -ForegroundColor Yellow
                continue
            }
            
            $updateMutation = @"
mutation {
  updateProjectV2ItemFieldValue(input: {
    projectId: "$projectId"
    itemId: "$itemId"
    fieldId: "$fieldId"
    value: {
      singleSelectOptionId: "$optionId"
    }
  }) {
    projectV2Item {
      id
    }
  }
}
"@
            
            gh api graphql -f query=$updateMutation | Out-Null
        }
        
        Write-Host "  ✅ Updated successfully" -ForegroundColor Green
        $successCount++
        
        # Rate limiting protection
        Start-Sleep -Milliseconds 500
        
    } catch {
        Write-Host "  ❌ Error: $_" -ForegroundColor Red
        $errorCount++
    }
    
    Write-Host ""
}

# ============================================================================
# Summary
# ============================================================================

Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📊 Update Summary" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Total issues processed: $totalIssues" -ForegroundColor White
Write-Host "✅ Successful updates: $successCount" -ForegroundColor Green
Write-Host "❌ Errors: $errorCount" -ForegroundColor Red
Write-Host ""

if ($DryRun) {
    Write-Host "⚠️  This was a DRY RUN - no changes were applied" -ForegroundColor Yellow
    Write-Host "Run without -DryRun flag to apply changes" -ForegroundColor Yellow
} else {
    Write-Host "✅ All fields updated successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🔗 View project: https://github.com/users/$OWNER/projects/$PROJECT_NUMBER" -ForegroundColor Cyan
}

Write-Host ""
