-- =====================================================
-- EXECUTE THIS SQL IN YOUR DATABASE CLIENT (pgAdmin, DBeaver, etc.)
-- =====================================================
-- Database: beercompetition
-- This inserts the development tenant required for testing
-- =====================================================

-- Step 1: Verify schema exists
SELECT schema_name 
FROM information_schema.schemata 
WHERE schema_name = 'Competition';
-- Expected: Should return 'Competition'

-- Step 2: Verify tenants table exists
SELECT table_schema, table_name 
FROM information_schema.tables 
WHERE table_schema = 'Competition' 
AND table_name = 'tenants';
-- Expected: Should return 'Competition' | 'tenants'

-- Step 3: Insert development tenant
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
-- Expected: INSERT 0 1 (if new) or INSERT 0 0 (if already exists)

-- Step 4: Verify insertion was successful
SELECT 
    id,
    tenant_id,
    organization_name,
    email,
    status,
    created_at
FROM "Competition".tenants
WHERE id = '11111111-1111-1111-1111-111111111111';
-- Expected: Should return 1 row with the development tenant

-- Step 5: Verify foreign key constraint exists
SELECT
    tc.constraint_name,
    tc.table_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
    AND tc.table_schema = kcu.table_schema
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
    AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY'
AND tc.table_name = 'competitions'
AND tc.table_schema = 'Competition';
-- Expected: Should show FK_competitions_tenants_tenant_id

-- =====================================================
-- RESULT: Development tenant ready for use
-- =====================================================
-- You can now:
-- 1. Start the application: dotnet run --project Host\BeerCompetition.Host
-- 2. Open Swagger: https://localhost:5001
-- 3. Create competitions without providing X-Tenant-ID header
-- =====================================================
