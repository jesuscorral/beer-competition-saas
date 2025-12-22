-- =====================================================
-- Development Tenant Setup
-- =====================================================
-- This script creates a default tenant for development and testing purposes.
-- Tenant ID: 11111111-1111-1111-1111-111111111111
--
-- Usage:
-- 1. Run this script after initial database migration
-- 2. This tenant will be used automatically in Development environment when no X-Tenant-ID header is provided
--
-- IMPORTANT: Do NOT use this tenant ID in production!
-- =====================================================

-- Create Competition schema if it doesn't exist
CREATE SCHEMA IF NOT EXISTS "Competition";

-- Create tenants table in Competition schema
-- This will be managed by EF Core migrations once Tenant entity is implemented
CREATE TABLE IF NOT EXISTS "Competition".tenants (
    id UUID PRIMARY KEY,
    tenant_id UUID NOT NULL,  -- Self-reference: tenant_id = id for Tenant
    organization_name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Active',
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NULL,
    CONSTRAINT uq_tenants_email UNIQUE (email),
    CONSTRAINT ck_tenants_tenant_id_equals_id CHECK (tenant_id = id)
);

-- Create index on tenant_id (for query filter consistency)
CREATE INDEX IF NOT EXISTS ix_tenants_tenant_id ON "Competition".tenants(tenant_id);

-- Create index on status
CREATE INDEX IF NOT EXISTS ix_tenants_status ON "Competition".tenants(status);

-- Insert development tenant
INSERT INTO "Competition".tenants (id, tenant_id, organization_name, email, status, created_at)
VALUES (
    '11111111-1111-1111-1111-111111111111',
    '11111111-1111-1111-1111-111111111111',  -- tenant_id = id (self-reference)
    'Development Organization',
    'dev@beercompetition.local',
    'Active',
    NOW()
)
ON CONFLICT (id) DO NOTHING;

-- Verify insertion
SELECT * FROM "Competition".tenants WHERE id = '11111111-1111-1111-1111-111111111111';

-- =====================================================
-- Output should show:
-- id: 11111111-1111-1111-1111-111111111111
-- tenant_id: 11111111-1111-1111-1111-111111111111
-- organization_name: Development Organization
-- email: dev@beercompetition.local
-- status: Active
-- =====================================================
