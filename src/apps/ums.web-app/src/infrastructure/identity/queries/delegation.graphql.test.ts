import { describe, it, expect, vi, beforeEach } from 'vitest';
import { graphqlDelegationQueries } from './delegation.graphql';
import * as graphqlClientModule from '@infra/http/graphqlClient';

vi.mock('@infra/http/graphqlClient', () => ({
  graphqlClient: {
    request: vi.fn(),
  },
  GraphQlValidationError: class GraphQlValidationError extends Error {
    constructor(
      message: string,
      public errors: string[]
    ) {
      super(message);
      this.name = 'GraphQlValidationError';
    }
  },
}));

describe('graphqlDelegationQueries', () => {
  beforeEach(() => {
    vi.mocked(graphqlClientModule.graphqlClient.request).mockClear();
  });

  it('getDelegationById calls graphqlClient.request', async () => {
    const mockResponse = {
      getDelegationById: {
        delegationId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa7',
        delegatingAdminId: '3fa85f64-5717-4562-b3fc-2c963f66afa8',
        delegatedAdminId: '3fa85f64-5717-4562-b3fc-2c963f66afa9',
        scopeType: 'Tenant',
        scopeId: null,
        allowedActions: ['read', 'write'],
        validFrom: '2024-01-01',
        validUntil: '2024-12-31',
        maxDurationDays: null,
        requiresApproval: false,
        approvalRequestId: null,
        status: 'Active',
        revokedAt: null,
        revokedBy: null,
        revocationReason: null,
      },
    };

    vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue(mockResponse);

    const result = await graphqlDelegationQueries.getDelegationById(
      '3fa85f64-5717-4562-b3fc-2c963f66afa6'
    );

    expect(result.getDelegationById?.delegationId).toBe('3fa85f64-5717-4562-b3fc-2c963f66afa6');
    expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(expect.any(String), {
      delegationId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    });
  });

  it('getDelegationById throws on empty delegationId', async () => {
    await expect(graphqlDelegationQueries.getDelegationById('')).rejects.toThrow(
      'Invalid delegationId parameter'
    );
  });

  it('getDelegationsByDelegatedAdmin calls graphqlClient.request', async () => {
    vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
      getDelegationsByDelegatedAdmin: [],
    });

    const result = await graphqlDelegationQueries.getDelegationsByDelegatedAdmin(
      'admin1',
      'tenant1'
    );

    expect(result.getDelegationsByDelegatedAdmin).toEqual([]);
    expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(expect.any(String), {
      delegatedAdminId: 'admin1',
      tenantId: 'tenant1',
    });
  });

  it('getDelegationsByDelegatedAdmin throws on missing params', async () => {
    await expect(
      graphqlDelegationQueries.getDelegationsByDelegatedAdmin('', 'tenant1')
    ).rejects.toThrow('Invalid parameters');
  });

  it('getDelegationsByDelegatingAdmin calls graphqlClient.request', async () => {
    vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({
      getDelegationsByDelegatingAdmin: [],
    });

    const result = await graphqlDelegationQueries.getDelegationsByDelegatingAdmin(
      'admin1',
      'tenant1'
    );

    expect(result.getDelegationsByDelegatingAdmin).toEqual([]);
    expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(expect.any(String), {
      delegatingAdminId: 'admin1',
      tenantId: 'tenant1',
    });
  });

  it('getDelegationsByDelegatingAdmin throws on missing params', async () => {
    await expect(
      graphqlDelegationQueries.getDelegationsByDelegatingAdmin('', 'tenant1')
    ).rejects.toThrow('Invalid parameters');
  });
});
