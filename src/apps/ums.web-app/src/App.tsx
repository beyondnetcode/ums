import { useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { MainLayout } from './presentation/shared/layouts/MainLayout';
import { TenantDashboardScreen } from './presentation/identity/tenant/screens/TenantDashboardScreen';
import { ProfileScreen } from './presentation/identity/profile/screens/ProfileScreen';
import { LoginScreen } from './presentation/identity/profile/screens/LoginScreen';
import { AppErrorBoundary } from './presentation/shared/components/ErrorBoundary';
import { useThemeStore } from './application/stores/theme.store';

export default function App() {
  const isDarkMode = useThemeStore((state) => state.isDarkMode);

  // C-2: DOM manipulation moved from store to presentation layer
  useEffect(() => {
    document.body.classList.toggle('dark', isDarkMode);
  }, [isDarkMode]);

  return (
    <div className={isDarkMode ? 'dark' : ''}>
      <AppErrorBoundary>
        <BrowserRouter>
          <MainLayout>
            <Routes>
              <Route path="/"         element={<Navigate to="/tenants" replace />} />
              <Route path="/tenants"  element={<TenantDashboardScreen />} />
              <Route path="/profile"  element={<ProfileScreen />} />
              <Route path="/login"    element={<LoginScreen />} />
              <Route path="*"         element={<Navigate to="/tenants" replace />} />
            </Routes>
          </MainLayout>
        </BrowserRouter>
      </AppErrorBoundary>
    </div>
  );
}
