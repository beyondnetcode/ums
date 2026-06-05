import { useEffect, lazy, Suspense } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { MainLayout } from './presentation/shared/layouts/MainLayout';
import { AppErrorBoundary } from './presentation/shared/components/ErrorBoundary';
import { RouteLoader } from './presentation/shared/components/RouteLoader';
import { ProtectedRoute } from './presentation/shared/components/ProtectedRoute';
import { AccessGuard } from './presentation/shared/components/AccessGuard';
import { useThemeStore } from './application/stores/theme.store';
import { ShieldCheck } from 'lucide-react';

const UnauthorizedFallback = () => (
  <div className="flex flex-col items-center justify-center h-full p-8 text-m3-secondary animate-fadeIn">
    <ShieldCheck className="w-16 h-16 mb-4 opacity-30" />
    <h2 className="text-xl font-bold mb-2">Acceso Denegado</h2>
    <p className="text-sm opacity-70">
      No tienes permisos para visualizar este módulo o la función ha sido deshabilitada.
    </p>
  </div>
);

const TenantDashboardScreen = lazy(
  () => import('./presentation/identity/tenant/screens/TenantDashboardScreen')
);
const UserAccountDashboardScreen = lazy(
  () => import('./presentation/identity/user-account/screens/UserAccountDashboardScreen')
);
const DelegationDashboardScreen = lazy(
  () => import('./presentation/identity/delegation/screens/DelegationDashboardScreen')
);
const SystemSuiteDashboardScreen = lazy(
  () => import('./presentation/authorization/system-suite/screens/SystemSuiteDashboardScreen')
);
const PermissionTemplateDashboardScreen = lazy(
  () =>
    import(
      './presentation/authorization/permission-template/screens/PermissionTemplateDashboardScreen'
    )
);
const ProfileDashboardScreen = lazy(
  () => import('./presentation/authorization/profile/screens/ProfileDashboardScreen')
);
const FeatureFlagDashboardScreen = lazy(
  () => import('./presentation/configuration/feature-flag/screens/FeatureFlagDashboardScreen')
);
const AppConfigurationDashboardScreen = lazy(
  () =>
    import('./presentation/configuration/app-configuration/screens/AppConfigurationDashboardScreen')
);
const GlobalAppConfigurationDashboardScreen = lazy(
  () =>
    import(
      './presentation/configuration/app-configuration/screens/GlobalAppConfigurationDashboardScreen'
    )
);
const ParameterCatalogScreen = lazy(
  () => import('./presentation/configuration/parameter-catalog/screens/ParameterCatalogScreen')
);
const ProfileScreen = lazy(() => import('./presentation/identity/profile/screens/ProfileScreen'));
const LoginScreen = lazy(() => import('./presentation/identity/profile/screens/LoginScreen'));

export default function App() {
  const isDarkMode = useThemeStore(state => state.isDarkMode);

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
                <Route path="/" element={<Navigate to="/login" replace />} />
                <Route path="/login" element={<LoginScreen />} />
                <Route
                  path="/tenants"
                  element={
                    <ProtectedRoute>
                      <AccessGuard
                        moduleCode="IDM"
                        menuCode="TENANTS"
                        fallback={<UnauthorizedFallback />}
                      >
                        <TenantDashboardScreen />
                      </AccessGuard>
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="/users"
                  element={
                    <ProtectedRoute>
                      <AccessGuard
                        moduleCode="IDM"
                        menuCode="USERS"
                        fallback={<UnauthorizedFallback />}
                      >
                        <UserAccountDashboardScreen />
                      </AccessGuard>
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="/delegations"
                  element={
                    <ProtectedRoute>
                      <AccessGuard
                        moduleCode="IDM"
                        menuCode="DELEGATIONS"
                        fallback={<UnauthorizedFallback />}
                      >
                        <DelegationDashboardScreen />
                      </AccessGuard>
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="/system-suites"
                  element={
                    <ProtectedRoute>
                      <AccessGuard
                        moduleCode="AUTH"
                        menuCode="SYSTEM_SUITES"
                        fallback={<UnauthorizedFallback />}
                      >
                        <SystemSuiteDashboardScreen />
                      </AccessGuard>
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="/permission-templates"
                  element={
                    <ProtectedRoute>
                      <AccessGuard
                        moduleCode="AUTH"
                        menuCode="PERMISSION_TEMPLATES"
                        fallback={<UnauthorizedFallback />}
                      >
                        <PermissionTemplateDashboardScreen />
                      </AccessGuard>
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="/profiles"
                  element={
                    <ProtectedRoute>
                      <AccessGuard
                        moduleCode="AUTH"
                        menuCode="PROFILES"
                        fallback={<UnauthorizedFallback />}
                      >
                        <ProfileDashboardScreen />
                      </AccessGuard>
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="/feature-flags"
                  element={
                    <ProtectedRoute>
                      <AccessGuard
                        moduleCode="SYS"
                        menuCode="FEATURE_FLAGS"
                        fallback={<UnauthorizedFallback />}
                      >
                        <FeatureFlagDashboardScreen />
                      </AccessGuard>
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="/app-configurations"
                  element={
                    <ProtectedRoute>
                      <AccessGuard
                        moduleCode="SYS"
                        menuCode="APP_CONFIG"
                        fallback={<UnauthorizedFallback />}
                      >
                        <AppConfigurationDashboardScreen />
                      </AccessGuard>
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="/app-configurations/global"
                  element={
                    <ProtectedRoute>
                      <AccessGuard
                        moduleCode="SYS"
                        menuCode="APP_CONFIG"
                        fallback={<UnauthorizedFallback />}
                      >
                        <GlobalAppConfigurationDashboardScreen />
                      </AccessGuard>
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="/parameter-catalog"
                  element={
                    <ProtectedRoute>
                      <AccessGuard
                        moduleCode="SYS"
                        menuCode="PARAM_CATALOG"
                        fallback={<UnauthorizedFallback />}
                      >
                        <ParameterCatalogScreen />
                      </AccessGuard>
                    </ProtectedRoute>
                  }
                />
                <Route
                  path="/profile"
                  element={
                    <ProtectedRoute>
                      <ProfileScreen />
                    </ProtectedRoute>
                  }
                />
                <Route path="*" element={<Navigate to="/login" replace />} />
              </Routes>
            </Suspense>
          </MainLayout>
        </BrowserRouter>
      </AppErrorBoundary>
    </div>
  );
}
