import React from 'react'
import ReactDOM from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import App from './App.tsx'
import './index.css'
import { useDevToolsStore } from './application/stores/devTools.store'
import { useI18nStore } from './application/stores/i18n.store'
import { configureRequestContext } from './infrastructure/http/request-context'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: 1,
    }
  }
})

const devContextProvider = () => {
  if (!import.meta.env.DEV) return {};
  const { devUserId, devLanguage } = useDevToolsStore.getState();
  useI18nStore.getState().setLanguage(devLanguage);
  return { userId: devUserId, language: devLanguage };
};

configureRequestContext(devContextProvider);

function LocaleSync() {
  const lang = useI18nStore((s) => s.language);
  React.useEffect(() => {
    document.documentElement.lang = lang;
  }, [lang]);
  return null;
}

const rootEl = document.getElementById('root');
if (!rootEl) {
  throw new Error(
    '[UMS] Root element #root not found in index.html. ' +
    'Check that public/index.html contains <div id="root"></div>.'
  );
}

ReactDOM.createRoot(rootEl).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <LocaleSync />
      <App />
    </QueryClientProvider>
  </React.StrictMode>,
)
