/**
 * tenant.graphql.ts — GraphQL query definitions for the Tenant bounded context
 *
 * All read operations use these GraphQL queries.
 * Commands/transactions use REST endpoints via tenant.service.ts.
 */
import { graphqlClient } from '@infra/http/graphqlClient';

// ─── GraphQL Query Strings ──────────────────────────────────────────────────

const GET_TENANTS = `#graphql
  query Tenants(
    $page: Int!
    $pageSize: Int!
    $search: String
    $criteria: String
    $status: String
    $sortBy: String
    $sortOrder: String
  ) {
    tenants(
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

const GET_TENANT_BY_ID = `#graphql
  query Tenant($tenantId: UUID!) {
    tenantById(tenantId: $tenantId) {
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

const GET_TENANT_BRANCHES = `#graphql
  query TenantBranches($tenantId: UUID!) {
    tenantBranches(tenantId: $tenantId) {
      branchId
      code
      name
      isActive
      geofencingMetadata
    }
  }
`;

// ─── GraphQL Response Types ─────────────────────────────────────────────────

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
  tenants: GraphqlTenantPage;
}

export interface GetTenantByIdResponse {
  tenantById: GraphqlTenantDto | null;
}

export interface GetTenantBranchesResponse {
  tenantBranches: GraphqlBranchDto[];
}

// ─── Query Functions ────────────────────────────────────────────────────────

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
    return graphqlClient.request<GetTenantsResponse>(GET_TENANTS, params);
  },

  getTenantById: async (tenantId: string): Promise<GetTenantByIdResponse> => {
    return graphqlClient.request<GetTenantByIdResponse>(GET_TENANT_BY_ID, { tenantId });
  },

  getTenantBranches: async (tenantId: string): Promise<GetTenantBranchesResponse> => {
    return graphqlClient.request<GetTenantBranchesResponse>(GET_TENANT_BRANCHES, { tenantId });
  },
};
