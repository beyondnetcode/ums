interface HttpErrorLike {
  supportReferenceId?: string;
  response?: {
    status?: number;
    headers?: Record<string, unknown>;
    data?: {
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
                traceId: typeof data.traceId === 'string' ? data.traceId : undefined,
                supportReferenceId: typeof data.supportReferenceId === 'string' ? data.supportReferenceId : undefined,
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
  const graphqlTraceId = httpError.graphQLErrors
    ?.map((item) => item.extensions?.traceId)
    .find((value): value is string => typeof value === 'string');
  const headerTraceId = httpError.response?.headers?.['x-correlation-id']
    ?? httpError.response?.headers?.['X-Correlation-Id'];

  return httpError.supportReferenceId
    ?? httpError.response?.data?.supportReferenceId
    ?? httpError.response?.data?.traceId
    ?? graphqlTraceId
    ?? (typeof headerTraceId === 'string' ? headerTraceId : undefined);
};

export const getHttpErrorMessage = (_error: unknown, fallback: string): string => fallback;
