# ============================================================================
# 📋 Beer Competition SaaS - Complete Project Fields Update
# ============================================================================
# Purpose: Update ALL GitHub issues with Sprint, Epic, Complexity, Priority,
#          Agent, Size, Estimate, Start Date, and Target Date
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

Write-Host "🚀 Beer Competition SaaS - Complete Project Fields Update" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

if ($DryRun) {
    Write-Host "⚠️  DRY RUN MODE - No changes will be applied" -ForegroundColor Yellow
    Write-Host ""
}

# ============================================================================
# Sprint Dates (from MVP_IMPLEMENTATION_GUIDE.md)
# ============================================================================

$sprintDates = @{
    "Sprint 0" = @{ Start = "2025-01-20"; End = "2025-02-03"; Days = 14 }   # 2 weeks
    "Sprint 1" = @{ Start = "2025-02-03"; End = "2025-02-24"; Days = 21 }   # 3 weeks
    "Sprint 2" = @{ Start = "2025-02-24"; End = "2025-03-17"; Days = 21 }   # 3 weeks
    "Sprint 3" = @{ Start = "2025-03-17"; End = "2025-04-07"; Days = 21 }   # 3 weeks
    "Sprint 4" = @{ Start = "2025-04-07"; End = "2025-05-05"; Days = 28 }   # 4 weeks
    "Sprint 5" = @{ Start = "2025-05-05"; End = "2025-05-19"; Days = 14 }   # 2 weeks
    "Sprint 6" = @{ Start = "2025-05-19"; End = "2025-06-02"; Days = 14 }   # 2 weeks
    "Post-MVP" = @{ Start = "2025-06-02"; End = "2025-12-31"; Days = 180 }  # TBD
}

# ============================================================================
# Complexity/Size to Estimate Mapping
# ============================================================================

$complexityToEstimate = @{
    "S"  = 2  # 2 days
    "M"  = 3  # 3-4 days
    "L"  = 5  # 5-7 days
    "XL" = 7  # 7+ days
}

$complexityToSize = @{
    "S"  = "S"
    "M"  = "M"
    "L"  = "L"
    "XL" = "XL"
}

# ============================================================================
# Complete Issue-to-Field Mapping
# ============================================================================

