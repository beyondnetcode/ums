/**
 * tenant.service.ts
 *
 * Infrastructure service for the Identity / Tenant bounded context.
 * Queries use GraphQL via graphqlClient.
 * Commands/transactions use REST via httpClient.
 * All responses are validated at runtime with Zod before returning.
 */
import { httpClient } from '@infra/http/httpClient';
import { graphqlQueries } from '@infra/identity/queries/tenant.graphql';
import { logger } from '@app/utils/logger';
import {
  TenantPageSchema,
  TenantSchema,
  CreateTenantResponseSchema,
  type Tenant,
  type TenantPage,
  type CreateTenantPayload,
  type CreateTenantResponse,
} from '@domain/identity/schemas/tenant.schema';
import {
  BranchListSchema,
  AddBranchResponseSchema,
  type Branch,
  type AddBranchPayload,
  type AddBranchResponse,
} from '@domain/identity/schemas/branch.schema';

export const tenantService = {
  // ── Queries (GraphQL) ─────────────────────────────────────────────────────

  getAllTenants: async (params?: {
    page?: number;
    pageSize?: number;
    search?: string;
    criteria?: string;
    status?: string;
    sortBy?: string;
    sortOrder?: string;
  }): Promise<TenantPage> => {
    const response = await graphqlQueries.getTenants({
      page: params?.page ?? 1,
      pageSize: params?.pageSize ?? 20,
      search: params?.search,
      criteria: params?.criteria,
      status: params?.status,
      sortBy: params?.sortBy,
      sortOrder: params?.sortOrder,
    });

    const pageResult = TenantPageSchema.safeParse(response.tenants);
    if (!pageResult.success) {
      logger.error('Invalid GraphQL response shape for tenants query', pageResult.error);
      throw new Error('Invalid GraphQL response shape for tenants query');
    }
    return pageResult.data;
  },

  getTenantById: async (tenantId: string): Promise<Tenant> => {
    const response = await graphqlQueries.getTenantById(tenantId);
    if (!response.tenantById) throw new Error('Tenant not found');
    return TenantSchema.parse(response.tenantById);
  },

  getBranches: async (tenantId: string): Promise<Branch[]> => {
    const response = await graphqlQueries.getTenantBranches(tenantId);
    return BranchListSchema.parse(response.tenantBranches);
  },

  // ── Commands (REST) ───────────────────────────────────────────────────────

  createTenant: async (payload: CreateTenantPayload): Promise<CreateTenantResponse> => {
    const { data } = await httpClient.post('/tenants', payload);
    return CreateTenantResponseSchema.parse(data);
  },

  activateTenant: async (tenantId: string): Promise<void> => {
    await httpClient.post(`/tenants/${tenantId}/activate`);
  },

  suspendTenant: async (tenantId: string): Promise<void> => {
    await httpClient.post(`/tenants/${tenantId}/suspend`);
  },

  addBranch: async (tenantId: string, payload: AddBranchPayload): Promise<AddBranchResponse> => {
    const { data } = await httpClient.post(`/tenants/${tenantId}/branches`, payload);
    return AddBranchResponseSchema.parse(data);
  },

  removeBranch: async (tenantId: string, branchId: string): Promise<void> => {
    await httpClient.delete(`/tenants/${tenantId}/branches/${branchId}`);
  },

  deactivateBranch: async (tenantId: string, branchId: string): Promise<void> => {
    await httpClient.post(`/tenants/${tenantId}/branches/${branchId}/deactivate`);
  },

  reactivateBranch: async (tenantId: string, branchId: string): Promise<void> => {
    await httpClient.post(`/tenants/${tenantId}/branches/${branchId}/reactivate`);
  },
};

export default tenantService;
