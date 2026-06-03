import { useEffect, lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { MainLayout } from './presentation/shared/layouts/MainLayout';
import { AppErrorBoundary } from './presentation/shared/components/ErrorBoundary';
import { RouteLoader } from './presentation/shared/components/RouteLoader';
import { ProtectedRoute } from './presentation/shared/components/ProtectedRoute';
import { useThemeStore } from './application/stores/theme.store';

const TenantDashboardScreen = lazy(() => import('./presentation/identity/tenant/screens/TenantDashboardScreen'));
const UserAccountDashboardScreen = lazy(() => import('./presentation/identity/user-account/screens/UserAccountDashboardScreen'));
const DelegationDashboardScreen = lazy(() => import('./presentation/identity/delegation/screens/DelegationDashboardScreen'));
const SystemSuiteDashboardScreen = lazy(() => import('./presentation/authorization/system-suite/screens/SystemSuiteDashboardScreen'));
const PermissionTemplateDashboardScreen = lazy(() => import('./presentation/authorization/permission-template/screens/PermissionTemplateDashboardScreen'));
const ProfileDashboardScreen = lazy(() => import('./presentation/authorization/profile/screens/ProfileDashboardScreen'));
const FeatureFlagDashboardScreen = lazy(() => import('./presentation/configuration/feature-flag/screens/FeatureFlagDashboardScreen'));
const AppConfigurationDashboardScreen = lazy(() => import('./presentation/configuration/app-configuration/screens/AppConfigurationDashboardScreen'));
const GlobalAppConfigurationDashboardScreen = lazy(() => import('./presentation/configuration/app-configuration/screens/GlobalAppConfigurationDashboardScreen'));
const ParameterCatalogScreen = lazy(() => import('./presentation/configuration/parameter-catalog/screens/ParameterCatalogScreen'));
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
                <Route path="/"             element={<Navigate to="/login" replace />} />
                <Route path="/login"        element={<LoginScreen />} />
                <Route path="/tenants"      element={<ProtectedRoute><TenantDashboardScreen /></ProtectedRoute>} />
                <Route path="/users"        element={<ProtectedRoute><UserAccountDashboardScreen /></ProtectedRoute>} />
                <Route path="/delegations"  element={<ProtectedRoute><DelegationDashboardScreen /></ProtectedRoute>} />
                <Route path="/system-suites"         element={<ProtectedRoute><SystemSuiteDashboardScreen /></ProtectedRoute>} />
                <Route path="/permission-templates"  element={<ProtectedRoute><PermissionTemplateDashboardScreen /></ProtectedRoute>} />
                <Route path="/profiles"              element={<ProtectedRoute><ProfileDashboardScreen /></ProtectedRoute>} />
                <Route path="/feature-flags"         element={<ProtectedRoute><FeatureFlagDashboardScreen /></ProtectedRoute>} />
                <Route path="/app-configurations"    element={<ProtectedRoute><AppConfigurationDashboardScreen /></ProtectedRoute>} />
                <Route path="/app-configurations/global" element={<ProtectedRoute><GlobalAppConfigurationDashboardScreen /></ProtectedRoute>} />
                <Route path="/parameter-catalog"     element={<ProtectedRoute><ParameterCatalogScreen /></ProtectedRoute>} />
                <Route path="/profile"               element={<ProtectedRoute><ProfileScreen /></ProtectedRoute>} />
                <Route path="*"             element={<Navigate to="/login" replace />} />
              </Routes>
            </Suspense>
          </MainLayout>
        </BrowserRouter>
      </AppErrorBoundary>
    </div>
  );
}