$issueMapping = @{
    # Sprint 0 - Infrastructure (2 weeks: 2025-01-20 to 2025-02-03)
    "6"  = @{ Sprint = "Sprint 0"; Epic = "Infrastructure"; Complexity = "M"; Priority = "P0"; Agent = "DevOps"; Estimate = 3 }
    "2"  = @{ Sprint = "Sprint 0"; Epic = "Infrastructure"; Complexity = "L"; Priority = "P0"; Agent = "DevOps"; Estimate = 5 }
    "7"  = @{ Sprint = "Sprint 0"; Epic = "Infrastructure"; Complexity = "M"; Priority = "P0"; Agent = "Backend"; Estimate = 3 }
    "8"  = @{ Sprint = "Sprint 0"; Epic = "Infrastructure"; Complexity = "M"; Priority = "P0"; Agent = "DevOps"; Estimate = 3 }
    "13" = @{ Sprint = "Sprint 0"; Epic = "Infrastructure"; Complexity = "S"; Priority = "P0"; Agent = "Backend"; Estimate = 2 }
    "11" = @{ Sprint = "Sprint 0"; Epic = "Observability"; Complexity = "S"; Priority = "P0"; Agent = "Backend"; Estimate = 2 }
    "12" = @{ Sprint = "Sprint 0"; Epic = "Observability"; Complexity = "S"; Priority = "P0"; Agent = "Backend"; Estimate = 2 }
    
    # Sprint 1 - Authentication & Competitions (3 weeks: 2025-02-03 to 2025-02-24)
    "3"  = @{ Sprint = "Sprint 1"; Epic = "Authentication"; Complexity = "L"; Priority = "P0"; Agent = "Backend"; Estimate = 5 }
    "9"  = @{ Sprint = "Sprint 1"; Epic = "Authentication"; Complexity = "M"; Priority = "P0"; Agent = "Backend"; Estimate = 3 }
    "10" = @{ Sprint = "Sprint 1"; Epic = "Authentication"; Complexity = "M"; Priority = "P0"; Agent = "Backend"; Estimate = 3 }
    "5"  = @{ Sprint = "Sprint 1"; Epic = "Competitions"; Complexity = "M"; Priority = "P0"; Agent = "Backend"; Estimate = 3 }
    "4"  = @{ Sprint = "Sprint 1"; Epic = "Competitions"; Complexity = "M"; Priority = "P0"; Agent = "Backend"; Estimate = 4 }
    "15" = @{ Sprint = "Sprint 1"; Epic = "Competitions"; Complexity = "M"; Priority = "P0"; Agent = "Backend"; Estimate = 3 }
    "14" = @{ Sprint = "Sprint 1"; Epic = "Competitions"; Complexity = "S"; Priority = "P0"; Agent = "Backend"; Estimate = 2 }
    "17" = @{ Sprint = "Sprint 1"; Epic = "UI/Frontend"; Complexity = "S"; Priority = "P0"; Agent = "Frontend"; Estimate = 2 }
    "18" = @{ Sprint = "Sprint 1"; Epic = "UI/Frontend"; Complexity = "M"; Priority = "P0"; Agent = "Frontend"; Estimate = 3 }
    "30" = @{ Sprint = "Sprint 1"; Epic = "Competitions"; Complexity = "S"; Priority = "P1"; Agent = "Backend"; Estimate = 2 }
    "31" = @{ Sprint = "Sprint 1"; Epic = "Competitions"; Complexity = "M"; Priority = "P1"; Agent = "Backend"; Estimate = 3 }
    
    # Sprint 2 - Entry Management (3 weeks: 2025-02-24 to 2025-03-17)
    "16" = @{ Sprint = "Sprint 2"; Epic = "Entries"; Complexity = "M"; Priority = "P0"; Agent = "Backend"; Estimate = 4 }
    "19" = @{ Sprint = "Sprint 2"; Epic = "Entries"; Complexity = "M"; Priority = "P0"; Agent = "Backend"; Estimate = 3 }
    "20" = @{ Sprint = "Sprint 2"; Epic = "Entries"; Complexity = "M"; Priority = "P0"; Agent = "Backend"; Estimate = 3 }
    "21" = @{ Sprint = "Sprint 2"; Epic = "UI/Frontend"; Complexity = "M"; Priority = "P0"; Agent = "Frontend"; Estimate = 3 }
    "22" = @{ Sprint = "Sprint 2"; Epic = "UI/Frontend"; Complexity = "S"; Priority = "P0"; Agent = "Frontend"; Estimate = 2 }
    
    # Sprint 3 - Flight Management (3 weeks: 2025-03-17 to 2025-04-07)
    "27" = @{ Sprint = "Sprint 3"; Epic = "Flights"; Complexity = "M"; Priority = "P0"; Agent = "Backend"; Estimate = 4 }
    "23" = @{ Sprint = "Sprint 3"; Epic = "Flights"; Complexity = "L"; Priority = "P0"; Agent = "Backend"; Estimate = 6 }
    "24" = @{ Sprint = "Sprint 3"; Epic = "Flights"; Complexity = "M"; Priority = "P0"; Agent = "Backend"; Estimate = 3 }
    "25" = @{ Sprint = "Sprint 3"; Epic = "Flights"; Complexity = "S"; Priority = "P1"; Agent = "Backend"; Estimate = 2 }
    "28" = @{ Sprint = "Sprint 3"; Epic = "Flights"; Complexity = "M"; Priority = "P1"; Agent = "Backend"; Estimate = 3 }
    "29" = @{ Sprint = "Sprint 3"; Epic = "Flights"; Complexity = "M"; Priority = "P1"; Agent = "Backend"; Estimate = 3 }
    
    # Sprint 4 - Offline PWA & Scoring (4 weeks: 2025-04-07 to 2025-05-05)
    "32" = @{ Sprint = "Sprint 4"; Epic = "Scoring"; Complexity = "L"; Priority = "P0"; Agent = "Frontend"; Estimate = 6 }
    "33" = @{ Sprint = "Sprint 4"; Epic = "Scoring"; Complexity = "M"; Priority = "P0"; Agent = "Backend"; Estimate = 4 }
    "34" = @{ Sprint = "Sprint 4"; Epic = "Scoring"; Complexity = "M"; Priority = "P0"; Agent = "Backend"; Estimate = 3 }
    "26" = @{ Sprint = "Sprint 4"; Epic = "UI/Frontend"; Complexity = "M"; Priority = "P0"; Agent = "Frontend"; Estimate = 3 }
    "35" = @{ Sprint = "Sprint 4"; Epic = "Scoring"; Complexity = "M"; Priority = "P1"; Agent = "Backend"; Estimate = 3 }
    "36" = @{ Sprint = "Sprint 4"; Epic = "Scoring"; Complexity = "S"; Priority = "P1"; Agent = "Backend"; Estimate = 2 }
    "37" = @{ Sprint = "Sprint 4"; Epic = "Scoring"; Complexity = "M"; Priority = "P1"; Agent = "Backend"; Estimate = 2 }
    
    # Sprint 5 - Best of Show (2 weeks: 2025-05-05 to 2025-05-19)
    "38" = @{ Sprint = "Sprint 5"; Epic = "Best of Show"; Complexity = "M"; Priority = "P0"; Agent = "Backend"; Estimate = 3 }
    "39" = @{ Sprint = "Sprint 5"; Epic = "Best of Show"; Complexity = "S"; Priority = "P0"; Agent = "Backend"; Estimate = 2 }
    "40" = @{ Sprint = "Sprint 5"; Epic = "Best of Show"; Complexity = "M"; Priority = "P0"; Agent = "Backend"; Estimate = 3 }
    "41" = @{ Sprint = "Sprint 5"; Epic = "Best of Show"; Complexity = "M"; Priority = "P0"; Agent = "Backend"; Estimate = 3 }
    "42" = @{ Sprint = "Sprint 5"; Epic = "Best of Show"; Complexity = "M"; Priority = "P1"; Agent = "Backend"; Estimate = 3 }
    "43" = @{ Sprint = "Sprint 5"; Epic = "Best of Show"; Complexity = "S"; Priority = "P1"; Agent = "Backend"; Estimate = 2 }
    "44" = @{ Sprint = "Sprint 5"; Epic = "Best of Show"; Complexity = "M"; Priority = "P1"; Agent = "Backend"; Estimate = 2 }
    
    # Sprint 6 - Results & Polish (2 weeks: 2025-05-19 to 2025-06-02)
    "45" = @{ Sprint = "Sprint 6"; Epic = "UI/Frontend"; Complexity = "M"; Priority = "P1"; Agent = "Frontend"; Estimate = 3 }
    "46" = @{ Sprint = "Sprint 6"; Epic = "Authentication"; Complexity = "M"; Priority = "P1"; Agent = "Backend"; Estimate = 3 }
    
    # Post-MVP
    # Add post-MVP issues here if needed
}

