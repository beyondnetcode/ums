import { describe, it, expect, vi, beforeEach } from 'vitest';
import { delegationService } from './delegation.service';
import * as httpClientModule from '@infra/http/httpClient';
import * as graphqlDelegationQueriesModule from '@infra/identity/queries/delegation.graphql';

vi.mock('@infra/http/httpClient', () => ({
  httpClient: {
    post: vi.fn(),
  },
}));

vi.mock('@infra/identity/queries/delegation.graphql', () => ({
  graphqlDelegationQueries: {
    getDelegationById: vi.fn(),
    getDelegationsByDelegatedAdmin: vi.fn(),
    getDelegationsByDelegatingAdmin: vi.fn(),
  },
}));

describe('delegationService', () => {
  beforeEach(() => {
    vi.mocked(httpClientModule.httpClient.post).mockClear();
    vi.mocked(graphqlDelegationQueriesModule.graphqlDelegationQueries.getDelegationById).mockClear();
    vi.mocked(graphqlDelegationQueriesModule.graphqlDelegationQueries.getDelegationsByDelegatedAdmin).mockClear();
    vi.mocked(graphqlDelegationQueriesModule.graphqlDelegationQueries.getDelegationsByDelegatingAdmin).mockClear();
  });

  describe('getDelegationById', () => {
    it('throws when delegation not found', async () => {
      vi.mocked(graphqlDelegationQueriesModule.graphqlDelegationQueries.getDelegationById).mockResolvedValue({
        getDelegationById: null,
      });

      await expect(delegationService.getDelegationById('3fa85f64-5717-4562-b3fc-2c963f66afa6')).rejects.toThrow('Delegation not found');
    });

    it('returns parsed delegation when found', async () => {
      vi.mocked(graphqlDelegationQueriesModule.graphqlDelegationQueries.getDelegationById).mockResolvedValue({
        getDelegationById: {
          delegationId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
          tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa7',
          delegatingAdminId: '3fa85f64-5717-4562-b3fc-2c963f66afa8',
          delegatedAdminId: '3fa85f64-5717-4562-b3fc-2c963f66afa9',
          scopeType: 'Tenant',
          scopeId: null,
          allowedActions: ['CreateUser'],
          validFrom: '2024-01-01T00:00:00Z',
          validUntil: '2024-12-31T23:59:59Z',
          maxDurationDays: 30,
          requiresApproval: false,
          approvalRequestId: null,
          status: 'Active',
          revokedAt: null,
          revokedBy: null,
          revocationReason: null,
        },
      });

      const result = await delegationService.getDelegationById('3fa85f64-5717-4562-b3fc-2c963f66afa6');

      expect(result.delegationId).toBe('3fa85f64-5717-4562-b3fc-2c963f66afa6');
      expect(result.status).toBe('Active');
    });
  });

  describe('getDelegationsByDelegatedAdmin', () => {
    it('returns parsed delegations', async () => {
      vi.mocked(graphqlDelegationQueriesModule.graphqlDelegationQueries.getDelegationsByDelegatedAdmin).mockResolvedValue({
        getDelegationsByDelegatedAdmin: [],
      });

      const result = await delegationService.getDelegationsByDelegatedAdmin('3fa85f64-5717-4562-b3fc-2c963f66afa6', '3fa85f64-5717-4562-b3fc-2c963f66afa7');

      expect(result).toEqual([]);
    });
  });

  describe('getDelegationsByDelegatingAdmin', () => {
    it('returns parsed delegations', async () => {
      vi.mocked(graphqlDelegationQueriesModule.graphqlDelegationQueries.getDelegationsByDelegatingAdmin).mockResolvedValue({
        getDelegationsByDelegatingAdmin: [],
      });

      const result = await delegationService.getDelegationsByDelegatingAdmin('3fa85f64-5717-4562-b3fc-2c963f66afa6', '3fa85f64-5717-4562-b3fc-2c963f66afa7');

      expect(result).toEqual([]);
    });
  });

  describe('createDelegation', () => {
    it('calls httpClient.post and returns parsed response', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({
        data: { delegationId: '3fa85f64-5717-4562-b3fc-2c963f66afa6' },
      });

      const result = await delegationService.createDelegation({
        tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa7',
        delegatingAdminId: '3fa85f64-5717-4562-b3fc-2c963f66afa8',
        delegatedAdminId: '3fa85f64-5717-4562-b3fc-2c963f66afa9',
        scopeType: 'Tenant',
        allowedActions: ['CreateUser'],
        validFrom: '2024-01-01',
        validUntil: '2024-12-31',
        requiresApproval: false,
      });

      expect(result.delegationId).toBe('3fa85f64-5717-4562-b3fc-2c963f66afa6');
    });
  });

  describe('activateDelegation', () => {
    it('calls activate endpoint', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({});

      await delegationService.activateDelegation('3fa85f64-5717-4562-b3fc-2c963f66afa6');

      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith(
        '/delegations/3fa85f64-5717-4562-b3fc-2c963f66afa6/activate',
      );
    });
  });

  describe('revokeDelegation', () => {
    it('calls revoke endpoint with reason', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({});

      await delegationService.revokeDelegation('3fa85f64-5717-4562-b3fc-2c963f66afa6', 'No longer needed');

      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith(
        '/delegations/3fa85f64-5717-4562-b3fc-2c963f66afa6/revoke',
        null,
        { params: { reason: 'No longer needed' } },
      );
    });
  });

  describe('expireDelegation', () => {
    it('calls expire endpoint', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({});

      await delegationService.expireDelegation('3fa85f64-5717-4562-b3fc-2c963f66afa6');

      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith(
        '/delegations/3fa85f64-5717-4562-b3fc-2c963f66afa6/expire',
      );
    });
  });
});
