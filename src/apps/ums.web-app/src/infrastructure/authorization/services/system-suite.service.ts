/**
 * system-suite.service.ts
 *
 * Infrastructure service for the Authorization / SystemSuite bounded context.
 * Queries use GraphQL via graphqlClient.
 * Commands/transactions use REST via httpClient.
 * All responses are validated at runtime with Zod before returning.
 */
import { httpClient } from '@infra/http/httpClient';
import { graphqlSystemSuiteQueries } from '@infra/authorization/queries/system-suite.graphql';
import { logger } from '@app/utils/logger';
import {
  SystemSuitePageSchema,
  SystemSuiteSchema,
  CreateSystemSuiteResponseSchema,
  type SystemSuite,
  type SystemSuitePage,
  type CreateSystemSuitePayload,
  type CreateSystemSuiteResponse,
} from '@domain/authorization/schemas/system-suite.schema';

export const systemSuiteService = {
  // ── Queries (GraphQL) ─────────────────────────────────────────────────────

  getAllSystemSuites: async (params?: {
    page?: number;
    pageSize?: number;
    search?: string;
    criteria?: string;
    status?: string;
    sortBy?: string;
    sortOrder?: string;
    tenantId?: string;
  }): Promise<SystemSuitePage> => {
    const response = await graphqlSystemSuiteQueries.getSystemSuites({
      page: params?.page ?? 1,
      pageSize: params?.pageSize ?? 20,
      search: params?.search,
      criteria: params?.criteria,
      status: params?.status,
      sortBy: params?.sortBy,
      sortOrder: params?.sortOrder,
      tenantId: params?.tenantId,
    });

    const pageResult = SystemSuitePageSchema.safeParse(response.getSystemSuites);
    if (!pageResult.success) {
      logger.error('Invalid GraphQL response shape for system suites query', pageResult.error);
      throw new Error('Invalid GraphQL response shape for system suites query');
    }
    return pageResult.data;
  },

  getSystemSuiteById: async (systemSuiteId: string): Promise<SystemSuite> => {
    const response = await graphqlSystemSuiteQueries.getSystemSuiteById(systemSuiteId);
    if (!response.getSystemSuiteById) throw new Error('SystemSuite not found');
    return SystemSuiteSchema.parse(response.getSystemSuiteById);
  },

  // ── Commands (REST) ───────────────────────────────────────────────────────

  createSystemSuite: async (payload: CreateSystemSuitePayload): Promise<CreateSystemSuiteResponse> => {
    const { data } = await httpClient.post('/system-suites', payload);
    return CreateSystemSuiteResponseSchema.parse(data);
  },

  updateSystemSuite: async (systemSuiteId: string, name: string, description: string): Promise<void> => {
    await httpClient.put(`/system-suites/${systemSuiteId}`, { name, description });
  },

  setSystemSuiteStatus: async (systemSuiteId: string, status: string): Promise<void> => {
    await httpClient.post(`/system-suites/${systemSuiteId}/status`, undefined, { params: { status } });
  },

  // ── Module Lifecycle REST Commands ─────────────────────────────────────────

  addModule: async (
    systemSuiteId: string,
    payload: { code: string; name: string; description?: string; sortOrder: number },
  ): Promise<void> => {
    await httpClient.post(`/system-suites/${systemSuiteId}/modules`, {
      ...payload,
      description: payload.description?.trim() ?? '',
    });
  },

  updateModule: async (
    systemSuiteId: string,
    moduleId: string,
    payload: { name: string; description?: string; sortOrder: number },
  ): Promise<void> => {
    await httpClient.put(`/system-suites/${systemSuiteId}/modules/${moduleId}`, {
      ...payload,
      description: payload.description?.trim() ?? '',
    });
  },

  removeModule: async (systemSuiteId: string, moduleId: string): Promise<void> => {
    await httpClient.delete(`/system-suites/${systemSuiteId}/modules/${moduleId}`);
  },

  activateModule: async (systemSuiteId: string, moduleId: string): Promise<void> => {
    await httpClient.post(`/system-suites/${systemSuiteId}/modules/${moduleId}/activate`);
  },

  deactivateModule: async (systemSuiteId: string, moduleId: string): Promise<void> => {
    await httpClient.post(`/system-suites/${systemSuiteId}/modules/${moduleId}/deactivate`);
  },

  // ── Action Registry REST Commands ─────────────────────────────────────────

  registerAction: async (
    systemSuiteId: string,
    payload: { code: string; name: string },
  ): Promise<void> => {
    await httpClient.post(`/system-suites/${systemSuiteId}/actions`, payload);
  },

  removeAction: async (systemSuiteId: string, code: string): Promise<void> => {
    await httpClient.delete(`/system-suites/${systemSuiteId}/actions/${code}`);
  },
};

export default systemSuiteService;