# ============================================================================
# Get Project Information
# ============================================================================

Write-Host "📊 Fetching project information..." -ForegroundColor Yellow

$query = @'
query {
  user(login: "jesuscorral") {
    projectV2(number: 9) {
      id
      fields(first: 20) {
        nodes {
          ... on ProjectV2Field {
            id
            name
            dataType
          }
          ... on ProjectV2SingleSelectField {
            id
            name
            dataType
            options {
              id
              name
            }
          }
        }
      }
      items(first: 100) {
        nodes {
          id
          content {
            ... on Issue {
              number
              title
            }
          }
        }
      }
    }
  }
}
'@

$projectInfo = gh api graphql -f query=$query | ConvertFrom-Json
$project = $projectInfo.data.user.projectV2

Write-Host "✓ Project ID: $($project.id)" -ForegroundColor Green
Write-Host "✓ Total items: $($project.items.nodes.Count)" -ForegroundColor Green
Write-Host ""

# ============================================================================
# Build Field Lookups
# ============================================================================

Write-Host "🔍 Building field lookups..." -ForegroundColor Yellow

$fields = @{}
foreach ($field in $project.fields.nodes) {
    $fields[$field.name] = @{
        Id = $field.id
        Type = $field.dataType
        Options = @{}
    }
    
    if ($field.options) {
        foreach ($option in $field.options) {
            $fields[$field.name].Options[$option.name] = $option.id
        }
    }
}

