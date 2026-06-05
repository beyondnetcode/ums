import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { ApiErrorBanner } from './ApiErrorBanner';
import * as useI18nModule from '@app/i18n/use-i18n';
import * as httpErrorModule from '@app/errors/http-error';
import * as graphqlClientModule from '@infra/http/graphqlClient';

vi.mock('@app/i18n/use-i18n');
vi.mock('@app/errors/http-error');
vi.mock('@infra/http/graphqlClient');

describe('ApiErrorBanner', () => {
  beforeEach(() => {
    vi.restoreAllMocks();

    vi.mocked(useI18nModule.useI18n).mockReturnValue({
      errorBackendUnavailableTitle: 'Backend Unavailable',
      errorBackendUnavailableHint: 'Start the backend',
      errorInvalidRequestTitle: 'Invalid Request',
      errorInvalidRequestHint: 'Check parameters',
      errorGenericTitle: 'Error',
      errorGenericHint: 'Ensure backend is running',
      errorSupportReference: (ref: string) => `Ref: ${ref}`,
    } as any);

    vi.mocked(httpErrorModule.getSupportReferenceId).mockReturnValue(null);
  });

  it('renders generic error message', () => {
    const error = new Error('Something went wrong');
    render(<ApiErrorBanner error={error} />);
    expect(screen.getByText('Error')).toBeInTheDocument();
  });

  it('renders hint message', () => {
    const error = new Error('Something went wrong');
    render(<ApiErrorBanner error={error} />);
    expect(screen.getByText('Ensure backend is running')).toBeInTheDocument();
  });

  it('renders backend unavailable message for GraphQlUnavailableError', () => {
    vi.mocked(graphqlClientModule.GraphQlUnavailableError).mockImplementation(
      class GraphQlUnavailableError extends Error {
        constructor(message: string) {
          super(message);
          this.name = 'GraphQlUnavailableError';
        }
      }
    );

    const UnavailableError = vi.mocked(graphqlClientModule.GraphQlUnavailableError);
    const error = new UnavailableError('Backend unavailable');

    render(<ApiErrorBanner error={error} />);
    expect(screen.getByText('Backend Unavailable')).toBeInTheDocument();
  });

  it('renders validation error message for GraphQlValidationError', () => {
    vi.mocked(graphqlClientModule.GraphQlValidationError).mockImplementation(
      class GraphQlValidationError extends Error {
        constructor(
          message: string,
          public details: string[]
        ) {
          super(message);
          this.name = 'GraphQlValidationError';
        }
      }
    );

    const ValidationError = vi.mocked(graphqlClientModule.GraphQlValidationError);
    const error = new ValidationError('Validation failed', ['detail']);

    render(<ApiErrorBanner error={error} />);
    expect(screen.getByText('Invalid Request')).toBeInTheDocument();
  });

  it('renders support reference when available', () => {
    vi.mocked(httpErrorModule.getSupportReferenceId).mockReturnValue('REF-123');

    const error = new Error('Something went wrong');
    render(<ApiErrorBanner error={error} />);
    expect(screen.getByText('Ref: REF-123')).toBeInTheDocument();
  });
});
