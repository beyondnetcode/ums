import { describe, it, expect, vi, beforeEach } from 'vitest';
import { tenantService } from './tenant.service';
import * as httpClientModule from '@infra/http/httpClient';
import * as graphqlQueriesModule from '@infra/identity/queries/tenant.graphql';
import * as loggerModule from '@app/utils/logger';

vi.mock('@infra/http/httpClient', () => ({
  httpClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

vi.mock('@infra/identity/queries/tenant.graphql', () => ({
  graphqlQueries: {
    getTenants: vi.fn(),
    getTenantById: vi.fn(),
    getTenantBranches: vi.fn(),
  },
}));

vi.mock('@app/utils/logger', () => ({
  logger: {
    error: vi.fn(),
  },
}));

describe('tenantService', () => {
  beforeEach(() => {
    vi.mocked(httpClientModule.httpClient.get).mockClear();
    vi.mocked(httpClientModule.httpClient.post).mockClear();
    vi.mocked(httpClientModule.httpClient.put).mockClear();
    vi.mocked(httpClientModule.httpClient.delete).mockClear();
    vi.mocked(graphqlQueriesModule.graphqlQueries.getTenants).mockClear();
    vi.mocked(graphqlQueriesModule.graphqlQueries.getTenantById).mockClear();
    vi.mocked(graphqlQueriesModule.graphqlQueries.getTenantBranches).mockClear();
    vi.mocked(loggerModule.logger.error).mockClear();
  });

  describe('getAllTenants', () => {
    it('calls graphql with default params', async () => {
      vi.mocked(graphqlQueriesModule.graphqlQueries.getTenants).mockResolvedValue({
        getTenants: {
          items: [],
          page: 1,
          pageSize: 20,
          totalItems: 0,
          totalPages: 0,
        },
      });

      const result = await tenantService.getAllTenants();

      expect(result.items).toHaveLength(0);
      expect(graphqlQueriesModule.graphqlQueries.getTenants).toHaveBeenCalledWith({
        page: 1,
        pageSize: 20,
        search: undefined,
        criteria: undefined,
        status: undefined,
        sortBy: undefined,
        sortOrder: undefined,
      });
    });

    it('passes custom params', async () => {
      vi.mocked(graphqlQueriesModule.graphqlQueries.getTenants).mockResolvedValue({
        getTenants: {
          items: [{ tenantId: '12345678-1234-1234-1234-123456789012', code: 'T1', name: 'Tenant 1', type: 'Enterprise', status: 'Active', parentTenantId: null }],
          page: 2,
          pageSize: 10,
          totalItems: 1,
          totalPages: 1,
        },
      });

      const result = await tenantService.getAllTenants({ page: 2, pageSize: 10, search: 'test' });

      expect(result.items).toHaveLength(1);
      expect(result.page).toBe(2);
    });

    it('throws on invalid response', async () => {
      vi.mocked(graphqlQueriesModule.graphqlQueries.getTenants).mockResolvedValue({
        getTenants: { invalid: 'shape' },
      });

      await expect(tenantService.getAllTenants()).rejects.toThrow('Invalid GraphQL response shape');
      expect(loggerModule.logger.error).toHaveBeenCalled();
    });
  });

  describe('getTenantById', () => {
    it('returns parsed tenant', async () => {
      vi.mocked(graphqlQueriesModule.graphqlQueries.getTenantById).mockResolvedValue({
        getTenantById: {
          tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
          code: 'T1',
          name: 'Tenant 1',
          type: 'Enterprise',
          status: 'Active',
          parentTenantId: null,
        },
      });

      const result = await tenantService.getTenantById('3fa85f64-5717-4562-b3fc-2c963f66afa6');

      expect(result.tenantId).toBe('3fa85f64-5717-4562-b3fc-2c963f66afa6');
    });

    it('throws when tenant not found', async () => {
      vi.mocked(graphqlQueriesModule.graphqlQueries.getTenantById).mockResolvedValue({
        getTenantById: null,
      });

      await expect(tenantService.getTenantById('nonexistent')).rejects.toThrow('Tenant not found');
    });
  });

  describe('getBranches', () => {
    it('returns parsed branches', async () => {
      vi.mocked(graphqlQueriesModule.graphqlQueries.getTenantBranches).mockResolvedValue({
        getTenantBranches: [
          { branchId: '3fa85f64-5717-4562-b3fc-2c963f66afa6', code: 'B1', name: 'Branch 1', isActive: true },
        ],
      });

      const result = await tenantService.getBranches('3fa85f64-5717-4562-b3fc-2c963f66afa7');

      expect(result).toHaveLength(1);
      expect(result[0].branchId).toBe('3fa85f64-5717-4562-b3fc-2c963f66afa6');
    });
  });

  describe('createTenant', () => {
    it('calls POST and returns parsed response', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({
        data: { tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa6', code: 'NEW', name: 'New Tenant' },
      });

      const result = await tenantService.createTenant({ code: 'NEW', name: 'New Tenant', type: 'Enterprise' });

      expect(result.tenantId).toBe('3fa85f64-5717-4562-b3fc-2c963f66afa6');
      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith('/tenants', expect.any(Object));
    });
  });

  describe('activateTenant', () => {
    it('calls activate endpoint', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({});

      await tenantService.activateTenant('t1');

      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith('/tenants/t1/activate');
    });
  });

  describe('suspendTenant', () => {
    it('calls suspend endpoint', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({});

      await tenantService.suspendTenant('t1');

      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith('/tenants/t1/suspend');
    });
  });

  describe('addBranch', () => {
    it('calls POST branches endpoint', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({
        data: { branchId: '3fa85f64-5717-4562-b3fc-2c963f66afa6', tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa7', code: 'B1' },
      });

      const result = await tenantService.addBranch('3fa85f64-5717-4562-b3fc-2c963f66afa7', { code: 'B1', name: 'Branch 1' });

      expect(result.branchId).toBe('3fa85f64-5717-4562-b3fc-2c963f66afa6');
      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith('/tenants/3fa85f64-5717-4562-b3fc-2c963f66afa7/branches', expect.any(Object));
    });
  });

  describe('removeBranch', () => {
    it('calls DELETE branch endpoint', async () => {
      vi.mocked(httpClientModule.httpClient.delete).mockResolvedValue({});

      await tenantService.removeBranch('t1', 'b1');

      expect(httpClientModule.httpClient.delete).toHaveBeenCalledWith('/tenants/t1/branches/b1');
    });
  });

  describe('deactivateBranch', () => {
    it('calls deactivate endpoint', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({});

      await tenantService.deactivateBranch('t1', 'b1');

      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith('/tenants/t1/branches/b1/deactivate');
    });
  });

  describe('reactivateBranch', () => {
    it('calls reactivate endpoint', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({});

      await tenantService.reactivateBranch('t1', 'b1');

      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith('/tenants/t1/branches/b1/reactivate');
    });
  });
});
