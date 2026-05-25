import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { AppErrorBoundary, RouteErrorBoundary } from './ErrorBoundary';


const Bomb = ({ shouldThrow }: { shouldThrow: boolean }) => {
  if (shouldThrow) {
    throw new Error('Explosion!');
  }
  return <div>Safe</div>;
};

describe('ErrorBoundary', () => {
  it('renders children if no error', () => {
    render(
      <AppErrorBoundary>
        <Bomb shouldThrow={false} />
      </AppErrorBoundary>
    );
    expect(screen.getByText('Safe')).toBeInTheDocument();
  });

  it('catches error and displays generic fallback', () => {
    const originalConsoleError = console.error;
    console.error = vi.fn(); // Suppress React error logging in test output

    render(
      <RouteErrorBoundary>
        <Bomb shouldThrow={true} />
      </RouteErrorBoundary>
    );

    // From i18n we expect "Algo salió mal" or "Something went wrong". We'll just check if the retry button is there.
    expect(screen.getByRole('button')).toBeInTheDocument();
    
    console.error = originalConsoleError;
  });
});
