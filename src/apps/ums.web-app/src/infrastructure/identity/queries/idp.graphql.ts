/**
 * idp.graphql.ts — GraphQL query definitions for Identity Provider bounded context
 *
 * All read operations use these GraphQL queries.
 * Commands/transactions use REST endpoints via idp.service.ts.
 */
import { graphqlClient, GraphQlValidationError } from '@infra/http/graphqlClient';

const GET_IDENTITY_PROVIDERS = `
  query IdentityProviders($tenantId: UUID!) {
    tenantIdentityProviders(tenantId: $tenantId) {
      identityProviderId
      code
      name
      description
      strategy
      isActive
    }
  }
`;

export interface GraphqlIdentityProviderDto {
  identityProviderId: string;
  code: string;
  name: string;
  description: string;
  strategy: string;
  isActive: boolean;
}

export interface GetIdentityProvidersResponse {
  tenantIdentityProviders: GraphqlIdentityProviderDto[];
}

export const graphqlIdpQueries = {
  getIdentityProviders: async (tenantId: string): Promise<GetIdentityProvidersResponse> => {
    if (!tenantId || tenantId.trim() === '') {
      throw new GraphQlValidationError('Invalid tenantId parameter', ['tenantId must be a non-empty string']);
    }
    return graphqlClient.request<GetIdentityProvidersResponse>(GET_IDENTITY_PROVIDERS, { tenantId });
  },
};
