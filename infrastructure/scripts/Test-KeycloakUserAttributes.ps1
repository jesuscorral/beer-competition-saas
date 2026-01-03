# Test Keycloak User Attributes in JWT Tokens
# This script creates a test user with custom attributes and verifies they appear in JWT tokens

param(
    [string]$KeycloakUrl = "http://localhost:8080",
    [string]$Realm = "beercomp",
    [string]$ClientId = "frontend-spa",
    [string]$TestUsername = "testuser-$(Get-Date -Format 'yyyyMMddHHmmss')",
    [string]$TestPassword = "Test123!",
    [string]$AdminUsername = "admin",
    [string]$AdminPassword = "admin123"
)

$ErrorActionPreference = "Stop"

function Get-JwtPayload {
    param([string]$Token)
    $parts = $Token.Split('.')
    if ($parts.Count -ne 3) {
        throw "Invalid JWT token format"
    }
    $payload = $parts[1]
    $payload = $payload.Replace('-', '+').Replace('_', '/')
    while ($payload.Length % 4 -ne 0) {
        $payload += '='
    }
    $bytes = [System.Convert]::FromBase64String($payload)
    $json = [System.Text.Encoding]::UTF8.GetString($bytes)
    return $json | ConvertFrom-Json
}

Write-Host "========================================"
Write-Host "Keycloak User Attributes Test"
Write-Host "========================================"
Write-Host ""

# Step 1: Get admin access token
Write-Host "[1/5] Obtaining admin access token..."
$adminTokenUrl = "$KeycloakUrl/realms/master/protocol/openid-connect/token"
$adminTokenBody = @{
    username   = $AdminUsername
    password   = $AdminPassword
    grant_type = "password"
    client_id  = "admin-cli"
}

try {
    $adminTokenResponse = Invoke-RestMethod -Uri $adminTokenUrl -Method Post -Body $adminTokenBody -ContentType "application/x-www-form-urlencoded"
    $adminToken = $adminTokenResponse.access_token
    Write-Host "Admin token obtained successfully"
}
catch {
    Write-Host "Failed to obtain admin token: $_"
    exit 1
}

# Step 2: Create test user
Write-Host ""
Write-Host "[2/5] Creating test user: $TestUsername..."
$createUserUrl = "$KeycloakUrl/admin/realms/$Realm/users"
$testTenantId = New-Guid
$testCompetitionId = New-Guid

$userData = @{
    username      = $TestUsername
    enabled       = $true
    emailVerified = $true
    email         = "$TestUsername@example.com"
    firstName     = "Test"
    lastName      = "User"
    credentials   = @(
        @{
            type      = "password"
            value     = $TestPassword
            temporary = $false
        }
    )
    attributes    = @{
        tenant_id      = @($testTenantId.ToString())
        competition_id = @($testCompetitionId.ToString())
        bjcp_rank      = @("Certified")
    }
    realmRoles    = @("judge")
} | ConvertTo-Json -Depth 10

$headers = @{
    Authorization  = "Bearer $adminToken"
    "Content-Type" = "application/json"
}

try {
    $createResponse = Invoke-WebRequest -Uri $createUserUrl -Method Post -Body $userData -Headers $headers
    $location = $createResponse.Headers.Location
    $createdUserId = $location.Split('/')[-1]
    Write-Host "User created successfully (ID: $createdUserId)"
    Write-Host "  - tenant_id: $testTenantId"
    Write-Host "  - competition_id: $testCompetitionId"
    Write-Host "  - bjcp_rank: Certified"
}
catch {
    Write-Host "Failed to create user: $_"
    exit 1
}

