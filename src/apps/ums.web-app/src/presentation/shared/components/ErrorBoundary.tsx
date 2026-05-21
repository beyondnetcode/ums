import React from 'react';
import { AlertTriangle, RefreshCw } from 'lucide-react';

interface ErrorBoundaryProps {
  children: React.ReactNode;
  /** Optional fallback. If omitted, a generic M3-styled error card is shown. */
  fallback?: (error: Error, reset: () => void) => React.ReactNode;
  /** Localized title. @default "Something went wrong" */
  errorTitle?: string;
  /** Localized message. @default "An unexpected error occurred in this view..." */
  errorMessage?: string;
  /** Localized retry label. @default "Try again" */
  retryLabel?: string;
}

interface ErrorBoundaryState {
  error: Error | null;
}

/**
 * AppErrorBoundary
 *
 * Catches unhandled render errors in the subtree and shows a recoverable
 * error screen instead of a blank page. Place at the layout level to
 * isolate crashes to a single route without bringing down the entire app.
 */
export class AppErrorBoundary extends React.Component<ErrorBoundaryProps, ErrorBoundaryState> {
  constructor(props: ErrorBoundaryProps) {
    super(props);
    this.state = { error: null };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { error };
  }

  componentDidCatch(error: Error, info: React.ErrorInfo) {
    // Replace with a real logger (Sentry, Datadog, etc.) in production
    console.error('[UMS ErrorBoundary]', error, info.componentStack);
  }

  reset = () => this.setState({ error: null });

  render() {
    const { error } = this.state;
    const { children, fallback, errorTitle = 'Something went wrong', errorMessage = 'An unexpected error occurred in this view. Your other tabs are unaffected.', retryLabel = 'Try again' } = this.props;

    if (!error) return children;

    if (fallback) return fallback(error, this.reset);

    return (
      <div className="flex-1 flex items-center justify-center p-8">
        <div className="max-w-md w-full rounded-2xl border border-m3-error/30 bg-m3-error-container/10 p-8 text-center space-y-4 shadow-lg">
          <div className="w-14 h-14 bg-m3-error/10 border border-m3-error/20 text-m3-error rounded-full flex items-center justify-center mx-auto">
            <AlertTriangle className="w-7 h-7" />
          </div>
          <div>
            <h2 className="text-sm font-semibold text-m3-on-surface">{errorTitle}</h2>
            <p className="mt-1 text-xs text-m3-secondary leading-relaxed">{errorMessage}</p>
          </div>
          {import.meta.env.DEV && (
            <pre className="text-left text-[10px] font-mono bg-m3-surface-container rounded-lg p-3 overflow-auto max-h-32 text-m3-error/80">
              {error.message}
            </pre>
          )}
          <button
            onClick={this.reset}
            className="inline-flex items-center gap-2 px-5 py-2 rounded-full bg-m3-primary text-m3-on-primary text-sm font-medium hover:bg-m3-primary/90 transition-colors"
          >
            <RefreshCw className="w-3.5 h-3.5" /> {retryLabel}
          </button>
        </div>
      </div>
    );
  }
}

/**
 * RouteErrorBoundary
 *
 * Lightweight boundary for wrapping individual route components when you
 * want per-route isolation rather than app-level.
 */
export const RouteErrorBoundary: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <AppErrorBoundary>{children}</AppErrorBoundary>
);
