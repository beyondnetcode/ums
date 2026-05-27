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

  // ── Menu Lifecycle REST Commands ──────────────────────────────────────────

  addMenu: async (
    systemSuiteId: string,
    moduleId: string,
    payload: { code: string; label: string; description?: string; sortOrder: number },
  ): Promise<void> => {
    await httpClient.post(`/system-suites/${systemSuiteId}/modules/${moduleId}/menus`, {
      ...payload,
      description: payload.description?.trim() ?? '',
    });
  },

  updateMenu: async (
    systemSuiteId: string,
    moduleId: string,
    menuId: string,
    payload: { label: string; description?: string; sortOrder: number },
  ): Promise<void> => {
    await httpClient.put(`/system-suites/${systemSuiteId}/modules/${moduleId}/menus/${menuId}`, {
      ...payload,
      description: payload.description?.trim() ?? '',
    });
  },

  removeMenu: async (systemSuiteId: string, moduleId: string, menuId: string): Promise<void> => {
    await httpClient.delete(`/system-suites/${systemSuiteId}/modules/${moduleId}/menus/${menuId}`);
  },

  // ── SubMenu Lifecycle REST Commands ──────────────────────────────────────

  addSubMenu: async (
    systemSuiteId: string,
    moduleId: string,
    menuId: string,
    payload: { code: string; label: string; description?: string; sortOrder: number },
  ): Promise<void> => {
    await httpClient.post(`/system-suites/${systemSuiteId}/modules/${moduleId}/menus/${menuId}/submenus`, {
      ...payload,
      description: payload.description?.trim() ?? '',
    });
  },

  updateSubMenu: async (
    systemSuiteId: string,
    moduleId: string,
    menuId: string,
    subMenuId: string,
    payload: { label: string; description?: string; sortOrder: number },
  ): Promise<void> => {
    await httpClient.put(
      `/system-suites/${systemSuiteId}/modules/${moduleId}/menus/${menuId}/submenus/${subMenuId}`,
      { ...payload, description: payload.description?.trim() ?? '' },
    );
  },

  removeSubMenu: async (systemSuiteId: string, moduleId: string, menuId: string, subMenuId: string): Promise<void> => {
    await httpClient.delete(`/system-suites/${systemSuiteId}/modules/${moduleId}/menus/${menuId}/submenus/${subMenuId}`);
  },

  // ── Option Lifecycle REST Commands ────────────────────────────────────────

  addOption: async (
    systemSuiteId: string,
    moduleId: string,
    menuId: string,
    subMenuId: string,
    payload: { code: string; label: string; description?: string; actionCode: string; sortOrder: number },
  ): Promise<void> => {
    await httpClient.post(
      `/system-suites/${systemSuiteId}/modules/${moduleId}/menus/${menuId}/submenus/${subMenuId}/options`,
      { ...payload, description: payload.description?.trim() ?? '' },
    );
  },

  updateOption: async (
    systemSuiteId: string,
    moduleId: string,
    menuId: string,
    subMenuId: string,
    optionId: string,
    payload: { label: string; description?: string; actionCode: string; sortOrder: number },
  ): Promise<void> => {
    await httpClient.put(
      `/system-suites/${systemSuiteId}/modules/${moduleId}/menus/${menuId}/submenus/${subMenuId}/options/${optionId}`,
      { ...payload, description: payload.description?.trim() ?? '' },
    );
  },

  removeOption: async (systemSuiteId: string, moduleId: string, menuId: string, subMenuId: string, optionId: string): Promise<void> => {
    await httpClient.delete(
      `/system-suites/${systemSuiteId}/modules/${moduleId}/menus/${menuId}/submenus/${subMenuId}/options/${optionId}`,
    );
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

  // ── Domain Resources REST Commands ────────────────────────────────────────

  addDomainResource: async (
    systemSuiteId: string,
    payload: { moduleId?: string | null; type: 'Aggregate' | 'Entity'; code: string; name: string; description: string; },
  ): Promise<void> => {
    await httpClient.post(`/system-suites/${systemSuiteId}/domain-resources`, payload);
  },

  updateDomainResource: async (
    systemSuiteId: string,
    domainResourceId: string,
    payload: { name: string; description: string; },
  ): Promise<void> => {
    await httpClient.put(`/system-suites/${systemSuiteId}/domain-resources/${domainResourceId}`, payload);
  },

  removeDomainResource: async (systemSuiteId: string, domainResourceId: string): Promise<void> => {
    await httpClient.delete(`/system-suites/${systemSuiteId}/domain-resources/${domainResourceId}`);
  },
};

export default systemSuiteService;
