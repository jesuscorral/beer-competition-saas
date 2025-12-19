# ============================================================================
# 🏷️ Beer Competition SaaS - GitHub Labels Setup
# ============================================================================
# Purpose: Create all necessary labels for project organization
# ============================================================================

param(
    [switch]$DryRun = $false
)

$ErrorActionPreference = "Stop"

$OWNER = "jesuscorral"
$REPO = "beer-competition-saas"

Write-Host "🏷️  Beer Competition SaaS - GitHub Labels Setup" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

if ($DryRun) {
    Write-Host "⚠️  DRY RUN MODE - No changes will be applied" -ForegroundColor Yellow
    Write-Host ""
}

# ============================================================================
# Label Definitions
# ============================================================================

$labels = @(
    # Priority Labels
    @{ name = "priority-p0"; color = "B60205"; description = "Critical - MVP Blocker" }
    @{ name = "priority-p1"; color = "D93F0B"; description = "High - Enhanced Feature" }
    @{ name = "priority-p2"; color = "FBCA04"; description = "Medium - Nice to Have" }
    
    # Epic Labels
    @{ name = "epic-infrastructure"; color = "0E8A16"; description = "Infrastructure & DevOps" }
    @{ name = "epic-competitions"; color = "1D76DB"; description = "Competition Management" }
    @{ name = "epic-entries"; color = "5319E7"; description = "Entry Management" }
    @{ name = "epic-flights"; color = "C5DEF5"; description = "Flight Management" }
    @{ name = "epic-scoring"; color = "F9D0C4"; description = "Scoring & Judging" }
    @{ name = "epic-best-of-show"; color = "FFD700"; description = "Best of Show" }
    @{ name = "epic-authentication"; color = "D4C5F9"; description = "Authentication & Authorization" }
    @{ name = "epic-ui-frontend"; color = "BFD4F2"; description = "UI/Frontend" }
    @{ name = "epic-observability"; color = "C2E0C6"; description = "Observability & Monitoring" }
    
    # Complexity Labels
    @{ name = "complexity-S"; color = "E4E669"; description = "Small (1-2 days)" }
    @{ name = "complexity-M"; color = "FEF2C0"; description = "Medium (3-4 days)" }
    @{ name = "complexity-L"; color = "FFC9A5"; description = "Large (5-7 days)" }
    
    # Sprint Labels
    @{ name = "sprint-0"; color = "006B75"; description = "Sprint 0 - Foundation" }
    @{ name = "sprint-1"; color = "1F8297"; description = "Sprint 1 - Auth & Competitions" }
    @{ name = "sprint-2"; color = "3E99B3"; description = "Sprint 2 - Entry Management" }
    @{ name = "sprint-3"; color = "5DB1CF"; description = "Sprint 3 - Flight Management" }
    @{ name = "sprint-4"; color = "7CC8E0"; description = "Sprint 4 - Offline PWA" }
    @{ name = "sprint-5"; color = "9BE0F2"; description = "Sprint 5 - Best of Show" }
    @{ name = "sprint-6"; color = "BAF8FF"; description = "Sprint 6 - Results & Polish" }
    @{ name = "sprint-post-mvp"; color = "EDEDED"; description = "Post-MVP Features" }
    
    # Agent Labels
    @{ name = "agent-backend"; color = "7057FF"; description = "Backend Agent" }
    @{ name = "agent-frontend"; color = "008672"; description = "Frontend Agent" }
    @{ name = "agent-devops"; color = "D876E3"; description = "DevOps Agent" }
    @{ name = "agent-qa"; color = "BFDADC"; description = "QA Agent" }
    @{ name = "agent-data-science"; color = "1F77B4"; description = "Data Science Agent" }
    @{ name = "agent-product-owner"; color = "E99695"; description = "Product Owner" }
    
    # Type Labels
    @{ name = "feature"; color = "A2EEEF"; description = "New feature or enhancement" }
    @{ name = "bug"; color = "D73A4A"; description = "Something isn't working" }
    @{ name = "documentation"; color = "0075CA"; description = "Documentation improvements" }
    @{ name = "test"; color = "D4C5F9"; description = "Testing related" }
    @{ name = "refactor"; color = "FBCA04"; description = "Code refactoring" }
    @{ name = "chore"; color = "FEF2C0"; description = "Maintenance tasks" }
    
    # Status Labels
    @{ name = "mvp-blocker"; color = "B60205"; description = "Blocking MVP completion" }
    @{ name = "blocked"; color = "D93F0B"; description = "Blocked by dependencies" }
    @{ name = "needs-review"; color = "FBCA04"; description = "Needs code review" }
    @{ name = "needs-testing"; color = "FEF2C0"; description = "Needs testing" }
    @{ name = "ready-for-dev"; color = "0E8A16"; description = "Ready for development" }
    @{ name = "in-progress"; color = "1D76DB"; description = "Work in progress" }
    @{ name = "triage"; color = "EDEDED"; description = "Needs triage" }
    
    # Special Labels
    @{ name = "good-first-issue"; color = "7057FF"; description = "Good for newcomers" }
    @{ name = "help-wanted"; color = "008672"; description = "Extra attention needed" }
    @{ name = "question"; color = "D876E3"; description = "Further information requested" }
    @{ name = "duplicate"; color = "CFD3D7"; description = "Duplicate issue" }
    @{ name = "wontfix"; color = "FFFFFF"; description = "Will not be worked on" }
)

