import { describe, it, expect, vi, beforeEach } from 'vitest';
import { graphqlPermissionTemplateQueries } from './permission-template.graphql';
import * as graphqlClientModule from '@infra/http/graphqlClient';

vi.mock('@infra/http/graphqlClient', () => ({
  graphqlClient: {
    request: vi.fn(),
  },
}));

describe('graphqlPermissionTemplateQueries', () => {
  beforeEach(() => {
    vi.mocked(graphqlClientModule.graphqlClient.request).mockClear();
  });

  it('getPermissionTemplates calls graphqlClient.request', async () => {
    const mockResponse = {
      getPermissionTemplates: {
        items: [],
        page: 1,
        pageSize: 20,
        totalItems: 0,
        totalPages: 0,
      },
    };

    vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue(mockResponse);

    const result = await graphqlPermissionTemplateQueries.getPermissionTemplates({ page: 1, pageSize: 20 });

    expect(result.getPermissionTemplates).toEqual(mockResponse.getPermissionTemplates);
    expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(expect.any(String), expect.objectContaining({ page: 1, pageSize: 20 }));
  });

  it('getPermissionTemplateById calls graphqlClient.request', async () => {
    const mockResponse = {
      getPermissionTemplateById: {
        templateId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa7',
        roleId: '3fa85f64-5717-4562-b3fc-2c963f66afa8',
        roleName: 'Admin',
        systemSuiteId: '3fa85f64-5717-4562-b3fc-2c963f66afa9',
        systemSuiteName: 'Suite 1',
        version: '1.0',
        status: 'Draft',
        items: [],
      },
    };

    vi.mocked(graphqlClientModule.graphqlClient.request).mockResolvedValue(mockResponse);

    const result = await graphqlPermissionTemplateQueries.getPermissionTemplateById('3fa85f64-5717-4562-b3fc-2c963f66afa6');

    expect(result.getPermissionTemplateById?.templateId).toBe('3fa85f64-5717-4562-b3fc-2c963f66afa6');
    expect(graphqlClientModule.graphqlClient.request).toHaveBeenCalledWith(expect.any(String), { templateId: '3fa85f64-5717-4562-b3fc-2c963f66afa6' });
  });
});
