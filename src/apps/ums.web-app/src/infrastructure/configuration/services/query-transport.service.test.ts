import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { queryTransportService } from './query-transport.service';
import * as httpClientModule from '@infra/http/httpClient';
import * as loggerModule from '@app/utils/logger';

vi.mock('@infra/http/httpClient', () => ({
  httpClient: {
    get: vi.fn(),
  },
}));

vi.mock('@app/utils/logger', () => ({
  logger: {
    warn: vi.fn(),
  },
}));

describe('queryTransportService', () => {
  beforeEach(() => {
    vi.mocked(httpClientModule.httpClient.get).mockClear();
    vi.mocked(loggerModule.logger.warn).mockClear();
    queryTransportService.resetCache();
  });

  afterEach(() => {
    queryTransportService.resetCache();
  });

  describe('getQueryTransport', () => {
    it('returns rest when backend resolves to REST', async () => {
      vi.mocked(httpClientModule.httpClient.get).mockResolvedValue({
        data: { code: 'FRONTEND_CONFIG_TRANSPORT', effectiveValue: 'rest' },
      });

      const result = await queryTransportService.getQueryTransport();

      expect(result).toBe('rest');
    });

    it('returns graphql when backend resolves to graphql', async () => {
      vi.mocked(httpClientModule.httpClient.get).mockResolvedValue({
        data: { code: 'FRONTEND_CONFIG_TRANSPORT', effectiveValue: 'graphql' },
      });

      const result = await queryTransportService.getQueryTransport();

      expect(result).toBe('graphql');
    });

    it('treats graphql value case-insensitively', async () => {
      vi.mocked(httpClientModule.httpClient.get).mockResolvedValue({
        data: { code: 'FRONTEND_CONFIG_TRANSPORT', effectiveValue: 'GraphQL' },
      });

      const result = await queryTransportService.getQueryTransport();

      expect(result).toBe('graphql');
    });

    it('falls back to rest when backend request fails', async () => {
      vi.mocked(httpClientModule.httpClient.get).mockRejectedValue(new Error('Network error'));

      const result = await queryTransportService.getQueryTransport();

      expect(result).toBe('rest');
      expect(loggerModule.logger.warn).toHaveBeenCalled();
    });

    it('falls back to rest for unknown transport values', async () => {
      vi.mocked(httpClientModule.httpClient.get).mockResolvedValue({
        data: { code: 'FRONTEND_CONFIG_TRANSPORT', effectiveValue: 'kafka' },
      });

      const result = await queryTransportService.getQueryTransport();

      expect(result).toBe('rest');
    });

    it('caches the resolved transport and does not re-fetch within TTL', async () => {
      vi.mocked(httpClientModule.httpClient.get).mockResolvedValue({
        data: { code: 'FRONTEND_CONFIG_TRANSPORT', effectiveValue: 'graphql' },
      });

      await queryTransportService.getQueryTransport();
      await queryTransportService.getQueryTransport();

      expect(httpClientModule.httpClient.get).toHaveBeenCalledTimes(1);
    });

    it('deduplicates concurrent requests into a single backend call', async () => {
      vi.mocked(httpClientModule.httpClient.get).mockResolvedValue({
        data: { code: 'FRONTEND_CONFIG_TRANSPORT', effectiveValue: 'graphql' },
      });

      const [first, second] = await Promise.all([
        queryTransportService.getQueryTransport(),
        queryTransportService.getQueryTransport(),
      ]);

      expect(first).toBe('graphql');
      expect(second).toBe('graphql');
      expect(httpClientModule.httpClient.get).toHaveBeenCalledTimes(1);
    });

    it('re-fetches after resetCache', async () => {
      vi.mocked(httpClientModule.httpClient.get)
        .mockResolvedValueOnce({
          data: { code: 'FRONTEND_CONFIG_TRANSPORT', effectiveValue: 'graphql' },
        })
        .mockResolvedValueOnce({
          data: { code: 'FRONTEND_CONFIG_TRANSPORT', effectiveValue: 'rest' },
        });

      const first = await queryTransportService.getQueryTransport();
      queryTransportService.resetCache();
      const second = await queryTransportService.getQueryTransport();

      expect(first).toBe('graphql');
      expect(second).toBe('rest');
      expect(httpClientModule.httpClient.get).toHaveBeenCalledTimes(2);
    });
  });
});
