import { describe, it, expect, vi, beforeEach } from 'vitest';
import { graphqlFeatureFlagQueries } from './feature-flag.graphql';
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

describe('graphqlFeatureFlagQueries', () => {
  beforeEach(() => {
    vi.mocked(graphqlClientModule.graphqlClient.request).mockClear();
  });

  it('getFeatureFlags calls graphqlClient.request', async () => {
    const mockResponse = {
      getFeatureFlags: {
        items: [],
        page: 1,
        pageSize: 20,
        totalItems: 0,
        totalPages: 0,
      },
    };

    vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue(mockResponse);

    const result = await graphqlFeatureFlagQueries.getFeatureFlags({ page: 1, pageSize: 20 });

    expect(result.getFeatureFlags.items).toHaveLength(0);
    expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(expect.any(String), expect.objectContaining({ page: 1, pageSize: 20 }));
  });

  it('getFeatureFlags passes optional params', async () => {
    vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({ getFeatureFlags: { items: [], page: 1, pageSize: 20, totalItems: 0, totalPages: 0 } });

    await graphqlFeatureFlagQueries.getFeatureFlags({ page: 1, pageSize: 20, search: 'test', status: 'Active' });

    expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(expect.any(String), expect.objectContaining({ search: 'test', status: 'Active' }));
  });

  it('getFeatureFlags throws on invalid page', async () => {
    await expect(graphqlFeatureFlagQueries.getFeatureFlags({ page: 0, pageSize: 20 })).rejects.toThrow('Invalid page parameter');
  });

  it('getFeatureFlags throws on invalid pageSize', async () => {
    await expect(graphqlFeatureFlagQueries.getFeatureFlags({ page: 1, pageSize: 0 })).rejects.toThrow('Invalid pageSize parameter');
  });

  it('getFeatureFlagById calls graphqlClient.request', async () => {
    const mockResponse = {
      getFeatureFlagById: {
        featureFlagId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        systemSuiteId: '3fa85f64-5717-4562-b3fc-2c963f66afa7',
        flagCode: 'FLAG_1',
        flagType: 'Boolean',
        flagTargets: 'all',
        status: 'Active',
        rolloutPercentage: null,
        criteria: [],
      },
    };

    vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue(mockResponse);

    const result = await graphqlFeatureFlagQueries.getFeatureFlagById('3fa85f64-5717-4562-b3fc-2c963f66afa6');

    expect(result.getFeatureFlagById?.flagCode).toBe('FLAG_1');
    expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(expect.any(String), { id: '3fa85f64-5717-4562-b3fc-2c963f66afa6' });
  });

  it('getFeatureFlagById throws on empty id', async () => {
    await expect(graphqlFeatureFlagQueries.getFeatureFlagById('')).rejects.toThrow('Invalid id parameter');
  });
});
