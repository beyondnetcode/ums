interface HttpErrorLike {
  message?: string;
  response?: {
    status?: number;
    data?: {
      detail?: string;
      message?: string;
    };
  };
  graphQLErrors?: ReadonlyArray<{
    message?: string;
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
    message: typeof error.message === 'string' ? error.message : undefined,
    response: response
      ? {
          status: typeof response.status === 'number' ? response.status : undefined,
          data: data
            ? {
                detail: typeof data.detail === 'string' ? data.detail : undefined,
                message: typeof data.message === 'string' ? data.message : undefined,
              }
            : undefined,
        }
      : undefined,
    graphQLErrors,
  };
};

export const getHttpStatus = (error: unknown): number | undefined =>
  asHttpError(error).response?.status;

export const getHttpErrorMessage = (error: unknown, fallback: string): string => {
  const httpError = asHttpError(error);

  // GraphQL errors
  if (httpError.graphQLErrors?.length) {
    const firstMessage = httpError.graphQLErrors[0]?.message;
    if (firstMessage) return firstMessage;
  }

  // REST errors
  return httpError.response?.data?.detail
    ?? httpError.response?.data?.message
    ?? httpError.message
    ?? fallback;
};
