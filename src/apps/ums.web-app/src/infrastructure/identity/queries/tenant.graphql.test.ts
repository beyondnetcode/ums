import { describe, it, expect, vi, beforeEach } from 'vitest';
import { graphqlQueries } from './tenant.graphql';
import { GraphQlValidationError } from '@infra/http/graphqlClient';
import * as graphqlClientModule from '@infra/http/graphqlClient';

vi.mock('@infra/http/graphqlClient', () => ({
  graphqlClient: {
    request: vi.fn(),
  },
  GraphQlValidationError: class GraphQlValidationError extends Error {
    constructor(message: string, public details: string[]) {
      super(message);
      this.name = 'GraphQlValidationError';
    }
  },
}));

describe('graphqlQueries', () => {
  beforeEach(() => {
    vi.mocked(graphqlClientModule.graphqlClient.request).mockClear();
  });

  describe('getTenants', () => {
    it('calls graphqlClient with correct query and variables', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
        getTenants: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 },
      });

      await graphqlQueries.getTenants({ page: 1, pageSize: 10 });

      expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(
        expect.stringContaining('query Tenants'),
        expect.objectContaining({ page: 1, pageSize: 10 }),
      );
    });

    it('includes optional search parameter', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
        getTenants: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 },
      });

      await graphqlQueries.getTenants({ page: 1, pageSize: 10, search: 'test' });

      const callArgs = vi.mocked(graphqlClientModule.graphqlClient.request).mock.calls[0];
      expect(callArgs[1]).toHaveProperty('search', 'test');
    });

    it('includes optional criteria parameter', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
        getTenants: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 },
      });

      await graphqlQueries.getTenants({ page: 1, pageSize: 10, criteria: 'name' });

      const callArgs = vi.mocked(graphqlClientModule.graphqlClient.request).mock.calls[0];
      expect(callArgs[1]).toHaveProperty('criteria', 'name');
    });

    it('includes optional status parameter', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
        getTenants: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 },
      });

      await graphqlQueries.getTenants({ page: 1, pageSize: 10, status: 'Active' });

      const callArgs = vi.mocked(graphqlClientModule.graphqlClient.request).mock.calls[0];
      expect(callArgs[1]).toHaveProperty('status', 'Active');
    });

    it('includes optional sortBy parameter', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
        getTenants: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 },
      });

      await graphqlQueries.getTenants({ page: 1, pageSize: 10, sortBy: 'name' });

      const callArgs = vi.mocked(graphqlClientModule.graphqlClient.request).mock.calls[0];
      expect(callArgs[1]).toHaveProperty('sortBy', 'name');
    });

    it('throws on invalid page', async () => {
      await expect(graphqlQueries.getTenants({ page: 0, pageSize: 10 })).rejects.toThrow(GraphQlValidationError);
    });

    it('throws on invalid pageSize', async () => {
      await expect(graphqlQueries.getTenants({ page: 1, pageSize: 0 })).rejects.toThrow(GraphQlValidationError);
    });
  });

  describe('getTenantById', () => {
    it('calls graphqlClient with tenantId', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
        getTenantById: { tenantId: '123', code: 'TEST', name: 'Test', type: 'INTERNAL', status: 'Active', parentTenantId: null, companyReference: null },
      });

      await graphqlQueries.getTenantById('3fa85f64-5717-4562-b3fc-2c963f66afa6');

      expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(
        expect.stringContaining('query Tenant'),
        expect.objectContaining({ tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa6' }),
      );
    });

    it('throws on empty tenantId', async () => {
      await expect(graphqlQueries.getTenantById('')).rejects.toThrow(GraphQlValidationError);
    });

    it('throws on whitespace tenantId', async () => {
      await expect(graphqlQueries.getTenantById('   ')).rejects.toThrow(GraphQlValidationError);
    });
  });

  describe('getTenantBranches', () => {
    it('calls graphqlClient with tenantId', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
        getTenantBranches: [],
      });

      await graphqlQueries.getTenantBranches('3fa85f64-5717-4562-b3fc-2c963f66afa6');

      expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(
        expect.stringContaining('query TenantBranches'),
        expect.objectContaining({ tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa6' }),
      );
    });

    it('throws on empty tenantId', async () => {
      await expect(graphqlQueries.getTenantBranches('')).rejects.toThrow(GraphQlValidationError);
    });
  });
});
