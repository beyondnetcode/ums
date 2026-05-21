import { describe, expect, it } from 'vitest';
import { getHttpErrorMessage, getHttpStatus } from './http-error';

describe('http-error helpers', () => {
  it('extracts status and detail from axios-like errors', () => {
    const error = {
      message: 'Request failed',
      response: {
        status: 404,
        data: { detail: 'Tenant not found' },
      },
    };

    expect(getHttpStatus(error)).toBe(404);
    expect(getHttpErrorMessage(error, 'Fallback')).toBe('Tenant not found');
  });

  it('falls back when the error shape is unknown', () => {
    expect(getHttpStatus('boom')).toBeUndefined();
    expect(getHttpErrorMessage('boom', 'Fallback')).toBe('Fallback');
  });
});
