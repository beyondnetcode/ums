import { useEffect, lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { MainLayout } from './presentation/shared/layouts/MainLayout';
import { AppErrorBoundary } from './presentation/shared/components/ErrorBoundary';
import { RouteLoader } from './presentation/shared/components/RouteLoader';
import { useThemeStore } from './application/stores/theme.store';

const TenantDashboardScreen = lazy(() => import('./presentation/identity/tenant/screens/TenantDashboardScreen'));
const UserAccountDashboardScreen = lazy(() => import('./presentation/identity/user-account/screens/UserAccountDashboardScreen'));
const DelegationDashboardScreen = lazy(() => import('./presentation/identity/delegation/screens/DelegationDashboardScreen'));
const ProfileScreen = lazy(() => import('./presentation/identity/profile/screens/ProfileScreen'));
const LoginScreen = lazy(() => import('./presentation/identity/profile/screens/LoginScreen'));

export default function App() {
  const isDarkMode = useThemeStore((state) => state.isDarkMode);

  // C-2: DOM manipulation moved from store to presentation layer
  useEffect(() => {
    document.body.classList.toggle('dark', isDarkMode);
  }, [isDarkMode]);

  return (
    <div className={isDarkMode ? 'dark' : ''}>
      <AppErrorBoundary>
        <BrowserRouter future={{ v7_startTransition: true, v7_relativeSplatPath: true }}>
          <MainLayout>
            <Suspense fallback={<RouteLoader />}>
              <Routes>
                <Route path="/"             element={<Navigate to="/tenants" replace />} />
                <Route path="/tenants"      element={<TenantDashboardScreen />} />
                <Route path="/users"        element={<UserAccountDashboardScreen />} />
                <Route path="/delegations"  element={<DelegationDashboardScreen />} />
                <Route path="/profile"      element={<ProfileScreen />} />
                <Route path="/login"        element={<LoginScreen />} />
                <Route path="*"             element={<Navigate to="/tenants" replace />} />
              </Routes>
            </Suspense>
          </MainLayout>
        </BrowserRouter>
      </AppErrorBoundary>
    </div>
  );
}
