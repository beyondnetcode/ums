import { describe, expect, it } from 'vitest';
import { getHttpErrorMessage, getHttpStatus, getSupportReferenceId } from './http-error';

describe('http-error helpers', () => {
  it('does not expose REST error details and extracts the support reference', () => {
    const error = {
      message: 'Request failed',
      response: {
        status: 404,
        data: {
          detail: 'System.InvalidOperationException: private detail',
          errorId: 'db83c6dd-770d-4d92-b6b8-98e80c790e4a',
          traceId: 'corr-404',
        },
      },
    };

    expect(getHttpStatus(error)).toBe(404);
    expect(getHttpErrorMessage(error, 'Could not load tenants.')).toBe('Could not load tenants.');
    expect(getSupportReferenceId(error)).toBe('db83c6dd-770d-4d92-b6b8-98e80c790e4a');
  });

  it('extracts a GraphQL support reference without displaying its message', () => {
    const error = {
      graphQLErrors: [{
        message: 'Sensitive internal failure',
        extensions: { errorId: '3d8695ca-180b-44c2-9908-6683316cbf77', traceId: 'corr-gql' },
      }],
    };

    expect(getHttpErrorMessage(error, 'Try again later.')).toBe('Try again later.');
    expect(getSupportReferenceId(error)).toBe('3d8695ca-180b-44c2-9908-6683316cbf77');
  });

  it('displays only an explicitly approved actionable API message', () => {
    const error = {
      response: {
        status: 422,
        data: {
          detail: 'Validation.Failed: private transport content',
          userMessage: 'La descripción del módulo no puede exceder 500 caracteres.',
        },
      },
    };

    expect(getHttpErrorMessage(error, 'No se pudo registrar el módulo.'))
      .toBe('La descripción del módulo no puede exceder 500 caracteres.');
  });

  it('falls back when the error shape is unknown', () => {
    expect(getHttpStatus('boom')).toBeUndefined();
    expect(getHttpErrorMessage('boom', 'Fallback')).toBe('Fallback');
  });
});
