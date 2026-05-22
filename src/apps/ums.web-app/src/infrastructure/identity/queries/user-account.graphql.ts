/**
 * user-account.graphql.ts — GraphQL query definitions for the UserAccount bounded context
 *
 * All read operations use these GraphQL queries.
 * Commands/transactions use REST endpoints via user-account.service.ts.
 */
import { graphqlClient, GraphQlValidationError } from '@infra/http/graphqlClient';

const GET_USER_ACCOUNTS = 'query UserAccounts($page: Int!, $pageSize: Int!, $search: String, $criteria: String, $status: String, $sortBy: String, $sortOrder: String, $tenantId: UUID) { getUserAccounts: userAccounts(page: $page, pageSize: $pageSize, search: $search, criteria: $criteria, status: $status, sortBy: $sortBy, sortOrder: $sortOrder, tenantId: $tenantId) { items { userAccountId tenantId branchId email category status identityReference identityReferenceType } page pageSize totalItems totalPages } }';

const GET_USER_ACCOUNT_BY_ID = 'query UserAccount($userAccountId: UUID!) { getUserAccountById: userAccountById(userAccountId: $userAccountId) { userAccountId tenantId branchId email category status identityReference identityReferenceType } }';

export interface GraphqlUserAccountDto {
  userAccountId: string;
  tenantId: string;
  branchId: string | null;
  email: string;
  category: string;
  status: string;
  identityReference: string | null;
  identityReferenceType: string | null;
}

export interface GraphqlUserAccountPage {
  items: GraphqlUserAccountDto[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface GetUserAccountsResponse {
  getUserAccounts: GraphqlUserAccountPage;
}

export interface GetUserAccountByIdResponse {
  getUserAccountById: GraphqlUserAccountDto | null;
}

export interface GetUserAccountsParams {
  page: number;
  pageSize: number;
  search?: string;
  criteria?: string;
  status?: string;
  sortBy?: string;
  sortOrder?: string;
  tenantId?: string;
}

function validateUserAccountsParams(params: GetUserAccountsParams): void {
  if (!Number.isInteger(params.page) || params.page < 1) {
    throw new GraphQlValidationError('Invalid page parameter', [`page must be a positive integer, got: ${params.page}`]);
  }
  if (!Number.isInteger(params.pageSize) || params.pageSize < 1) {
    throw new GraphQlValidationError('Invalid pageSize parameter', [`pageSize must be a positive integer, got: ${params.pageSize}`]);
  }
}

export const graphqlQueries = {
  getUserAccounts: async (params: GetUserAccountsParams): Promise<GetUserAccountsResponse> => {
    validateUserAccountsParams(params);

    const variables: Record<string, unknown> = {
      page: params.page,
      pageSize: params.pageSize,
    };
    if (params.search !== undefined)     variables.search = params.search;
    if (params.criteria !== undefined)   variables.criteria = params.criteria;
    if (params.status !== undefined)     variables.status = params.status;
    if (params.sortBy !== undefined)     variables.sortBy = params.sortBy;
    if (params.sortOrder !== undefined)  variables.sortOrder = params.sortOrder;
    if (params.tenantId !== undefined)   variables.tenantId = params.tenantId;

    return graphqlClient.request<GetUserAccountsResponse>(GET_USER_ACCOUNTS, variables);
  },

  getUserAccountById: async (userAccountId: string): Promise<GetUserAccountByIdResponse> => {
    if (!userAccountId || userAccountId.trim() === '') {
      throw new GraphQlValidationError('Invalid userAccountId parameter', ['userAccountId must be a non-empty string']);
    }
    return graphqlClient.request<GetUserAccountByIdResponse>(GET_USER_ACCOUNT_BY_ID, { userAccountId });
  },
};
