/**
 * LoginScreen.tsx
 *
 * Production-grade login screen.
 * Implements OWASP recommendations:
 *
 * - Input validation and sanitization
 * - No information leakage in error messages
 * - Rate limiting awareness (handled by backend)
 * - Secure password handling (no logging)
 * - Proper session management on success
 * - Accessible design with proper labels
 */
import React, { useState, useEffect, useCallback } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuthStore, detectBrowserTimezone } from '@app/stores/auth.store';
import { useI18nStore } from '@app/stores/i18n.store';
import type { SupportedLanguage } from '@app/stores/i18n.store';
import { useDevToolsStore } from '@app/stores/devTools.store';
import { useI18n } from '@app/i18n/use-i18n';
import { M3Card } from '@shared/components/M3Card';
import { M3Button } from '@shared/components/M3Button';
import { M3TextField } from '@shared/components/M3TextField';
import { TenantSelect } from '@shared/components/TenantSelect';
import { Key, Building2, ShieldCheck, AlertCircle, Info } from 'lucide-react';
import { useNotificationStore } from '@app/stores/notification.store';
import { DEV_TENANTS } from '@domain/identity/constants/tenant.constants';
import { ForgotPasswordForm } from '../components/ForgotPasswordForm';
import { authService } from '@app/identity/services/auth.service';
import { getHttpErrorMessage, getSupportReferenceId } from '@app/errors/http-error';
import { SignupForm } from '../components/SignupForm';
import { TenantSignupForm } from '../components/TenantSignupForm';
import { ProfileRequestForm } from '../components/ProfileRequestForm';

