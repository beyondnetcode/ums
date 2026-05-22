/**
 * user-account.graphql.ts — GraphQL query definitions for the UserAccount bounded context
 *
 * All read operations use these GraphQL queries.
 * Commands/transactions use REST endpoints via user-account.service.ts.
 */
import { graphqlClient } from '@infra/http/graphqlClient';

const GET_USER_ACCOUNTS = `#graphql
  query UserAccounts(
    $page: Int!
    $pageSize: Int!
    $search: String
    $criteria: String
    $status: String
    $sortBy: String
    $sortOrder: String
    $tenantId: UUID
  ) {
    getUserAccounts(
      page: $page
      pageSize: $pageSize
      search: $search
      criteria: $criteria
      status: $status
      sortBy: $sortBy
      sortOrder: $sortOrder
      tenantId: $tenantId
    ) {
      items {
        userAccountId
        tenantId
        branchId
        email
        category
        status
        identityReference
        identityReferenceType
      }
      page
      pageSize
      totalItems
      totalPages
    }
  }
`;

const GET_USER_ACCOUNT_BY_ID = `#graphql
  query UserAccount($userAccountId: UUID!) {
    getUserAccountById(userAccountId: $userAccountId) {
      userAccountId
      tenantId
      branchId
      email
      category
      status
      identityReference
      identityReferenceType
    }
  }
`;

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

export const graphqlQueries = {
  getUserAccounts: async (params: {
    page: number;
    pageSize: number;
    search?: string;
    criteria?: string;
    status?: string;
    sortBy?: string;
    sortOrder?: string;
    tenantId?: string;
  }): Promise<GetUserAccountsResponse> => {
    return graphqlClient.request<GetUserAccountsResponse>(GET_USER_ACCOUNTS, params);
  },

  getUserAccountById: async (userAccountId: string): Promise<GetUserAccountByIdResponse> => {
    return graphqlClient.request<GetUserAccountByIdResponse>(GET_USER_ACCOUNT_BY_ID, { userAccountId });
  },
};
