import { describe, it, expect, vi, beforeEach } from 'vitest';
import { graphqlBrandingQueries } from './branding.graphql';
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

describe('graphqlBrandingQueries', () => {
  beforeEach(() => {
    vi.mocked(graphqlClientModule.graphqlClient.request).mockClear();
  });

  it('getBranding calls graphqlClient.request', async () => {
    const mockResponse = {
      getTenantBranding: {
        logo: 'logo.png',
        logoFormat: 'png',
        primaryColor: '#3b5bdb',
        backgroundStyle: 'solid',
        headlineText: 'Welcome',
        secondaryText: 'Sign in',
        primaryButtonLabel: 'Login',
        footerText: 'Footer',
        customDomain: null,
        magicLinkFallbackEnabled: false,
        dnsVerificationStatus: 'Pending',
      },
    };

    vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue(mockResponse);

    const result = await graphqlBrandingQueries.getBranding('3fa85f64-5717-4562-b3fc-2c963f66afa7');

    expect(result.getTenantBranding?.headlineText).toBe('Welcome');
    expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(expect.any(String), {
      tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa7',
    });
  });

  it('getBranding throws on empty tenantId', async () => {
    await expect(graphqlBrandingQueries.getBranding('')).rejects.toThrow(
      'Invalid tenantId parameter'
    );
  });
});
