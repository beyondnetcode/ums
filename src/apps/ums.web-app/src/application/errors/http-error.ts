interface HttpErrorLike {
  supportReferenceId?: string;
  response?: {
    status?: number;
    headers?: Record<string, unknown>;
    data?: {
      // RFC 7807 Problem Details fields
      detail?: string;         // RFC 7807 user-safe detail provided by the API
      title?: string;
      // Approved, localized user-facing content
      userMessage?: string;
      // Correlation
      errorId?: string;        // handle the user gives to support (look up in Loki)
      traceId?: string;
      supportReferenceId?: string;
    };
  };
  graphQLErrors?: ReadonlyArray<{
    extensions?: Record<string, unknown>;
  }>;
}

const isRecord = (value: unknown): value is Record<string, unknown> =>
  typeof value === 'object' && value !== null;

export const asHttpError = (error: unknown): HttpErrorLike => {
  if (!isRecord(error)) return {};
  const response = isRecord(error.response) ? error.response : undefined;
  const data = response && isRecord(response.data) ? response.data : undefined;
  const graphQLErrors = Array.isArray(error.graphQLErrors)
    ? error.graphQLErrors
    : undefined;
  return {
    supportReferenceId: typeof error.supportReferenceId === 'string' ? error.supportReferenceId : undefined,
    response: response
      ? {
          status: typeof response.status === 'number' ? response.status : undefined,
          headers: isRecord(response.headers) ? response.headers : undefined,
          data: data
            ? {
                detail:             typeof data.detail === 'string' ? data.detail : undefined,
                title:              typeof data.title === 'string' ? data.title : undefined,
                errorId:            typeof data.errorId === 'string' ? data.errorId : undefined,
                traceId:            typeof data.traceId === 'string' ? data.traceId : undefined,
                supportReferenceId: typeof data.supportReferenceId === 'string' ? data.supportReferenceId : undefined,
                userMessage:        typeof data.userMessage === 'string' ? data.userMessage : undefined,
              }
            : undefined,
        }
      : undefined,
    graphQLErrors,
  };
};

export const getHttpStatus = (error: unknown): number | undefined =>
  asHttpError(error).response?.status;

export const getSupportReferenceId = (error: unknown): string | undefined => {
  const httpError = asHttpError(error);
  const graphqlErrorId = httpError.graphQLErrors
    ?.map((item) => item.extensions?.errorId)
    .find((value): value is string => typeof value === 'string');
  const graphqlTraceId = httpError.graphQLErrors
    ?.map((item) => item.extensions?.traceId)
    .find((value): value is string => typeof value === 'string');
  const headerErrorId = httpError.response?.headers?.['x-error-id']
    ?? httpError.response?.headers?.['X-Error-Id'];
  const headerTraceId = httpError.response?.headers?.['x-correlation-id']
    ?? httpError.response?.headers?.['X-Correlation-Id'];

  return httpError.supportReferenceId
    ?? httpError.response?.data?.supportReferenceId
    ?? httpError.response?.data?.errorId
    ?? graphqlErrorId
    ?? (typeof headerErrorId === 'string' ? headerErrorId : undefined)
    ?? httpError.response?.data?.traceId
    ?? graphqlTraceId
    ?? (typeof headerTraceId === 'string' ? headerTraceId : undefined);
};

/**
 * Extracts the user-visible error message from an API error.
 *
 * Priority chain (OBS-01 contract):
 *  1. `userMessage` — localized and approved by the backend
 *  2. Problem Details `detail` — user-safe API message for RFC 7807 responses
 *  3. Caller-supplied fallback string
 *
 * Stack traces and internal identifiers never appear here; they stay in Grafana Loki,
 * keyed by the `errorId` returned by `getSupportReferenceId()`.
 */
export const getHttpErrorMessage = (error: unknown, fallback: string): string => {
  const data = asHttpError(error).response?.data;
  return data?.userMessage ?? data?.detail ?? fallback;
};
