import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { GraphQlValidationError } from '@infra/http/graphqlClient';
import { ApiErrorBanner } from './ApiErrorBanner';

describe('ApiErrorBanner', () => {
  it('shows a support reference without exposing technical error details', () => {
    render(
      <ApiErrorBanner
        error={new GraphQlValidationError(
          'GraphQL validation failed: stack trace text',
          ['Internal resolver detail'],
          'corr-banner-123',
        )}
      />,
    );

    expect(screen.getByText(/corr-banner-123/)).toBeInTheDocument();
    expect(screen.queryByText(/stack trace text/)).not.toBeInTheDocument();
    expect(screen.queryByText(/Internal resolver detail/)).not.toBeInTheDocument();
  });
});
