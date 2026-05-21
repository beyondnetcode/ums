/**
 * graphqlClient.ts — Infrastructure GraphQL client
 *
 * Single configured GraphQL client for all read operations.
 * Commands/transactions continue to use the REST httpClient.
 */
import { GraphQLClient } from 'graphql-request';
import { getRequestContext, DEV_TENANT_ID } from './request-context';

function getGraphqlUrl(): string {
  if (typeof window !== 'undefined') {
    return `${window.location.origin}/graphql`;
  }
  return 'http://localhost:5173/graphql';
}

function createGraphQLClient(): GraphQLClient {
  return new GraphQLClient(getGraphqlUrl(), {
    requestMiddleware: (request) => {
      const { userId, language } = getRequestContext();
      request.headers ??= {};
      if (userId)   (request.headers as Record<string, string>)['X-User-Id'] = userId;
      if (language) (request.headers as Record<string, string>)['X-Language'] = language;
      (request.headers as Record<string, string>)['X-Tenant-Id'] = DEV_TENANT_ID;
      return request;
    },
  });
}

export const graphqlClient = createGraphQLClient();