Write-Host "✓ Found fields:" -ForegroundColor Green
$fields.Keys | ForEach-Object { Write-Host "  - $_" -ForegroundColor Gray }
Write-Host ""

# ============================================================================
# Update Issues
# ============================================================================

Write-Host "🔄 Updating issues..." -ForegroundColor Yellow
Write-Host ""

$updated = 0
$skipped = 0
$errors = 0

foreach ($item in $project.items.nodes) {
    if (-not $item.content.number) {
        continue
    }
    
    $issueNumber = $item.content.number.ToString()
    $issueTitle = $item.content.title
    
    if (-not $issueMapping.ContainsKey($issueNumber)) {
        Write-Host "⊘ Issue #$issueNumber - No mapping found (skipping)" -ForegroundColor DarkGray
        $skipped++
        continue
    }
    
    $mapping = $issueMapping[$issueNumber]
    
    Write-Host "📝 Issue #$issueNumber - $issueTitle" -ForegroundColor Cyan
    
    try {
        # Get sprint dates
        $sprint = $mapping.Sprint
        $sprintInfo = $sprintDates[$sprint]
        $startDate = $sprintInfo.Start
        $endDate = $sprintInfo.End
        
        # Calculate Size from Complexity
        $size = $complexityToSize[$mapping.Complexity]
        
        # Get estimate
        $estimate = $mapping.Estimate
        
        Write-Host "  Sprint: $sprint" -ForegroundColor Gray
        Write-Host "  Epic: $($mapping.Epic)" -ForegroundColor Gray
        Write-Host "  Complexity: $($mapping.Complexity)" -ForegroundColor Gray
        Write-Host "  Size: $size" -ForegroundColor Gray
        Write-Host "  Priority: $($mapping.Priority)" -ForegroundColor Gray
        Write-Host "  Agent: $($mapping.Agent)" -ForegroundColor Gray
        Write-Host "  Estimate: $estimate days" -ForegroundColor Gray
        Write-Host "  Start Date: $startDate" -ForegroundColor Gray
        Write-Host "  Target Date: $endDate" -ForegroundColor Gray
        
        if (-not $DryRun) {
            # Update Sprint
            if ($fields["Sprint"].Options[$sprint]) {
                $sprintOptionId = $fields["Sprint"].Options[$sprint]
                gh api graphql -f query="mutation { updateProjectV2ItemFieldValue(input: {projectId: \`"$($project.id)\`", itemId: \`"$($item.id)\`", fieldId: \`"$($fields['Sprint'].Id)\`", value: {singleSelectOptionId: \`"$sprintOptionId\`"}}) { projectV2Item { id } } }" | Out-Null
            }
            
            # Update Epic
            if ($fields["Epic"].Options[$mapping.Epic]) {
                $epicOptionId = $fields["Epic"].Options[$mapping.Epic]
                gh api graphql -f query="mutation { updateProjectV2ItemFieldValue(input: {projectId: \`"$($project.id)\`", itemId: \`"$($item.id)\`", fieldId: \`"$($fields['Epic'].Id)\`", value: {singleSelectOptionId: \`"$epicOptionId\`"}}) { projectV2Item { id } } }" | Out-Null
            }
            
            # Update Complexity
            if ($fields["Complexity"].Options[$mapping.Complexity]) {
                $complexityOptionId = $fields["Complexity"].Options[$mapping.Complexity]
                gh api graphql -f query="mutation { updateProjectV2ItemFieldValue(input: {projectId: \`"$($project.id)\`", itemId: \`"$($item.id)\`", fieldId: \`"$($fields['Complexity'].Id)\`", value: {singleSelectOptionId: \`"$complexityOptionId\`"}}) { projectV2Item { id } } }" | Out-Null
            }
            
            # Update Size
            if ($fields["Size"].Options[$size]) {
                $sizeOptionId = $fields["Size"].Options[$size]
                gh api graphql -f query="mutation { updateProjectV2ItemFieldValue(input: {projectId: \`"$($project.id)\`", itemId: \`"$($item.id)\`", fieldId: \`"$($fields['Size'].Id)\`", value: {singleSelectOptionId: \`"$sizeOptionId\`"}}) { projectV2Item { id } } }" | Out-Null
            }
            
            # Update Priority
            if ($fields["Priority"].Options[$mapping.Priority]) {
                $priorityOptionId = $fields["Priority"].Options[$mapping.Priority]
                gh api graphql -f query="mutation { updateProjectV2ItemFieldValue(input: {projectId: \`"$($project.id)\`", itemId: \`"$($item.id)\`", fieldId: \`"$($fields['Priority'].Id)\`", value: {singleSelectOptionId: \`"$priorityOptionId\`"}}) { projectV2Item { id } } }" | Out-Null
            }
            
            # Update Agent
            if ($fields["Agent"].Options[$mapping.Agent]) {
                $agentOptionId = $fields["Agent"].Options[$mapping.Agent]
                gh api graphql -f query="mutation { updateProjectV2ItemFieldValue(input: {projectId: \`"$($project.id)\`", itemId: \`"$($item.id)\`", fieldId: \`"$($fields['Agent'].Id)\`", value: {singleSelectOptionId: \`"$agentOptionId\`"}}) { projectV2Item { id } } }" | Out-Null
            }
            
            # Update Estimate (Number field)
            gh api graphql -f query="mutation { updateProjectV2ItemFieldValue(input: {projectId: \`"$($project.id)\`", itemId: \`"$($item.id)\`", fieldId: \`"$($fields['Estimate'].Id)\`", value: {number: $estimate}}) { projectV2Item { id } } }" | Out-Null
            
            # Update Start Date
            gh api graphql -f query="mutation { updateProjectV2ItemFieldValue(input: {projectId: \`"$($project.id)\`", itemId: \`"$($item.id)\`", fieldId: \`"$($fields['Start date'].Id)\`", value: {date: \`"$startDate\`"}}) { projectV2Item { id } } }" | Out-Null
            
            # Update Target Date
            gh api graphql -f query="mutation { updateProjectV2ItemFieldValue(input: {projectId: \`"$($project.id)\`", itemId: \`"$($item.id)\`", fieldId: \`"$($fields['Target date'].Id)\`", value: {date: \`"$endDate\`"}}) { projectV2Item { id } } }" | Out-Null
        }
        
        Write-Host "  ✓ Updated successfully" -ForegroundColor Green
        $updated++
        
    } catch {
        Write-Host "  ✗ Error: $_" -ForegroundColor Red
        $errors++
    }
    
    Write-Host ""
}

# ============================================================================
# Summary
# ============================================================================

Write-Host ""
Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📊 Summary" -ForegroundColor Cyan
Write-Host "════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "✓ Updated: $updated issues" -ForegroundColor Green
Write-Host "⊘ Skipped: $skipped issues" -ForegroundColor Yellow
if ($errors -gt 0) {
    Write-Host "✗ Errors: $errors issues" -ForegroundColor Red
}
Write-Host ""

if ($DryRun) {
    Write-Host "⚠️  This was a DRY RUN - no actual changes were made" -ForegroundColor Yellow
    Write-Host "Run without -DryRun to apply changes" -ForegroundColor Yellow
} else {
    Write-Host "✅ All fields updated successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🌐 View project: https://github.com/users/$OWNER/projects/$PROJECT_NUMBER" -ForegroundColor Cyan
}
Write-Host ""
