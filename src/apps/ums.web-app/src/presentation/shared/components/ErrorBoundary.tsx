import React, { Component, ErrorInfo, ReactNode } from 'react';
import { M3Card } from './M3Card';
import { M3Button } from './M3Button';
import { AlertTriangle } from 'lucide-react';

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo): void {
    if (import.meta.env.DEV) {
      console.error('ErrorBoundary caught:', error, errorInfo);
    }
  }

  render(): ReactNode {
    if (this.state.hasError) {
      return (
        <div className="min-h-screen flex items-center justify-center bg-m3-surface p-6">
          <M3Card variant="elevated" className="max-w-md w-full p-6 border border-m3-error/20 space-y-4">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-m3-error/15 rounded-lg text-m3-error">
                <AlertTriangle className="w-6 h-6" />
              </div>
              <div>
                <h2 className="text-base font-semibold text-m3-on-surface">Something went wrong</h2>
                <p className="text-xs text-m3-secondary mt-1">An unexpected error occurred.</p>
              </div>
            </div>

            {import.meta.env.DEV && this.state.error && (
              <pre className="text-[10px] font-mono text-m3-error bg-m3-error/5 p-3 rounded-lg overflow-auto max-h-32">
                {this.state.error.message}
              </pre>
            )}

            <M3Button
              variant="filled"
              onClick={() => window.location.reload()}
              className="w-full"
            >
              Reload Application
            </M3Button>
          </M3Card>
        </div>
      );
    }

    return this.props.children;
  }
}
