import { describe, it, expect, vi, beforeEach } from 'vitest';
import { brandingService } from './branding.service';
import * as httpClientModule from '@infra/http/httpClient';
import * as graphqlBrandingQueriesModule from '@infra/identity/queries/branding.graphql';

vi.mock('@infra/http/httpClient', () => ({
  httpClient: {
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

vi.mock('@infra/identity/queries/branding.graphql', () => ({
  graphqlBrandingQueries: {
    getBranding: vi.fn(),
  },
}));

describe('brandingService', () => {
  beforeEach(() => {
    vi.mocked(httpClientModule.httpClient.post).mockClear();
    vi.mocked(httpClientModule.httpClient.put).mockClear();
    vi.mocked(httpClientModule.httpClient.delete).mockClear();
    vi.mocked(graphqlBrandingQueriesModule.graphqlBrandingQueries.getBranding).mockClear();
  });

  describe('getBranding', () => {
    it('returns null when no branding exists', async () => {
      vi.mocked(graphqlBrandingQueriesModule.graphqlBrandingQueries.getBranding).mockResolvedValue({
        getTenantBranding: null,
      });

      const result = await brandingService.getBranding('3fa85f64-5717-4562-b3fc-2c963f66afa6');

      expect(result).toBeNull();
    });

    it('returns parsed branding when available', async () => {
      vi.mocked(graphqlBrandingQueriesModule.graphqlBrandingQueries.getBranding).mockResolvedValue({
        getTenantBranding: {
          logo: 'data:image/png;base64,...',
          logoFormat: 'png',
          primaryColor: '#3b82f6',
          backgroundStyle: 'solid',
          headlineText: 'Welcome',
          secondaryText: 'Login to continue',
          primaryButtonLabel: 'Sign In',
          footerText: 'Powered by UMS',
          customDomain: null,
          magicLinkFallbackEnabled: false,
          dnsVerificationStatus: 'pending',
        },
      });

      const result = await brandingService.getBranding('3fa85f64-5717-4562-b3fc-2c963f66afa6');

      expect(result).not.toBeNull();
      expect(result?.headlineText).toBe('Welcome');
    });
  });

  describe('setBranding', () => {
    it('calls httpClient.post with correct payload', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({});

      await brandingService.setBranding('3fa85f64-5717-4562-b3fc-2c963f66afa6', {
        logo: 'data:image/png;base64,...',
        logoFormat: 'png',
        primaryColor: '#3b82f6',
        backgroundStyle: 'solid',
        headlineText: 'Welcome',
        secondaryText: 'Login to continue',
        primaryButtonLabel: 'Sign In',
        footerText: 'Powered by UMS',
        magicLinkFallbackEnabled: false,
      });

      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith(
        '/tenants/3fa85f64-5717-4562-b3fc-2c963f66afa6/branding',
        expect.objectContaining({
          headlineText: 'Welcome',
          primaryColor: '#3b82f6',
        }),
      );
    });
  });

  describe('updateBranding', () => {
    it('calls httpClient.put with correct payload', async () => {
      vi.mocked(httpClientModule.httpClient.put).mockResolvedValue({});

      await brandingService.updateBranding('3fa85f64-5717-4562-b3fc-2c963f66afa6', {
        logo: 'data:image/png;base64,...',
        logoFormat: 'png',
        primaryColor: '#3b82f6',
        backgroundStyle: 'solid',
        headlineText: 'Updated Welcome',
        secondaryText: 'Login to continue',
        primaryButtonLabel: 'Sign In',
        footerText: 'Powered by UMS',
        magicLinkFallbackEnabled: false,
      });

      expect(httpClientModule.httpClient.put).toHaveBeenCalledWith(
        '/tenants/3fa85f64-5717-4562-b3fc-2c963f66afa6/branding',
        expect.objectContaining({
          headlineText: 'Updated Welcome',
        }),
      );
    });
  });

  describe('removeBranding', () => {
    it('calls httpClient.delete', async () => {
      vi.mocked(httpClientModule.httpClient.delete).mockResolvedValue({});

      await brandingService.removeBranding('3fa85f64-5717-4562-b3fc-2c963f66afa6');

      expect(httpClientModule.httpClient.delete).toHaveBeenCalledWith(
        '/tenants/3fa85f64-5717-4562-b3fc-2c963f66afa6/branding',
      );
    });
  });

  describe('verifyDns', () => {
    it('calls verify endpoint', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({});

      await brandingService.verifyDns('3fa85f64-5717-4562-b3fc-2c963f66afa6');

      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith(
        '/tenants/3fa85f64-5717-4562-b3fc-2c963f66afa6/branding/dns/verify',
      );
    });
  });

  describe('failDns', () => {
    it('calls fail endpoint', async () => {
      vi.mocked(httpClientModule.httpClient.post).mockResolvedValue({});

      await brandingService.failDns('3fa85f64-5717-4562-b3fc-2c963f66afa6');

      expect(httpClientModule.httpClient.post).toHaveBeenCalledWith(
        '/tenants/3fa85f64-5717-4562-b3fc-2c963f66afa6/branding/dns/fail',
      );
    });
  });
});
