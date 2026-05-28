import { describe, it, expect, vi, beforeEach } from 'vitest';
import { graphqlProfileQueries } from './profile.graphql';
import * as graphqlClientModule from '@infra/http/graphqlClient';

vi.mock('@infra/http/graphqlClient', () => ({
  graphqlClient: {
    request: vi.fn(),
  },
}));

describe('graphqlProfileQueries', () => {
  beforeEach(() => {
    vi.mocked(graphqlClientModule.graphqlClient.request).mockClear();
  });

  describe('getProfiles', () => {
    it('calls graphqlClient with default values', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({ getProfiles: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 } });

      await graphqlProfileQueries.getProfiles({ page: 1, pageSize: 10 });

      expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(
        expect.stringContaining('query GetProfiles'),
        expect.objectContaining({
          page: 1,
          pageSize: 10,
          criteria: 'userId',
          status: 'all',
          sortBy: 'userId',
          sortOrder: 'asc',
        }),
      );
    });

    it('includes optional search parameter', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({ getProfiles: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 } });

      await graphqlProfileQueries.getProfiles({ page: 1, pageSize: 10, search: 'test' });

      const callArgs = vi.mocked(graphqlClientModule.graphqlClient.request).mock.calls[0];
      expect(callArgs[1]).toHaveProperty('search', 'test');
    });

    it('includes optional tenantId parameter', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({ getProfiles: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 } });

      await graphqlProfileQueries.getProfiles({ page: 1, pageSize: 10, tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa6' });

      const callArgs = vi.mocked(graphqlClientModule.graphqlClient.request).mock.calls[0];
      expect(callArgs[1]).toHaveProperty('tenantId', '3fa85f64-5717-4562-b3fc-2c963f66afa6');
    });

    it('includes optional userId parameter', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({ getProfiles: { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 } });

      await graphqlProfileQueries.getProfiles({ page: 1, pageSize: 10, userId: '3fa85f64-5717-4562-b3fc-2c963f66afa7' });

      const callArgs = vi.mocked(graphqlClientModule.graphqlClient.request).mock.calls[0];
      expect(callArgs[1]).toHaveProperty('userId', '3fa85f64-5717-4562-b3fc-2c963f66afa7');
    });
  });

  describe('getProfileById', () => {
    it('calls graphqlClient with profileId', async () => {
      vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue({ getProfileById: null });

      await graphqlProfileQueries.getProfileById('3fa85f64-5717-4562-b3fc-2c963f66afa6');

      expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(
        expect.stringContaining('query GetProfileById'),
        expect.objectContaining({ profileId: '3fa85f64-5717-4562-b3fc-2c963f66afa6' }),
      );
    });
  });
});
