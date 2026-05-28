import { describe, it, expect, vi, beforeEach } from 'vitest';
import { graphqlIdpQueries } from './idp.graphql';
import * as graphqlClientModule from '@infra/http/graphqlClient';

vi.mock('@infra/http/graphqlClient', () => ({
  graphqlClient: {
    request: vi.fn(),
  },
  GraphQlValidationError: class GraphQlValidationError extends Error {
    constructor(message: string, public errors: string[]) {
      super(message);
      this.name = 'GraphQlValidationError';
    }
  },
}));

describe('graphqlIdpQueries', () => {
  beforeEach(() => {
    vi.mocked(graphqlClientModule.graphqlClient.request).mockClear();
  });

  it('getIdentityProviders calls graphqlClient.request', async () => {
    const mockResponse = {
      getTenantIdentityProviders: [
        {
          identityProviderId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
          code: 'AZURE',
          name: 'Azure AD',
          description: 'Azure Active Directory',
          strategy: 'OIDC',
          isActive: true,
        },
      ],
    };

    vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue(mockResponse);

    const result = await graphqlIdpQueries.getIdentityProviders('3fa85f64-5717-4562-b3fc-2c963f66afa7');

    expect(result.getTenantIdentityProviders).toHaveLength(1);
    expect(result.getTenantIdentityProviders[0].name).toBe('Azure AD');
    expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(expect.any(String), { tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa7' });
  });

  it('getIdentityProviders throws on empty tenantId', async () => {
    await expect(graphqlIdpQueries.getIdentityProviders('')).rejects.toThrow('Invalid tenantId parameter');
  });

  it('getIdentityProviders throws on whitespace-only tenantId', async () => {
    await expect(graphqlIdpQueries.getIdentityProviders('   ')).rejects.toThrow('Invalid tenantId parameter');
  });
});
