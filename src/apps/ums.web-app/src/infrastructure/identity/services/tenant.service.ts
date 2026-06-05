/**
 * tenant.service.ts
 *
 * Infrastructure service for the Identity / Tenant bounded context.
 * Queries use the transport configured by FRONTEND_CONFIG_TRANSPORT.
 * Commands/transactions use REST via httpClient.
 * All responses are validated at runtime with Zod before returning.
 */
import { httpClient } from '@infra/http/httpClient';
import { queryTransportService } from '@infra/configuration/services/query-transport.service';
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

type TenantListParams = {
  page?: number;
  pageSize?: number;
  search?: string;
  criteria?: string;
  status?: string;
  sortBy?: string;
  sortOrder?: string;
};

function buildTenantQueryString(
  params: Required<Pick<TenantListParams, 'page' | 'pageSize'>> & TenantListParams
): string {
  const searchParams = new URLSearchParams();
  searchParams.set('page', String(params.page));
  searchParams.set('pageSize', String(params.pageSize));
  if (params.search !== undefined) searchParams.set('search', params.search);
  if (params.criteria !== undefined) searchParams.set('criteria', params.criteria);
  if (params.status !== undefined) searchParams.set('status', params.status);
  if (params.sortBy !== undefined) searchParams.set('sortBy', params.sortBy);
  if (params.sortOrder !== undefined) searchParams.set('sortOrder', params.sortOrder);
  return searchParams.toString();
}

async function getTenantsViaRest(
  params: Required<Pick<TenantListParams, 'page' | 'pageSize'>> & TenantListParams
): Promise<TenantPage> {
  const { data } = await httpClient.get<TenantPage>(`/tenants?${buildTenantQueryString(params)}`);
  const pageResult = TenantPageSchema.safeParse(data);
  if (!pageResult.success) {
    logger.error('Invalid REST response shape for tenants query', pageResult.error);
    throw new Error('Invalid REST response shape for tenants query');
  }
  return pageResult.data;
}

async function getTenantsViaGraphql(
  params: Required<Pick<TenantListParams, 'page' | 'pageSize'>> & TenantListParams
): Promise<TenantPage> {
  const response = await graphqlQueries.getTenants(params);
  const pageResult = TenantPageSchema.safeParse(response.tenants);
  if (!pageResult.success) {
    logger.error('Invalid GraphQL response shape for tenants query', pageResult.error);
    throw new Error('Invalid GraphQL response shape for tenants query');
  }
  return pageResult.data;
}

import { RequirePermission } from '@app/authorization/decorators/require-permission.decorator';

export class TenantService {
  // ── Queries (parameterized REST/GraphQL transport) ────────────────────────

  @RequirePermission('TENANT', 'VIEW')
  async getAll(params?: TenantListParams): Promise<TenantPage> {
    const normalizedParams = {
      page: params?.page ?? 1,
      pageSize: params?.pageSize ?? 20,
      search: params?.search,
      criteria: params?.criteria,
      status: params?.status,
      sortBy: params?.sortBy,
      sortOrder: params?.sortOrder,
    };

    const transport = await queryTransportService.getQueryTransport();
    return transport === 'graphql'
      ? getTenantsViaGraphql(normalizedParams)
      : getTenantsViaRest(normalizedParams);
  }

  @RequirePermission('TENANT', 'VIEW')
  async getById(tenantId: string): Promise<Tenant> {
    const transport = await queryTransportService.getQueryTransport();
    if (transport === 'graphql') {
      const response = await graphqlQueries.getTenantById(tenantId);
      if (!response.tenantById) throw new Error('Tenant not found');
      return TenantSchema.parse(response.tenantById);
    }

    const { data } = await httpClient.get<Tenant>(`/tenants/${tenantId}`);
    return TenantSchema.parse(data);
  }

  @RequirePermission('TENANT', 'VIEW')
  async getBranches(tenantId: string): Promise<Branch[]> {
    const transport = await queryTransportService.getQueryTransport();
    if (transport === 'graphql') {
      const response = await graphqlQueries.getTenantBranches(tenantId);
      return BranchListSchema.parse(response.tenantBranches);
    }

    const { data } = await httpClient.get<Branch[]>(`/tenants/${tenantId}/branches`);
    return BranchListSchema.parse(data);
  }

  // ── Commands (REST) ───────────────────────────────────────────────────────

  @RequirePermission('TENANT', 'CREATE')
  async createTenant(payload: CreateTenantPayload): Promise<CreateTenantResponse> {
    const { data } = await httpClient.post('/tenants', payload);
    return CreateTenantResponseSchema.parse(data);
  }

  @RequirePermission('TENANT', 'MANAGE')
  async activateTenant(tenantId: string): Promise<void> {
    await httpClient.post(`/tenants/${tenantId}/activate`);
  }

  @RequirePermission('TENANT', 'MANAGE')
  async setManagementOwner(tenantId: string, value: boolean): Promise<void> {
    await httpClient.post(`/tenants/${tenantId}/set-management-owner`, { value });
  }

  @RequirePermission('TENANT', 'MANAGE')
  async suspendTenant(tenantId: string): Promise<void> {
    await httpClient.post(`/tenants/${tenantId}/suspend`);
  }

  @RequirePermission('TENANT', 'MANAGE')
  async addBranch(tenantId: string, payload: AddBranchPayload): Promise<AddBranchResponse> {
    const { data } = await httpClient.post(`/tenants/${tenantId}/branches`, payload);
    return AddBranchResponseSchema.parse(data);
  }

  @RequirePermission('TENANT', 'MANAGE')
  async removeBranch(tenantId: string, branchId: string): Promise<void> {
    await httpClient.delete(`/tenants/${tenantId}/branches/${branchId}`);
  }

  @RequirePermission('TENANT', 'MANAGE')
  async deactivateBranch(tenantId: string, branchId: string): Promise<void> {
    await httpClient.post(`/tenants/${tenantId}/branches/${branchId}/deactivate`);
  }

  @RequirePermission('TENANT', 'MANAGE')
  async reactivateBranch(tenantId: string, branchId: string): Promise<void> {
    await httpClient.post(`/tenants/${tenantId}/branches/${branchId}/reactivate`);
  }
}

export const tenantService = new TenantService();
export default tenantService;
