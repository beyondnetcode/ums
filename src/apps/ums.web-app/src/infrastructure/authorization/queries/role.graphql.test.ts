import { describe, it, expect, vi, beforeEach } from 'vitest';
import { graphqlRoleQueries } from './role.graphql';
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

describe('graphqlRoleQueries', () => {
  beforeEach(() => {
    vi.mocked(graphqlClientModule.graphqlClient.request).mockClear();
  });

  describe('getRolesBySystemSuite', () => {
    it('calls graphqlClient with systemSuiteId', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
        rolesBySystemSuite: [],
      });

      await graphqlRoleQueries.getRolesBySystemSuite('3fa85f64-5717-4562-b3fc-2c963f66afa6');

      expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(
        expect.stringContaining('query RolesBySystemSuite'),
        expect.objectContaining({ systemSuiteId: '3fa85f64-5717-4562-b3fc-2c963f66afa6' }),
      );
    });

    it('throws on empty systemSuiteId', async () => {
      await expect(graphqlRoleQueries.getRolesBySystemSuite('')).rejects.toThrow(GraphQlValidationError);
    });

    it('throws on whitespace systemSuiteId', async () => {
      await expect(graphqlRoleQueries.getRolesBySystemSuite('   ')).rejects.toThrow(GraphQlValidationError);
    });

    it('returns roles when successful', async () => {
      const mockRoles = [
        {
          roleId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
          tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa7',
          systemSuiteId: '3fa85f64-5717-4562-b3fc-2c963f66afa8',
          parentRoleId: null,
          code: 'ADMIN',
          value: 'Administrator',
          description: 'Admin role',
          hierarchyLevel: 0,
          promotionOrder: 1,
          isActive: true,
        },
      ];
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
        rolesBySystemSuite: mockRoles,
      });

      const result = await graphqlRoleQueries.getRolesBySystemSuite('3fa85f64-5717-4562-b3fc-2c963f66afa6');

      expect(result.rolesBySystemSuite).toHaveLength(1);
      expect(result.rolesBySystemSuite[0].code).toBe('ADMIN');
    });
  });
});
