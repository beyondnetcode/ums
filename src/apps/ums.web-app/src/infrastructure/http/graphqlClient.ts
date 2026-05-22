/**
 * graphqlClient.ts — Infrastructure GraphQL client
 *
 * Single configured GraphQL client for all read operations.
 * Commands/transactions continue to use the REST httpClient.
 *
 * Uses native fetch with explicit body construction for full control
 * over request format, ensuring compatibility with HotChocolate.
 *
 * In development mode, logs the outgoing payload for debugging.
 */
import { getRequestContext, DEV_TENANT_ID } from './request-context';
import { logger } from '@app/utils/logger';

function getGraphqlUrl(): string {
  if (typeof window !== 'undefined') {
    return `${window.location.origin}/graphql`;
  }
  return 'http://localhost:5173/graphql';
}

export interface GraphQlErrorResponse {
  errors: Array<{
    message: string;
    locations?: Array<{ line: number; column: number }>;
    path?: string[];
    extensions?: { code: string };
  }>;
  data?: null;
}

export class GraphQlError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly responseErrors?: GraphQlErrorResponse['errors'],
    public readonly operationName?: string,
  ) {
    super(message);
    this.name = 'GraphQlError';
  }
}

export class GraphQlUnavailableError extends Error {
  constructor(public readonly status: number) {
    super(status === 0 ? 'Network error: unable to reach the API' : `API unavailable (HTTP ${status})`);
    this.name = 'GraphQlUnavailableError';
  }
}

export class GraphQlValidationError extends Error {
  constructor(message: string, public readonly details: string[]) {
    super(message);
    this.name = 'GraphQlValidationError';
  }
}

function extractOperationName(query: string): string | undefined {
  const match = query.match(/(?:query|mutation)\s+(\w+)/);
  return match?.[1];
}

async function executeGraphQl<T>(query: string, variables?: Record<string, unknown>): Promise<T> {
  const url = getGraphqlUrl();
  const { userId, language } = getRequestContext();

  const body: Record<string, unknown> = { query };
  if (variables && Object.keys(variables).length > 0) {
    body.variables = variables;
  }
  const operationName = extractOperationName(query);
  if (operationName) {
    body.operationName = operationName;
  }

  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    'X-Tenant-Id': DEV_TENANT_ID,
  };
  if (userId)   headers['X-User-Id'] = userId;
  if (language) headers['X-Language'] = language;

  const isDev = typeof import.meta !== 'undefined' && import.meta.env?.DEV;
  if (isDev) {
    logger.debug('GraphQL request', {
      url,
      operationName,
      variables: variables ?? {},
      headers: { 'X-Tenant-Id': headers['X-Tenant-Id'], 'X-User-Id': headers['X-User-Id'], 'X-Language': headers['X-Language'] },
    });
  }

  const response = await fetch(url, {
    method: 'POST',
    headers,
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    if (response.status === 502 || response.status === 503 || response.status === 0) {
      throw new GraphQlUnavailableError(response.status);
    }

    let errorBody: GraphQlErrorResponse | null = null;
    try {
      errorBody = await response.json();
    } catch {
      // ignore parse errors on non-JSON responses
    }

    if (isDev) {
      logger.warn('GraphQL error response', {
        url,
        status: response.status,
        operationName,
        requestBody: body,
        responseBody: errorBody,
      });
    }

    if (response.status === 400 && errorBody?.errors) {
      const messages = errorBody.errors.map((e) => e.message);
      throw new GraphQlValidationError(
        `GraphQL validation failed: ${messages.join('; ')}`,
        messages,
      );
    }

    throw new GraphQlError(
      `GraphQL request failed with HTTP ${response.status}`,
      response.status,
      errorBody?.errors,
      operationName,
    );
  }

  const result = await response.json() as { data?: T; errors?: GraphQlErrorResponse['errors'] };

  if (result.errors && result.errors.length > 0) {
    const messages = result.errors.map((e) => e.message);
    throw new GraphQlValidationError(
      `GraphQL errors: ${messages.join('; ')}`,
      messages,
    );
  }

  if (!result.data) {
    throw new GraphQlError('GraphQL response contained no data', 200, undefined, operationName);
  }

  return result.data;
}

export const graphqlClient = {
  request: async <T>(query: string, variables?: Record<string, unknown>): Promise<T> => {
    return executeGraphQl<T>(query, variables);
  },
};