# ============================================================================
# Create or Update Labels
# ============================================================================

$totalLabels = $labels.Count
$currentIndex = 0
$createdCount = 0
$updatedCount = 0
$errorCount = 0

Write-Host "📋 Processing $totalLabels labels..." -ForegroundColor Cyan
Write-Host ""

foreach ($label in $labels) {
    $currentIndex++
    $name = $label.name
    $color = $label.color
    $description = $label.description
    
    Write-Host "[$currentIndex/$totalLabels] $name" -ForegroundColor Yellow
    Write-Host "  🎨 Color: #$color" -ForegroundColor Gray
    Write-Host "  📝 Description: $description" -ForegroundColor Gray
    
    if ($DryRun) {
        Write-Host "  ⏭️  Skipping (dry run)" -ForegroundColor DarkGray
        $createdCount++
        Write-Host ""
        continue
    }
    
    try {
        # Check if label exists
        $existingLabel = gh label list --repo "$OWNER/$REPO" --json name,color,description | 
                        ConvertFrom-Json | 
                        Where-Object { $_.name -eq $name }
        
        if ($existingLabel) {
            # Update existing label
            gh label edit $name `
                --repo "$OWNER/$REPO" `
                --color $color `
                --description $description | Out-Null
            
            Write-Host "  ✅ Updated" -ForegroundColor Green
            $updatedCount++
        } else {
            # Create new label
            gh label create $name `
                --repo "$OWNER/$REPO" `
                --color $color `
                --description $description | Out-Null
            
            Write-Host "  ✅ Created" -ForegroundColor Green
            $createdCount++
        }
        
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
Write-Host "📊 Label Setup Summary" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Total labels processed: $totalLabels" -ForegroundColor White
Write-Host "✅ Created: $createdCount" -ForegroundColor Green
Write-Host "🔄 Updated: $updatedCount" -ForegroundColor Yellow
Write-Host "❌ Errors: $errorCount" -ForegroundColor Red
Write-Host ""

if ($DryRun) {
    Write-Host "⚠️  This was a DRY RUN - no changes were applied" -ForegroundColor Yellow
    Write-Host "Run without -DryRun flag to apply changes" -ForegroundColor Yellow
} else {
    Write-Host "✅ All labels configured successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🔗 View labels: https://github.com/$OWNER/$REPO/labels" -ForegroundColor Cyan
}

Write-Host ""
