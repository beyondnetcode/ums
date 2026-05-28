import { describe, it, expect, vi, beforeEach } from 'vitest';
import { graphqlQueries } from './user-account.graphql';
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

describe('graphqlQueries (user-account)', () => {
  beforeEach(() => {
    vi.mocked(graphqlClientModule.graphqlClient.request).mockClear();
  });

  describe('getUserAccounts', () => {
    it('calls graphqlClient with correct query and variables', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
        getUserAccounts: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 },
      });

      await graphqlQueries.getUserAccounts({ page: 1, pageSize: 10 });

      expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(
        expect.stringContaining('query UserAccounts'),
        expect.objectContaining({ page: 1, pageSize: 10 }),
      );
    });

    it('includes optional search parameter', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
        getUserAccounts: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 },
      });

      await graphqlQueries.getUserAccounts({ page: 1, pageSize: 10, search: 'test' });

      const callArgs = vi.mocked(graphqlClientModule.graphqlClient.request).mock.calls[0];
      expect(callArgs[1]).toHaveProperty('search', 'test');
    });

    it('includes optional criteria parameter', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
        getUserAccounts: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 },
      });

      await graphqlQueries.getUserAccounts({ page: 1, pageSize: 10, criteria: 'email' });

      const callArgs = vi.mocked(graphqlClientModule.graphqlClient.request).mock.calls[0];
      expect(callArgs[1]).toHaveProperty('criteria', 'email');
    });

    it('includes optional status parameter', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
        getUserAccounts: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 },
      });

      await graphqlQueries.getUserAccounts({ page: 1, pageSize: 10, status: 'Active' });

      const callArgs = vi.mocked(graphqlClientModule.graphqlClient.request).mock.calls[0];
      expect(callArgs[1]).toHaveProperty('status', 'Active');
    });

    it('includes optional sortBy parameter', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
        getUserAccounts: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 },
      });

      await graphqlQueries.getUserAccounts({ page: 1, pageSize: 10, sortBy: 'email' });

      const callArgs = vi.mocked(graphqlClientModule.graphqlClient.request).mock.calls[0];
      expect(callArgs[1]).toHaveProperty('sortBy', 'email');
    });

    it('includes optional tenantId parameter', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
        getUserAccounts: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 },
      });

      await graphqlQueries.getUserAccounts({ page: 1, pageSize: 10, tenantId: 't-1' });

      const callArgs = vi.mocked(graphqlClientModule.graphqlClient.request).mock.calls[0];
      expect(callArgs[1]).toHaveProperty('tenantId', 't-1');
    });

    it('throws on invalid page', async () => {
      await expect(graphqlQueries.getUserAccounts({ page: 0, pageSize: 10 })).rejects.toThrow(GraphQlValidationError);
    });

    it('throws on invalid pageSize', async () => {
      await expect(graphqlQueries.getUserAccounts({ page: 1, pageSize: 0 })).rejects.toThrow(GraphQlValidationError);
    });
  });

  describe('getUserAccountById', () => {
    it('calls graphqlClient with userAccountId', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
        getUserAccountById: { userAccountId: 'u-1', email: 'test@test.com', category: 'Internal', status: 'Active', tenantId: 't-1', branchId: null, identityReference: null, identityReferenceType: null, hasActivePassword: true, passwordUpdatedAtUtc: null },
      });

      await graphqlQueries.getUserAccountById('3fa85f64-5717-4562-b3fc-2c963f66afa6');

      expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(
        expect.stringContaining('query UserAccount'),
        expect.objectContaining({ userAccountId: '3fa85f64-5717-4562-b3fc-2c963f66afa6' }),
      );
    });

    it('throws on empty userAccountId', async () => {
      await expect(graphqlQueries.getUserAccountById('')).rejects.toThrow(GraphQlValidationError);
    });

    it('throws on whitespace userAccountId', async () => {
      await expect(graphqlQueries.getUserAccountById('   ')).rejects.toThrow(GraphQlValidationError);
    });
  });
});
