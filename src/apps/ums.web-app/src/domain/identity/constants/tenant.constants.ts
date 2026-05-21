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
