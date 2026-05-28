import { describe, it, expect } from 'vitest';
import { TENANT_TYPES, TENANT_PAGE_SIZE } from '@domain/identity/constants/tenant.constants';

describe('tenant.constants', () => {
  it('exports correct tenant types', () => {
    expect(TENANT_TYPES).toContain('INTERNAL');
    expect(TENANT_TYPES).toContain('SUPPLIER');
    expect(TENANT_TYPES).toContain('CLIENT');
    expect(TENANT_TYPES).toHaveLength(3);
  });

  it('exports correct page size', () => {
    expect(TENANT_PAGE_SIZE).toBe(9);
  });
});