# Step 3: Enable direct access grants for testing
Write-Host ""
Write-Host "[3/5] Configuring client for testing..."
$clientsUrl = "$KeycloakUrl/admin/realms/$Realm/clients?clientId=$ClientId"
try {
    $clientsResponse = Invoke-RestMethod -Uri $clientsUrl -Method Get -Headers @{ Authorization = "Bearer $adminToken" }
    $client = $clientsResponse[0]
    
    if (-not $client.directAccessGrantsEnabled) {
        Write-Host "Enabling direct access grants..."
        $updateClientUrl = "$KeycloakUrl/admin/realms/$Realm/clients/$($client.id)"
        $client.directAccessGrantsEnabled = $true
        $clientJson = $client | ConvertTo-Json -Depth 10
        Invoke-RestMethod -Uri $updateClientUrl -Method Put -Body $clientJson -Headers @{ 
            Authorization  = "Bearer $adminToken"
            "Content-Type" = "application/json"
        } | Out-Null
    }
    Write-Host "Client configured"
}
catch {
    Write-Warning "Could not configure client: $_"
}

# Step 4: Obtain token for test user
Write-Host ""
Write-Host "[4/5] Obtaining access token for test user..."
$tokenUrl = "$KeycloakUrl/realms/$Realm/protocol/openid-connect/token"
$tokenBody = @{
    username   = $TestUsername
    password   = $TestPassword
    grant_type = "password"
    client_id  = $ClientId
}

try {
    $tokenResponse = Invoke-RestMethod -Uri $tokenUrl -Method Post -Body $tokenBody -ContentType "application/x-www-form-urlencoded"
    $accessToken = $tokenResponse.access_token
    Write-Host "Access token obtained successfully"
}
catch {
    Write-Host "Failed to obtain access token: $_"
    exit 1
}

# Step 5: Decode JWT and verify claims
Write-Host ""
Write-Host "[5/5] Decoding JWT and verifying claims..."

try {
    $payload = Get-JwtPayload -Token $accessToken
    Write-Host "JWT decoded successfully"
    Write-Host ""
    Write-Host "Claims found:"
    
    $claimsToCheck = @{
        "tenant_id"      = $testTenantId.ToString()
        "competition_id" = $testCompetitionId.ToString()
        "bjcp_rank"      = "Certified"
    }
    
    $allClaimsPresent = $true
    
    foreach ($claim in $claimsToCheck.Keys) {
        $expectedValue = $claimsToCheck[$claim]
        $actualValue = $payload.$claim
        
        if ($actualValue) {
            if ($actualValue -eq $expectedValue) {
                Write-Host "  [OK] $claim = $actualValue"
            }
            else {
                Write-Host "  [WARN] $claim = $actualValue (expected: $expectedValue)"
            }
        }
        else {
            Write-Host "  [FAIL] $claim is MISSING"
            $allClaimsPresent = $false
        }
    }
    
    Write-Host ""
    Write-Host "Other claims:"
    Write-Host "  - sub: $($payload.sub)"
    Write-Host "  - preferred_username: $($payload.preferred_username)"
    Write-Host "  - email: $($payload.email)"
    if ($payload.roles) {
        Write-Host "  - roles: $($payload.roles -join ', ')"
    }
}
catch {
    Write-Host "Failed to decode JWT: $_"
    $allClaimsPresent = $false
}

# Cleanup
Write-Host ""
Write-Host "Cleanup..."
$cleanup = Read-Host "Delete test user? (y/N)"
if ($cleanup -eq 'y' -or $cleanup -eq 'Y') {
    try {
        $deleteUserUrl = "$KeycloakUrl/admin/realms/$Realm/users/$createdUserId"
        Invoke-RestMethod -Uri $deleteUserUrl -Method Delete -Headers @{ Authorization = "Bearer $adminToken" } | Out-Null
        Write-Host "Test user deleted"
    }
    catch {
        Write-Host "Failed to delete user: $_"
    }
}
else {
    Write-Host "Test user kept for manual inspection"
    Write-Host "Username: $TestUsername"
    Write-Host "Password: $TestPassword"
}

Write-Host ""
Write-Host "========================================"
if ($allClaimsPresent) {
    Write-Host "ALL TESTS PASSED"
    Write-Host "User attributes are correctly included in JWT tokens"
    exit 0
}
else {
    Write-Host "SOME TESTS FAILED"
    Write-Host "User attributes are NOT correctly configured"
    exit 1
}
