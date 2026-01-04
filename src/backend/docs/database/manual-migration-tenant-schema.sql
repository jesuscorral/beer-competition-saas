-- =====================================================
-- Manual Migration: Add Tenant Entity and Competition Schema
-- =====================================================
-- This script manually creates the database schema changes.
-- Ideally, use EF Core migrations instead:
--   dotnet ef migrations add AddTenantEntityAndCompetitionSchema
--
-- CAUTION: This script assumes:
-- 1. Database is empty or in development state
-- 2. No production data exists in 'competitions' table
-- 3. You have backup of existing data if needed
-- =====================================================

-- Step 1: Create Competition schema
CREATE SCHEMA IF NOT EXISTS "Competition";

-- Step 2: Create tenants table in Competition schema
CREATE TABLE "Competition".tenants (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    organization_name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Active',
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NULL,
    
    -- Constraints
    CONSTRAINT uq_tenants_email UNIQUE (email),
    CONSTRAINT ck_tenants_tenant_id_equals_id CHECK (tenant_id = id),
    CONSTRAINT ck_tenants_status CHECK (status IN ('Active', 'Suspended', 'Deleted'))
);

-- Step 3: Create indexes on tenants
CREATE INDEX ix_tenants_tenant_id ON "Competition".tenants(tenant_id);
CREATE INDEX ix_tenants_status ON "Competition".tenants(status);

-- Step 4: Insert development tenant
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

-- Step 5: Migrate competitions table to Competition schema
-- Option A: If competitions table exists in default schema (public)
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'public' 
        AND table_name = 'competitions'
    ) THEN
        -- Move table to Competition schema
        ALTER TABLE public.competitions SET SCHEMA "Competition";
        
        RAISE NOTICE 'Moved public.competitions to Competition.competitions';
    END IF;
END $$;

-- Step 6: Create competitions table if it doesn't exist
CREATE TABLE IF NOT EXISTS "Competition".competitions (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,
    name VARCHAR(255) NOT NULL,
    description VARCHAR(2000) NULL,
    registration_deadline TIMESTAMP NOT NULL,
    judging_start_date TIMESTAMP NOT NULL,
    judging_end_date TIMESTAMP NULL,
    status VARCHAR(50) NOT NULL,
    max_entries_per_entrant INTEGER NOT NULL DEFAULT 10,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NULL,
    
    -- Foreign key to tenants
    CONSTRAINT fk_competitions_tenant_id FOREIGN KEY (tenant_id) 
        REFERENCES "Competition".tenants(id) 
        ON DELETE RESTRICT,
    
    -- Check constraints
    CONSTRAINT ck_competitions_status CHECK (status IN ('Draft', 'Open', 'Judging', 'Completed', 'ResultsPublished', 'Canceled')),
    CONSTRAINT ck_competitions_dates CHECK (registration_deadline < judging_start_date)
);

-- Step 7: Create indexes on competitions
CREATE INDEX IF NOT EXISTS ix_competitions_tenant_id ON "Competition".competitions(tenant_id);
CREATE INDEX IF NOT EXISTS ix_competitions_tenant_status ON "Competition".competitions(tenant_id, status);
CREATE INDEX IF NOT EXISTS ix_competitions_registration_deadline ON "Competition".competitions(registration_deadline);

-- Step 8: Verify schema structure
DO $$
DECLARE
    tenant_count INTEGER;
    competition_count INTEGER;
BEGIN
    -- Count tenants
    SELECT COUNT(*) INTO tenant_count FROM "Competition".tenants;
    RAISE NOTICE 'Tenants count: %', tenant_count;
    
    -- Count competitions
    SELECT COUNT(*) INTO competition_count FROM "Competition".competitions;
    RAISE NOTICE 'Competitions count: %', competition_count;
    
    -- Verify foreign key
    IF EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_schema = 'Competition'
        AND table_name = 'competitions'
        AND constraint_name = 'fk_competitions_tenant_id'
    ) THEN
        RAISE NOTICE 'Foreign key constraint verified: OK';
    ELSE
        RAISE WARNING 'Foreign key constraint missing!';
    END IF;
END $$;

-- Step 9: Display schema information
SELECT 
    table_schema,
    table_name,
    column_name,
    data_type,
    is_nullable
FROM information_schema.columns
WHERE table_schema = 'Competition'
AND table_name IN ('tenants', 'competitions')
ORDER BY table_name, ordinal_position;

-- =====================================================
-- Migration Complete
-- =====================================================
-- Next steps:
-- 1. Verify data integrity: SELECT * FROM "Competition".tenants;
-- 2. Test tenant isolation: Create a competition via API
-- 3. Run EF Core migrations for future changes
-- =====================================================
