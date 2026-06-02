import { describe, it, expect, vi, beforeEach } from 'vitest';
import { profileService } from './profile.service';
import * as httpClientModule from '@infra/http/httpClient';
import * as graphqlProfileQueriesModule from '../queries/profile.graphql';
import * as loggerModule from '@app/utils/logger';

vi.mock('@infra/http/httpClient', () => ({
  httpClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

vi.mock('../queries/profile.graphql', () => ({
  graphqlProfileQueries: {
    getProfiles: vi.fn(),
    getProfileById: vi.fn(),
  },
}));

vi.mock('@app/utils/logger', () => ({
  logger: {
    error: vi.fn(),
  },
}));

describe('profileService', () => {
  beforeEach(() => {
    vi.mocked(httpClientModule.httpClient.get).mockClear();
    vi.mocked(httpClientModule.httpClient.post).mockClear();
    vi.mocked(httpClientModule.httpClient.put).mockClear();
    vi.mocked(httpClientModule.httpClient.delete).mockClear();
    vi.mocked(graphqlProfileQueriesModule.graphqlProfileQueries.getProfiles).mockClear();
    vi.mocked(graphqlProfileQueriesModule.graphqlProfileQueries.getProfileById).mockClear();
    vi.mocked(loggerModule.logger.error).mockClear();
  });

  describe('getAll', () => {
    it('calls graphql with default params', async () => {
      vi.mocked(graphqlProfileQueriesModule.graphqlProfileQueries.getProfiles).mockResolvedValue({
        profiles: {
          items: [],
          page: 1,
          pageSize: 20,
          totalItems: 0,
          totalPages: 0,
        },
      });

      const result = await profileService.getAll();

      expect(result.items).toHaveLength(0);
    });

    it('passes custom params', async () => {
      vi.mocked(graphqlProfileQueriesModule.graphqlProfileQueries.getProfiles).mockResolvedValue({
        profiles: {
          items: [{ profileId: '12345678-1234-1234-1234-123456789012', tenantId: '12345678-1234-1234-1234-123456789012', tenantCode: 'TENANT', tenantName: 'Tenant', userId: '12345678-1234-1234-1234-123456789012', userEmail: 'user@example.com', roleId: '12345678-1234-1234-1234-123456789012', roleCode: 'ADMIN', roleName: 'Admin', systemSuiteId: '12345678-1234-1234-1234-123456789012', systemSuiteCode: 'CORE', systemSuiteName: 'Core', branchId: null, branchName: null, scope: 'Tenant', isActive: true, permissionCount: 0, permissions: [] }],
          page: 2,
          pageSize: 10,
          totalItems: 1,
          totalPages: 1,
        },
      });

      const result = await profileService.getAll({ page: 2, pageSize: 10, search: 'test' });

      expect(result.items).toHaveLength(1);
      expect(result.page).toBe(2);
    });

    it('throws on invalid response', async () => {
      vi.mocked(graphqlProfileQueriesModule.graphqlProfileQueries.getProfiles).mockResolvedValue({
        profiles: { invalid: 'shape' },
      });

      await expect(profileService.getAll()).rejects.toThrow('Invalid response shape');
      expect(loggerModule.logger.error).toHaveBeenCalled();
    });
  });

  describe('getById', () => {
    it('returns parsed profile', async () => {
      vi.mocked(graphqlProfileQueriesModule.graphqlProfileQueries.getProfileById).mockResolvedValue({
        profileById: {
          profileId: '12345678-1234-1234-1234-123456789012',
          tenantId: '12345678-1234-1234-1234-123456789012',
          tenantCode: 'TENANT',
          tenantName: 'Tenant',
          userId: '12345678-1234-1234-1234-123456789012',
          userEmail: 'user@example.com',
          roleId: '12345678-1234-1234-1234-123456789012',
          roleCode: 'ADMIN',
          roleName: 'Admin',
          scope: 'Tenant',
          isActive: true,
          permissionCount: 0,
          permissions: [],
          systemSuiteId: '12345678-1234-1234-1234-123456789012',
          systemSuiteCode: 'CORE',
          systemSuiteName: 'Core',
        },
      });

      const result = await profileService.getById('p1');

      expect(result.profileId).toBe('12345678-1234-1234-1234-123456789012');
    });

    it('throws when not found', async () => {
      vi.mocked(graphqlProfileQueriesModule.graphqlProfileQueries.getProfileById).mockResolvedValue({
        profileById: null,
      });

      await expect(profileService.getById('nonexistent')).rejects.toThrow('Profile not found');
    });
  });

  describe('create', () => {
    it('calls POST and returns parsed response', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({
        data: { profileId: '12345678-1234-1234-1234-123456789012' },
      });

      const result = await profileService.create({ tenantId: '12345678-1234-1234-1234-123456789012', userId: '12345678-1234-1234-1234-123456789012', roleId: '12345678-1234-1234-1234-123456789012' });

      expect(result.profileId).toBe('12345678-1234-1234-1234-123456789012');
    });
  });

  describe('assignTemplate', () => {
    it('calls POST template endpoint', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({});

      await profileService.assignTemplate('p1', 't1');

      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith('/profiles/p1/templates/t1');
    });
  });

  describe('overridePermission', () => {
    it('calls POST override endpoint with allow effect', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({});

      await profileService.overridePermission('p1', 'perm1', 'allow');

      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith('/profiles/p1/permissions/perm1/override?effect=allow');
    });

    it('calls POST override endpoint with deny effect', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({});

      await profileService.overridePermission('p1', 'perm1', 'deny');

      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith('/profiles/p1/permissions/perm1/override?effect=deny');
    });
  });

  describe('activatePermission', () => {
    it('calls activate permission endpoint', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({});

      await profileService.activatePermission('p1', 'perm1');

      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith('/profiles/p1/permissions/perm1/activate');
    });
  });

  describe('deactivatePermission', () => {
    it('calls deactivate permission endpoint', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({});

      await profileService.deactivatePermission('p1', 'perm1');

      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith('/profiles/p1/permissions/perm1/deactivate');
    });
  });

  describe('activate', () => {
    it('calls activate endpoint', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({});

      await profileService.activate('p1');

      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith('/profiles/p1/activate');
    });
  });

  describe('deactivate', () => {
    it('calls deactivate endpoint', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({});

      await profileService.deactivate('p1');

      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith('/profiles/p1/deactivate');
    });
  });

  describe('exportGraph', () => {
    it('calls export endpoint with format', async () => {
      vi.mocked(httpClientModule.httpClient.get).mockResolvedValue({ data: 'exported data' });

      const result = await profileService.exportGraph('p1', 'json');

      expect(result).toBe('exported data');
      expect(httpClientModule.httpClient.get).toHaveBeenCalledWith('/profiles/p1/export', { params: { format: 'json' }, responseType: 'text' });
    });
  });
});
