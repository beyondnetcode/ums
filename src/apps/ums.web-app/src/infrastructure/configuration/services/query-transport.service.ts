import { logger } from '@app/utils/logger';
import { httpClient } from '@infra/http/httpClient';

export type QueryTransport = 'rest' | 'graphql';

const FRONTEND_CONFIG_TRANSPORT = 'FRONTEND_CONFIG_TRANSPORT';
const DEFAULT_QUERY_TRANSPORT: QueryTransport = 'rest';
const CACHE_TTL_MS = 5 * 60 * 1000;

interface ResolvedParameterResponse {
  code: string;
  effectiveValue: string;
}

let cachedTransport: { value: QueryTransport; expiresAt: number } | null = null;
let pendingTransport: Promise<QueryTransport> | null = null;

function normalizeTransport(value: string | undefined): QueryTransport {
  return value?.trim().toLowerCase() === 'graphql' ? 'graphql' : DEFAULT_QUERY_TRANSPORT;
}

async function fetchConfiguredTransport(): Promise<QueryTransport> {
  try {
    const { data } = await httpClient.get<ResolvedParameterResponse>(
      `/parameter-definitions/resolved/${FRONTEND_CONFIG_TRANSPORT}`
    );
    return normalizeTransport(data.effectiveValue);
  } catch (error) {
    logger.warn('Unable to resolve frontend query transport; using REST fallback.', error);
    return DEFAULT_QUERY_TRANSPORT;
  }
}

export const queryTransportService = {
  async getQueryTransport(): Promise<QueryTransport> {
    const now = Date.now();
    if (cachedTransport && cachedTransport.expiresAt > now) {
      return cachedTransport.value;
    }

    pendingTransport ??= fetchConfiguredTransport().then(value => {
      cachedTransport = { value, expiresAt: Date.now() + CACHE_TTL_MS };
      pendingTransport = null;
      return value;
    });

    return pendingTransport;
  },

  resetCache(): void {
    cachedTransport = null;
    pendingTransport = null;
  },
};