export default function LoginScreen(): React.JSX.Element {
  const navigate = useNavigate();
  const location = useLocation();
  const { login, isAuthenticated } = useAuthStore();
  const { setLanguage } = useI18nStore();
  const { setDevUserId } = useDevToolsStore();
  const addNotification = useNotificationStore(state => state.addNotification);
  const t = useI18n();

  const [tenantId, setTenantId] = useState(DEV_TENANTS[0].id);
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [attempts, setAttempts] = useState(0);
  const [lockoutUntil, setLockoutUntil] = useState<number | null>(null);
  const [showInfoPopup, setShowInfoPopup] = useState(false);
  const [authView, setAuthView] = useState<
    'login' | 'forgot' | 'signup' | 'tenant-signup' | 'profile-request'
  >('login');

  const showSessionExpired =
    new URLSearchParams(location.search).get('showSessionExpired') === 'true';
  const redirectTo = new URLSearchParams(location.search).get('redirect');

  useEffect(() => {
    if (isAuthenticated) {
      const destination = redirectTo || '/tenants';
      navigate(destination, { replace: true });
    }
  }, [isAuthenticated, navigate, redirectTo]);

  useEffect(() => {
    if (lockoutUntil && Date.now() < lockoutUntil) {
      const remaining = Math.ceil((lockoutUntil - Date.now()) / 1000);
      setError(`Demasiados intentos. Espere ${remaining} segundos`);
    } else if (lockoutUntil) {
      setLockoutUntil(null);
      setError('');
    }
  }, [lockoutUntil]);

  const handleLogin = useCallback(
    async (e: React.FormEvent) => {
      e.preventDefault();
      setError('');

      if (lockoutUntil && Date.now() < lockoutUntil) {
        return;
      }

      const trimmedUsername = username.trim();
      const trimmedPassword = password.trim();

      if (!trimmedUsername) {
        setError('Ingrese su correo electrónico');
        return;
      }
      if (!trimmedPassword) {
        setError('Ingrese su contraseña');
        return;
      }

      if (trimmedUsername.length < 2 || trimmedUsername.length > 100) {
        setError('Correo electrónico inválido');
        return;
      }

      setIsLoading(true);

      try {
        const selectedTenant = DEV_TENANTS.find(tn => tn.id === tenantId) ?? DEV_TENANTS[0];

        const data = await authService.login({
          tenantCode: selectedTenant.code,
          username: trimmedUsername,
          password: trimmedPassword,
          rememberMe: false,
        });

        setAttempts(0);
        setDevUserId(data.userId);

        // ADR-0076: Detect browser timezone (D2) and resolve language (D3).
        // Browser timezone wins over tenant default; browser Accept-Language (already applied
        // server-side) is reflected in data.language which wins over tenant parameter default.
        const browserTimezone = detectBrowserTimezone(
          data.sessionParameters?.defaultTimezone ?? 'America/Lima'
        );
        const resolvedParams = data.sessionParameters
          ? { ...data.sessionParameters, defaultTimezone: browserTimezone }
          : null;

        login(
          {
            id: data.userId,
            username: data.username,
            email: data.email,
            role: data.role ?? '',
            tenantId: data.tenantId,
            tenantCode: data.tenantCode,
            tenantName: data.tenantName,
            profileId: data.profileId ?? undefined,
            sessionTrackingId: data.sessionTrackingId,
            permissions: data.permissions,
            token: data.token,
            isInternalAdmin: data.isInternalAdmin,
            crossTenantAccessEnabled: false,
            sessionParameters: resolvedParams,
          },
          data.expiresIn * 1000,
          data.refreshExpiresIn * 1000
        );

        // ADR-0076 D3: Initialize UI language from server-resolved language
        // (backend already applied Accept-Language > tenant param > default chain).
        const supportedLangs: SupportedLanguage[] = ['es', 'en'];
        const lang = data.language as SupportedLanguage;
        if (supportedLangs.includes(lang)) {
          setLanguage(lang);
        }

        addNotification({
          title: 'Bienvenido',
          message: `Sesión iniciada en ${data.tenantCode}`,
          type: 'success',
        });

        const destination = redirectTo || '/tenants';
        navigate(destination, { replace: true });
      } catch (err) {
        const newAttempts = attempts + 1;
        setAttempts(newAttempts);

        if (newAttempts >= 5) {
          const lockDuration = Math.min(
            30 * 1000,
            10 * 1000 * Math.pow(2, Math.floor(newAttempts / 5))
          );
          setLockoutUntil(Date.now() + lockDuration);
          setError(`Demasiados intentos fallidos. Bloqueado por ${lockDuration / 1000} segundos`);
        } else {
          const supportReferenceId = getSupportReferenceId(err);
          const message = getHttpErrorMessage(
            err,
            'No pudimos iniciar sesión. Verifique sus credenciales.'
          );
          setError(supportReferenceId ? `${message} Referencia: ${supportReferenceId}` : message);
        }
      } finally {
        setIsLoading(false);
      }
    },
    [
      username,
      password,
      tenantId,
      attempts,
      lockoutUntil,
      login,
      navigate,
      addNotification,
      redirectTo,
      setDevUserId,
    ]
  );

  if (isAuthenticated) {
    return null;
  }

  const isLocked = lockoutUntil !== null && Date.now() < lockoutUntil;

  // ── Registration options shown on the right panel ──────────────────────────
  const REGISTRATION_OPTIONS = [
    {
      id: 'tenant-signup' as const,
      icon: <Building2 className="w-6 h-6" />,
      color: 'text-m3-primary',
      bg: 'bg-m3-primary/10 border-m3-primary/20',
      activeBg: 'bg-m3-primary/15 border-m3-primary/40',
      title: 'Registrar empresa',
      description:
        'Registra tu empresa en UMS para iniciar el proceso de onboarding y obtener acceso al sistema.',
      cta: 'Registrar mi empresa',
    },
    {
      id: 'signup' as const,
      icon: <Key className="w-6 h-6" />,
      color: 'text-emerald-600',
      bg: 'bg-emerald-500/10 border-emerald-500/20',
      activeBg: 'bg-emerald-500/15 border-emerald-500/40',
      title: 'Crear usuario',
      description:
        'Solicita una cuenta de usuario en un tenant existente. El administrador aprobará tu acceso.',
      cta: 'Solicitar acceso',
    },
    {
      id: 'profile-request' as const,
      icon: <ShieldCheck className="w-6 h-6" />,
      color: 'text-violet-600',
      bg: 'bg-violet-500/10 border-violet-500/20',
      activeBg: 'bg-violet-500/15 border-violet-500/40',
      title: 'Solicitar perfil',
      description:
        'Solicita acceso a un perfil o rol dentro de tu tenant para obtener permisos en el sistema.',
      cta: 'Solicitar perfil',
    },
  ] as const;

  const isRegistrationView =
    authView === 'signup' || authView === 'tenant-signup' || authView === 'profile-request';
  const isForgotPasswordView = authView === 'forgot';
  const showLoginForm = authView === 'login';

  return (
    <div className="min-h-screen flex items-center justify-center p-4 bg-gradient-to-br from-m3-surface via-m3-surface-container/30 to-m3-surface-container/50">
      <div className="w-full max-w-4xl">
        {/* ── Brand header ──────────────────────────────────────────────────── */}
        <div className="text-center mb-6">
          <div className="w-14 h-14 rounded-2xl bg-m3-primary/10 border border-m3-primary/20 flex items-center justify-center mx-auto mb-3">
            <Building2 className="w-7 h-7 text-m3-primary" />
          </div>
          <h1 className="text-2xl font-bold text-m3-on-surface tracking-tight">UMS</h1>
          <p className="text-sm text-m3-secondary mt-0.5">User Management System</p>
        </div>

        {showSessionExpired && (
          <div className="mb-4 p-3 rounded-lg bg-amber-500/10 border border-amber-500/30 flex items-center gap-2 max-w-md mx-auto">
            <AlertCircle className="w-4 h-4 text-amber-500 flex-shrink-0" />
            <p className="text-xs text-amber-500">
              Su sesión ha expirado. Inicie sesión nuevamente.
            </p>
          </div>
        )}

        {/* ── Main two-column layout ─────────────────────────────────────────── */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 items-start">
          {/* ── LEFT: Login form (or forgot password) ─────────────────────── */}
          <M3Card
            variant="elevated"
            className="p-6 border border-m3-outline/25 bg-m3-surface-container/30"
          >
            {isForgotPasswordView ? (
              <ForgotPasswordForm onBack={() => setAuthView('login')} />
            ) : isRegistrationView ? (
              // On mobile: show the selected form in the left card too
              <div className="lg:hidden">
                {authView === 'signup' && <SignupForm onBack={() => setAuthView('login')} />}
                {authView === 'tenant-signup' && (
                  <TenantSignupForm onBack={() => setAuthView('login')} />
                )}
                {authView === 'profile-request' && (
                  <ProfileRequestForm
                    onBack={() => setAuthView('login')}
                    onGoToLogin={() => setAuthView('login')}
                    onGoToSignup={() => setAuthView('signup')}
                  />
                )}
              </div>
            ) : null}

            {/* Show the login form only in the explicit login state */}
            {showLoginForm && (
              <div>
                <div className="flex items-center gap-2 mb-5">
                  <Key className="w-5 h-5 text-m3-primary" />
                  <h2 className="text-sm font-extrabold uppercase tracking-widest text-m3-on-surface">
                    Iniciar Sesión
                  </h2>
                </div>

                <form onSubmit={handleLogin} className="space-y-4">
                  <TenantSelect
                    label="Tenant"
                    value={tenantId}
                    onChange={setTenantId}
                    tenants={DEV_TENANTS}
                    placeholder="Buscar por código o nombre..."
                    disabled={isLoading || isLocked}
                  />

                  <M3TextField
                    label="Correo electrónico"
                    required
                    value={username}
                    onChange={e => setUsername(e.target.value)}
                    placeholder="usuario@empresa.com"
                    autoComplete="email"
                    disabled={isLoading || isLocked}
                    error={
                      error &&
                      (error.includes('correo') ||
                        error.includes('usuario') ||
                        error.includes('email'))
                        ? error
                        : undefined
                    }
                  />

                  <M3TextField
                    label="Contraseña"
                    type="password"
                    required
                    value={password}
                    onChange={e => setPassword(e.target.value)}
                    placeholder="••••••••"
                    autoComplete="current-password"
                    disabled={isLoading || isLocked}
                    error={error && error.includes('contraseña') ? error : undefined}
                  />

                  {error && !error.includes('usuario') && !error.includes('contraseña') && (
                    <div className="p-3 rounded-lg bg-m3-error-container/30 border border-m3-error/20 flex items-center gap-2">
                      <AlertCircle className="w-4 h-4 text-m3-error flex-shrink-0" />
                      <p className="text-xs text-m3-error">{error}</p>
                    </div>
                  )}

                  <M3Button
                    variant="filled"
                    className="w-full"
                    type="submit"
                    disabled={isLoading || isLocked}
                  >
                    {isLoading ? (
                      <span className="flex items-center gap-2">
                        <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24">
                          <circle
                            className="opacity-25"
                            cx="12"
                            cy="12"
                            r="10"
                            stroke="currentColor"
                            strokeWidth="4"
                            fill="none"
                          />
                          <path
                            className="opacity-75"
                            fill="currentColor"
                            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"
                          />
                        </svg>
                        Verificando...
                      </span>
                    ) : isLocked ? (
                      'Bloqueado'
                    ) : (
                      'Ingresar'
                    )}
                  </M3Button>

                  <div className="text-center pt-1">
                    <button
                      type="button"
                      onClick={() => setAuthView('forgot')}
                      className="text-xs text-m3-primary/70 hover:text-m3-primary transition-colors"
                    >
                      ¿Olvidaste tu contraseña?
                    </button>
                  </div>
                </form>
              </div>
            )}
          </M3Card>

          {/* ── RIGHT: Registration options or selected form ───────────────── */}
          <div className="space-y-3">
            {/* On desktop: show the active form in the right column */}
            {isRegistrationView && (
              <M3Card
                variant="elevated"
                className="p-6 border border-m3-outline/25 bg-m3-surface-container/30 hidden lg:block animate-fadeIn"
              >
                {authView === 'signup' && <SignupForm onBack={() => setAuthView('login')} />}
                {authView === 'tenant-signup' && (
                  <TenantSignupForm onBack={() => setAuthView('login')} />
                )}
                {authView === 'profile-request' && (
                  <ProfileRequestForm
                    onBack={() => setAuthView('login')}
                    onGoToLogin={() => setAuthView('login')}
                    onGoToSignup={() => setAuthView('signup')}
                  />
                )}
              </M3Card>
            )}

            {/* Registration options — always shown or collapsed */}
            <div>
              {!isRegistrationView && (
                <p className="text-[11px] font-semibold uppercase tracking-wider text-m3-secondary mb-2 px-1">
                  ¿Qué necesitas?
                </p>
              )}
              <div className="space-y-2">
                {REGISTRATION_OPTIONS.map(opt => {
                  const isActive = authView === opt.id;
                  return (
                    <button
                      key={opt.id}
                      type="button"
                      onClick={() => setAuthView(isActive ? 'login' : opt.id)}
                      className={[
                        'w-full flex items-start gap-4 p-4 rounded-2xl border text-left transition-all duration-200',
                        isActive
                          ? opt.activeBg
                          : 'border-m3-outline/20 bg-m3-surface-container/30 hover:border-m3-outline/40 hover:bg-m3-surface-container/50',
                      ].join(' ')}
                    >
                      <div
                        className={`p-2.5 rounded-xl border ${isActive ? opt.activeBg : opt.bg} ${opt.color} shrink-0`}
                      >
                        {opt.icon}
                      </div>
                      <div className="min-w-0 flex-1">
                        <p
                          className={`text-sm font-semibold ${isActive ? opt.color : 'text-m3-on-surface'}`}
                        >
                          {opt.title}
                        </p>
                        <p className="text-xs text-m3-secondary mt-0.5 leading-relaxed">
                          {opt.description}
                        </p>
                        <span
                          className={`inline-flex items-center gap-1 mt-2 text-[11px] font-medium ${isActive ? opt.color : 'text-m3-secondary/70'}`}
                        >
                          {isActive ? '← Volver al login' : `→ ${opt.cta}`}
                        </span>
                      </div>
                    </button>
                  );
                })}
              </div>
            </div>
          </div>
        </div>

        {/* ── Footer ────────────────────────────────────────────────────────── */}
        <div className="mt-5 flex items-center justify-center gap-4">
          <button
            type="button"
            onClick={() => setShowInfoPopup(true)}
            className="flex items-center gap-1 text-[10px] text-m3-secondary/60 hover:text-m3-secondary transition-colors"
          >
            <Info className="w-3 h-3" />
            <span>Datos de prueba</span>
          </button>
          <span className="text-[10px] text-m3-secondary/30">·</span>
          <p className="text-[10px] text-m3-secondary/50">
            Sistema protegido contra accesos no autorizados
          </p>
        </div>

        {/* ── Dev credentials popup ──────────────────────────────────────────── */}
        {showInfoPopup && (
          <>
            <div
              className="fixed inset-0 z-50 bg-black/50 backdrop-blur-sm"
              onClick={() => setShowInfoPopup(false)}
            />
            <div className="fixed z-50 left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 w-full max-w-sm p-4 rounded-xl border border-m3-outline/30 bg-m3-surface-container/95 shadow-2xl">
              <div className="flex items-center justify-between mb-3 pb-2 border-b border-m3-outline/20">
                <div className="flex items-center gap-2">
                  <ShieldCheck className="w-4 h-4 text-m3-primary" />
                  <p className="text-sm font-bold text-m3-on-surface">Credenciales de Prueba</p>
                </div>
                <button
                  type="button"
                  onClick={() => setShowInfoPopup(false)}
                  className="text-m3-secondary hover:text-m3-on-surface transition-colors"
                >
                  ✕
                </button>
              </div>
              <div className="space-y-2 text-xs max-h-80 overflow-y-auto">
                <div className="p-2 rounded-lg bg-m3-primary-container/30 border border-m3-primary/20">
                  <p className="font-bold text-m3-primary mb-1">
                    Admin Internal (ve todos los tenants)
                  </p>
                  <p className="text-m3-secondary font-mono">Tenant: INTERNAL_ADMIN</p>
                  <p className="text-m3-secondary font-mono">Usuario: admin@ums.local</p>
                  <p className="text-m3-secondary font-mono">Password: Admin@123</p>
                </div>
                <p className="font-semibold text-m3-on-surface mt-3 mb-1">Tenants Comerciales:</p>
                {DEV_TENANTS.filter(t => t.code !== 'INTERNAL_ADMIN').map(t => (
                  <div
                    key={t.id}
                    className="p-2 rounded bg-m3-surface/50 border border-m3-outline/10"
                  >
                    <p className="font-bold text-m3-primary">{t.name}</p>
                    <p className="text-m3-secondary font-mono">Tenant: {t.code}</p>
                    <p className="text-m3-secondary font-mono break-all">
                      Usuario: gerente.operaciones@
                      {t.code.toLowerCase().replace('_peru', '').replace('_', '.')}.pe
                    </p>
                    <p className="text-m3-secondary font-mono">Password: Admin@123</p>
                  </div>
                ))}
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
