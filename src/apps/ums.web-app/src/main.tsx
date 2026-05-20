import React from 'react'
import ReactDOM from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import App from './App.tsx'
import './index.css'
import { setApiHeaderProvider } from './infrastructure/identity/config/api-header.provider'
import { useAuthStore } from './application/stores/auth.store'
import { ErrorBoundary } from './presentation/shared/components/ErrorBoundary'

setApiHeaderProvider({
  getHeaders: () => {
    const { devUserId, devLanguage } = useAuthStore.getState();
    const headers: Record<string, string> = {};
    if (devUserId) headers['X-User-Id'] = devUserId;
    if (devLanguage) headers['X-Language'] = devLanguage;
    return headers;
  },
});

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    }
  }
})

document.body.classList.add('dark');

window.addEventListener('unhandledrejection', (event) => {
  if (import.meta.env.DEV) {
    console.error('Unhandled promise rejection:', event.reason);
  }
  event.preventDefault();
});

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <ErrorBoundary>
      <QueryClientProvider client={queryClient}>
        <App />
      </QueryClientProvider>
    </ErrorBoundary>
  </React.StrictMode>,
)
