import { describe, expect, it } from 'vitest';
import { getHttpErrorMessage, getHttpStatus, getSupportReferenceId } from './http-error';

describe('http-error helpers', () => {
  it('does not expose REST error details and extracts the support reference', () => {
    const error = {
      message: 'Request failed',
      response: {
        status: 404,
        data: { detail: 'System.InvalidOperationException: private detail', traceId: 'corr-404' },
      },
    };

    expect(getHttpStatus(error)).toBe(404);
    expect(getHttpErrorMessage(error, 'Could not load tenants.')).toBe('Could not load tenants.');
    expect(getSupportReferenceId(error)).toBe('corr-404');
  });

  it('extracts a GraphQL support reference without displaying its message', () => {
    const error = {
      graphQLErrors: [{
        message: 'Sensitive internal failure',
        extensions: { traceId: 'corr-gql' },
      }],
    };

    expect(getHttpErrorMessage(error, 'Try again later.')).toBe('Try again later.');
    expect(getSupportReferenceId(error)).toBe('corr-gql');
  });

  it('falls back when the error shape is unknown', () => {
    expect(getHttpStatus('boom')).toBeUndefined();
    expect(getHttpErrorMessage('boom', 'Fallback')).toBe('Fallback');
  });
});
