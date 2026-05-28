/**
 * permission-template.service.ts
 *
 * Queries  → GraphQL via graphqlPermissionTemplateQueries
 * Commands → REST via httpClient
 */
import { httpClient } from '@infra/http/httpClient';
import { graphqlPermissionTemplateQueries } from '@infra/authorization/queries/permission-template.graphql';
import { logger } from '@app/utils/logger';
import {
  PermissionTemplatePageSchema,
  PermissionTemplateDetailSchema,
  CreatePermissionTemplateResponseSchema,
  type PermissionTemplatePage,
  type PermissionTemplateDetail,
  type CreatePermissionTemplatePayload,
  type CreatePermissionTemplateResponse,
  type AddTemplateItemPayload,
} from '@domain/authorization/schemas/permission-template.schema';

export const permissionTemplateService = {
  // ── Queries (GraphQL) ────────────────────────────────────────────────────────

  getAll: async (params?: {
    page?: number;
    pageSize?: number;
    search?: string;
    criteria?: string;
    status?: string;
    sortBy?: string;
    sortOrder?: string;
    tenantId?: string;
    systemSuiteId?: string;
    roleId?: string;
  }): Promise<PermissionTemplatePage> => {
    const response = await graphqlPermissionTemplateQueries.getPermissionTemplates({
      page: params?.page ?? 1,
      pageSize: params?.pageSize ?? 20,
      search: params?.search,
      criteria: params?.criteria,
      status: params?.status,
      sortBy: params?.sortBy,
      sortOrder: params?.sortOrder,
      tenantId: params?.tenantId,
      systemSuiteId: params?.systemSuiteId,
      roleId: params?.roleId,
    });

    const result = PermissionTemplatePageSchema.safeParse(
      (response as Record<string, unknown>).getPermissionTemplates,
    );
    if (!result.success) {
      logger.error('Invalid GraphQL response for permission templates', result.error);
      throw new Error('Invalid response shape for permission templates');
    }
    return result.data;
  },

  getById: async (templateId: string): Promise<PermissionTemplateDetail> => {
    const response = await graphqlPermissionTemplateQueries.getPermissionTemplateById(templateId);
    const raw = (response as Record<string, unknown>).getPermissionTemplateById;
    if (!raw) throw new Error('Permission template not found');
    return PermissionTemplateDetailSchema.parse(raw);
  },

  // ── Commands (REST) ──────────────────────────────────────────────────────────

  create: async (payload: CreatePermissionTemplatePayload): Promise<CreatePermissionTemplateResponse> => {
    const { data } = await httpClient.post('/permission-templates', payload);
    return CreatePermissionTemplateResponseSchema.parse(data);
  },

  publish: async (templateId: string): Promise<void> => {
    await httpClient.post(`/permission-templates/${templateId}/publish`);
  },

  deprecate: async (templateId: string): Promise<void> => {
    await httpClient.post(`/permission-templates/${templateId}/deprecate`);
  },

  delete: async (templateId: string): Promise<void> => {
    await httpClient.delete(`/permission-templates/${templateId}`);
  },

  addItem: async (templateId: string, payload: AddTemplateItemPayload): Promise<void> => {
    await httpClient.post(`/permission-templates/${templateId}/items`, payload);
  },

  removeItem: async (templateId: string, itemId: string): Promise<void> => {
    await httpClient.delete(`/permission-templates/${templateId}/items/${itemId}`);
  },

  setItemEffect: async (templateId: string, itemId: string, effect: 'Allow' | 'Deny' | 'Neutral'): Promise<void> => {
    await httpClient.put(`/permission-templates/${templateId}/items/${itemId}/effect`, { effect });
  },

  activateItem: async (templateId: string, itemId: string): Promise<void> => {
    await httpClient.post(`/permission-templates/${templateId}/items/${itemId}/activate`);
  },

  deactivateItem: async (templateId: string, itemId: string): Promise<void> => {
    await httpClient.post(`/permission-templates/${templateId}/items/${itemId}/deactivate`);
  },
};

export default permissionTemplateService;
