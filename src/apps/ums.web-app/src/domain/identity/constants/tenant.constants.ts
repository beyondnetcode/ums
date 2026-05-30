/**
 * tenant.constants.ts — Domain constants for the Tenant aggregate.
 *
 * Centralises values that are referenced by multiple components
 * (forms, profile cards, selects) so changes propagate from one place.
 */

/** Valid tenant type values accepted by the API. */
export const TENANT_TYPES = ['INTERNAL', 'SUPPLIER', 'CLIENT'] as const;
export type TenantType = (typeof TENANT_TYPES)[number];

/** Default page size for tenant list views. */
export const TENANT_PAGE_SIZE = 9 as const;

/** Dev tenant definitions for local testing. */
export interface DevTenant {
  id: string;
  code: string;
  name: string;
}

export const DEV_TENANTS: DevTenant[] = [
  { id: '11111111-1111-1111-1111-111111111111', code: 'INTERNAL_ADMIN', name: 'Internal Admin (SuperAdmin)' },
  { id: '00000001-0000-0000-0000-000000000001', code: 'TECHNO', name: 'TechnoCorp' },
  { id: '00000002-0000-0000-0000-000000000001', code: 'LOGISTICA', name: 'LogisticaGlobal' },
  { id: '00000003-0000-0000-0000-000000000001', code: 'RETAIL', name: 'RetailMax' },
  { id: '00000004-0000-0000-0000-000000000001', code: 'SALUD', name: 'SaludTotal' },
  { id: '00000005-0000-0000-0000-000000000001', code: 'EDU', name: 'EduLearn' },
  { id: '00000006-0000-0000-0000-000000000001', code: 'FINANCE', name: 'FinancePro' },
  { id: '00000007-0000-0000-0000-000000000001', code: 'MEDIA', name: 'MediaHub' },
  { id: '00000008-0000-0000-0000-000000000001', code: 'AGILE', name: 'AgileFlow' },
  { id: '00000009-0000-0000-0000-000000000001', code: 'NEXTGEN', name: 'NextGenTech' },
  { id: '0000000a-0000-0000-0000-000000000001', code: 'QUANTUM', name: 'QuantumLabs' },
];
