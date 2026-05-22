/**
 * tenant.graphql.ts — GraphQL query definitions for the Tenant bounded context
 *
 * All read operations use these GraphQL queries.
 * Commands/transactions use REST endpoints via tenant.service.ts.
 */
import { graphqlClient, GraphQlValidationError } from '@infra/http/graphqlClient';

const GET_TENANTS = `
  query Tenants(
    $page: Int!
    $pageSize: Int!
    $search: String
    $criteria: String
    $status: String
    $sortBy: String
    $sortOrder: String
  ) {
    getTenants: tenants(
      page: $page
      pageSize: $pageSize
      search: $search
      criteria: $criteria
      status: $status
      sortBy: $sortBy
      sortOrder: $sortOrder
    ) {
      items {
        tenantId
        code
        name
        type
        status
        parentTenantId
        companyReference
      }
      page
      pageSize
      totalItems
      totalPages
    }
  }
`;

const GET_TENANT_BY_ID = `
  query Tenant($tenantId: UUID!) {
    getTenantById: tenantById(tenantId: $tenantId) {
      tenantId
      code
      name
      type
      status
      parentTenantId
      companyReference
    }
  }
`;

const GET_TENANT_BRANCHES = `
  query TenantBranches($tenantId: UUID!) {
    getTenantBranches: tenantBranches(tenantId: $tenantId) {
      branchId
      code
      name
      isActive
      geofencingMetadata
    }
  }
`;

export interface GraphqlTenantDto {
  tenantId: string;
  code: string;
  name: string;
  type: string;
  status: string;
  parentTenantId: string | null;
  companyReference: string | null;
}

export interface GraphqlTenantPage {
  items: GraphqlTenantDto[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface GraphqlBranchDto {
  branchId: string;
  code: string;
  name: string;
  isActive: boolean;
  geofencingMetadata: string | null;
}

export interface GetTenantsResponse {
  getTenants: GraphqlTenantPage;
}

export interface GetTenantByIdResponse {
  getTenantById: GraphqlTenantDto | null;
}

export interface GetTenantBranchesResponse {
  getTenantBranches: GraphqlBranchDto[];
}

function validateGetTenantsParams(params: {
  page: number;
  pageSize: number;
}): void {
  if (!Number.isInteger(params.page) || params.page < 1) {
    throw new GraphQlValidationError('Invalid page parameter', [`page must be a positive integer, got: ${params.page}`]);
  }
  if (!Number.isInteger(params.pageSize) || params.pageSize < 1) {
    throw new GraphQlValidationError('Invalid pageSize parameter', [`pageSize must be a positive integer, got: ${params.pageSize}`]);
  }
}

export const graphqlQueries = {
  getTenants: async (params: {
    page: number;
    pageSize: number;
    search?: string;
    criteria?: string;
    status?: string;
    sortBy?: string;
    sortOrder?: string;
  }): Promise<GetTenantsResponse> => {
    validateGetTenantsParams(params);

    const variables: Record<string, unknown> = {
      page: params.page,
      pageSize: params.pageSize,
    };
    if (params.search !== undefined)     variables.search = params.search;
    if (params.criteria !== undefined)   variables.criteria = params.criteria;
    if (params.status !== undefined)     variables.status = params.status;
    if (params.sortBy !== undefined)     variables.sortBy = params.sortBy;
    if (params.sortOrder !== undefined)  variables.sortOrder = params.sortOrder;

    return graphqlClient.request<GetTenantsResponse>(GET_TENANTS, variables);
  },

  getTenantById: async (tenantId: string): Promise<GetTenantByIdResponse> => {
    if (!tenantId || tenantId.trim() === '') {
      throw new GraphQlValidationError('Invalid tenantId parameter', ['tenantId must be a non-empty string']);
    }
    return graphqlClient.request<GetTenantByIdResponse>(GET_TENANT_BY_ID, { tenantId });
  },

  getTenantBranches: async (tenantId: string): Promise<GetTenantBranchesResponse> => {
    if (!tenantId || tenantId.trim() === '') {
      throw new GraphQlValidationError('Invalid tenantId parameter', ['tenantId must be a non-empty string']);
    }
    return graphqlClient.request<GetTenantBranchesResponse>(GET_TENANT_BRANCHES, { tenantId });
  },
};
