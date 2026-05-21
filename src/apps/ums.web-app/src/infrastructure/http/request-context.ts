/**
 * request-context.ts — Shared request context for HTTP and GraphQL clients
 *
 * M-7: Eliminates duplication between httpClient and graphqlClient.
 * L-6: BASE_URL and GRAPHQL_ENDPOINT configurable via env vars.
 */

export interface RequestContext {
  userId?: string;
  language?: 'en' | 'es';
}

export type RequestContextProvider = () => RequestContext;

let provider: RequestContextProvider = () => ({});

export const configureRequestContext = (p: RequestContextProvider) => {
  provider = p;
};

export const getRequestContext = (): RequestContext => provider();

export const BASE_URL = import.meta.env.VITE_API_BASE_PATH || '/api/v1';

// C-3/C-4: Hardcoded dev tenant ID for local testing.
// TODO: Replace with proper tenant resolution from auth context.
export const DEV_TENANT_ID = '3fa85f64-5717-4562-b3fc-2c963f66afa6';
