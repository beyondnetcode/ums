import { describe, it, expect, vi, beforeEach } from 'vitest';
import { graphqlSystemSuiteQueries } from './system-suite.graphql';
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

describe('graphqlSystemSuiteQueries', () => {
  beforeEach(() => {
    vi.mocked(graphqlClientModule.graphqlClient.request).mockClear();
  });

  it('getSystemSuites calls graphqlClient.request', async () => {
    const mockResponse = {
      getSystemSuites: {
        items: [],
        page: 1,
        pageSize: 20,
        totalItems: 0,
        totalPages: 0,
      },
    };

    vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue(mockResponse);

    const result = await graphqlSystemSuiteQueries.getSystemSuites({ page: 1, pageSize: 20 });

    expect(result.getSystemSuites.items).toHaveLength(0);
    expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(expect.any(String), expect.objectContaining({ page: 1, pageSize: 20 }));
  });

  it('getSystemSuiteById calls graphqlClient.request', async () => {
    const mockResponse = {
      getSystemSuiteById: {
        systemSuiteId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa7',
        code: 'S1',
        name: 'Suite 1',
        description: '',
        status: 'Active',
        modules: [],
        actions: [],
        domainResources: [],
      },
    };

    vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue(mockResponse);

    const result = await graphqlSystemSuiteQueries.getSystemSuiteById('3fa85f64-5717-4562-b3fc-2c963f66afa6');

    expect(result.getSystemSuiteById?.systemSuiteId).toBe('3fa85f64-5717-4562-b3fc-2c963f66afa6');
    expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(expect.any(String), { systemSuiteId: '3fa85f64-5717-4562-b3fc-2c963f66afa6' });
  });

  it('getSystemSuiteById throws on empty systemSuiteId', async () => {
    await expect(graphqlSystemSuiteQueries.getSystemSuiteById('')).rejects.toThrow('Invalid systemSuiteId parameter');
  });
});
