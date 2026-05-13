-- ============================================================================
-- UMS Technical Enabler (TE-03): PostgreSQL Row-Level Security (RLS)
-- Scope: Multi-Tenant Isolation by Organization
-- ============================================================================

-- 1. Ensure clean testing environment
DROP TABLE IF EXISTS public.subjects CASCADE;
DROP TABLE IF EXISTS public.organizations CASCADE;

-- 2. Create the absolute Containment Boundary: Organizations (Tenants)
CREATE TABLE public.organizations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    org_type VARCHAR(50) NOT NULL CHECK (org_type IN ('INTERNAL', 'SUPPLIER', 'CLIENT')),
    erp_code VARCHAR(100) UNIQUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 3. Create Subject Entity under Organization tutelage
CREATE TABLE public.subjects (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    organization_id UUID NOT NULL REFERENCES public.organizations(id) ON DELETE RESTRICT,
    email VARCHAR(255) NOT NULL UNIQUE,
    identity_reference VARCHAR(100) NOT NULL,
    reference_type VARCHAR(50) NOT NULL CHECK (reference_type IN ('HR_ID', 'VENDOR_CODE', 'GOVERNMENT_ID', 'PARTNER_REF')),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- 4. Add indexes for High Performance RLS filtering
CREATE INDEX idx_subjects_org_id ON public.subjects(organization_id);

-- 5. Enable Row-Level Security (RLS) for the Domain Tables
ALTER TABLE public.organizations ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.subjects ENABLE ROW LEVEL SECURITY;

-- ============================================================================
-- 6. Define Security Isolation Policies
-- ============================================================================

-- Current Setting extraction: reads 'app.current_organization_id' from the session.
-- If empty/null, returns NULL resulting in safe zero-row access.
CREATE POLICY organization_isolation_policy ON public.subjects
    FOR ALL
    TO public
    USING (
        organization_id = NULLIF(current_setting('app.current_organization_id', true), '')::uuid
    )
    WITH CHECK (
        organization_id = NULLIF(current_setting('app.current_organization_id', true), '')::uuid
    );

CREATE POLICY organization_self_isolation_policy ON public.organizations
    FOR ALL
    TO public
    USING (
        id = NULLIF(current_setting('app.current_organization_id', true), '')::uuid
    );

-- ============================================================================
-- 7. Seeding Test Data
-- ============================================================================

-- Create 2 Isolated Organizations
INSERT INTO public.organizations (id, name, org_type, erp_code) VALUES 
('11111111-1111-1111-1111-111111111111', 'Logistics Corp (Internal)', 'INTERNAL', 'LOG-001'),
('22222222-2222-2222-2222-222222222222', 'Retail Partner (Supplier)', 'SUPPLIER', 'SUP-999');

-- Populate Subjects inside respective organizations
INSERT INTO public.subjects (organization_id, email, identity_reference, reference_type) VALUES
('11111111-1111-1111-1111-111111111111', 'ceo@logisticscorp.com', 'EMP-001', 'HR_ID'),
('11111111-1111-1111-1111-111111111111', 'operator@logisticscorp.com', 'EMP-002', 'HR_ID'),
('22222222-2222-2222-2222-222222222222', 'driver@retailpartner.com', 'VND-456', 'VENDOR_CODE');

-- ============================================================================
-- 8. Demonstration / Verification Scenarios
-- ============================================================================

/*
-- Scenario A: Simulating "Logistics Corp" Request Context
BEGIN;
SET LOCAL app.current_organization_id = '11111111-1111-1111-1111-111111111111';
-- This returns 2 rows (ceo and operator). driver is physically isolated.
SELECT * FROM public.subjects; 
COMMIT;

-- Scenario B: Simulating "Retail Partner" Request Context
BEGIN;
SET LOCAL app.current_organization_id = '22222222-2222-2222-2222-222222222222';
-- This returns 1 row (driver). corporate employees are hidden.
SELECT * FROM public.subjects; 
COMMIT;

-- Scenario C: Simulating empty context (Zero-Trust Default)
BEGIN;
SET LOCAL app.current_organization_id = '';
-- This returns ZERO rows. Safe by default!
SELECT * FROM public.subjects; 
COMMIT;
*/
