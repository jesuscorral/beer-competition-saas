# =====================================================
# PowerShell Script to Insert Development Tenant
# =====================================================
# This script inserts the development tenant into PostgreSQL
# Requirements: PostgreSQL client tools installed
# =====================================================

param(
    [string]$Host = "localhost",
    [string]$Port = "5432",
    [string]$Database = "beercompetition",
    [string]$Username = "postgres"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Insert Development Tenant" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# SQL to insert tenant
$sql = @"
INSERT INTO "Competition".tenants (id, tenant_id, organization_name, email, status, created_at)
VALUES (
    '11111111-1111-1111-1111-111111111111',
    '11111111-1111-1111-1111-111111111111',
    'Development Organization',
    'dev@beercompetition.local',
    'Active',
    NOW()
)
ON CONFLICT (id) DO NOTHING;
"@

Write-Host "Database: $Database" -ForegroundColor Yellow
Write-Host "Host: $Host" -ForegroundColor Yellow
Write-Host "User: $Username" -ForegroundColor Yellow
Write-Host ""

# Check if psql is available
$psqlPath = Get-Command psql -ErrorAction SilentlyContinue

if (-not $psqlPath) {
    Write-Host "❌ ERROR: psql command not found" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install PostgreSQL client tools or use one of these alternatives:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Option 1: pgAdmin" -ForegroundColor Cyan
    Write-Host "  1. Open pgAdmin" -ForegroundColor White
    Write-Host "  2. Connect to database 'beercompetition'" -ForegroundColor White
    Write-Host "  3. Open Query Tool (Tools -> Query Tool)" -ForegroundColor White
    Write-Host "  4. Copy and execute the SQL from INSERT-DEVELOPMENT-TENANT.sql" -ForegroundColor White
    Write-Host ""
    Write-Host "Option 2: DBeaver / DataGrip" -ForegroundColor Cyan
    Write-Host "  1. Open your SQL client" -ForegroundColor White
    Write-Host "  2. Connect to database 'beercompetition'" -ForegroundColor White
    Write-Host "  3. Execute the SQL from INSERT-DEVELOPMENT-TENANT.sql" -ForegroundColor White
    Write-Host ""
    Write-Host "Option 3: Install PostgreSQL Tools" -ForegroundColor Cyan
    Write-Host "  Download from: https://www.postgresql.org/download/windows/" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host "✓ PostgreSQL client tools found" -ForegroundColor Green
Write-Host ""

# Execute SQL
try {
    Write-Host "Inserting development tenant..." -ForegroundColor Yellow
    
    $env:PGPASSWORD = Read-Host "Enter PostgreSQL password for user '$Username'" -AsSecureString
    $plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($env:PGPASSWORD))
    $env:PGPASSWORD = $plainPassword
    
    $result = & psql -h $Host -p $Port -U $Username -d $Database -c $sql 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ SUCCESS: Development tenant inserted!" -ForegroundColor Green
        Write-Host ""
        
        # Verify insertion
        Write-Host "Verifying tenant..." -ForegroundColor Yellow
        $verifySql = "SELECT id, organization_name, email, status FROM ""Competition"".tenants WHERE id = '11111111-1111-1111-1111-111111111111';"
        $verifyResult = & psql -h $Host -p $Port -U $Username -d $Database -c $verifySql 2>&1
        
        Write-Host ""
        Write-Host $verifyResult
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Cyan
        Write-Host "Next Steps:" -ForegroundColor Cyan
        Write-Host "========================================" -ForegroundColor Cyan
        Write-Host "1. Start the application:" -ForegroundColor White
        Write-Host "   dotnet run --project Host\BeerCompetition.Host" -ForegroundColor Gray
        Write-Host ""
        Write-Host "2. Open Swagger:" -ForegroundColor White
        Write-Host "   https://localhost:5001" -ForegroundColor Gray
        Write-Host ""
        Write-Host "3. Test POST /api/competitions" -ForegroundColor White
        Write-Host "   (No X-Tenant-ID header needed in Development)" -ForegroundColor Gray
        Write-Host ""
    }
    else {
        Write-Host ""
        Write-Host "❌ ERROR: Failed to insert tenant" -ForegroundColor Red
        Write-Host $result
        Write-Host ""
        Write-Host "Please check:" -ForegroundColor Yellow
        Write-Host "  - Database 'beercompetition' exists" -ForegroundColor White
        Write-Host "  - Schema 'Competition' exists" -ForegroundColor White
        Write-Host "  - EF Core migrations have been applied" -ForegroundColor White
        Write-Host ""
        exit 1
    }
}
catch {
    Write-Host ""
    Write-Host "❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    exit 1
}
finally {
    # Clear password from environment
    Remove-Item Env:\PGPASSWORD -ErrorAction SilentlyContinue
}
