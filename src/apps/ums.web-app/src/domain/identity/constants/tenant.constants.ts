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
  { id: '3fa85f64-5717-4562-b3fc-2c963f66afa6', code: 'RANSA_PERU', name: 'Ransa Comercial S.A.' },
  { id: 'c9b736b4-6a84-48f8-b34d-176bc5a6d542', code: 'NEPTUNIA', name: 'Neptunia S.A.' },
  { id: 'a3f5b9d2-7c3d-4c8e-a9b0-123456789abc', code: 'APM_CALLAO', name: 'APM Terminals Callao S.A.' },
  { id: '5f4e3d2c-1b0a-9f8e-7d6c-543210987654', code: 'UNIMAR', name: 'Unimar S.A.' },
];
