import React, { useState } from 'react';
import { KeyRound, Mail, Copy, Check, ArrowLeft, ShieldAlert } from 'lucide-react';
import { M3Button } from '@shared/components/M3Button';
import { M3TextField } from '@shared/components/M3TextField';
import { TenantSelect } from '@shared/components/TenantSelect';
import { DEV_TENANTS } from '@domain/identity/constants/tenant.constants';
import authService from '@infra/identity/services/auth.service';

interface ForgotPasswordFormProps {
  onBack: () => void;
}

export const ForgotPasswordForm: React.FC<ForgotPasswordFormProps> = ({ onBack }) => {
  const [tenantId, setTenantId] = useState(DEV_TENANTS[0].id);
  const [email, setEmail] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [result, setResult] = useState<{ message: string; tempPassword: string | null } | null>(
    null
  );
  const [copied, setCopied] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!email.trim()) {
      setError('Ingrese su correo electrónico');
      return;
    }

    const selectedTenant = DEV_TENANTS.find(t => t.id === tenantId);
    if (!selectedTenant) return;

    setIsLoading(true);
    try {
      const response = await authService.forgotPassword({
        tenantCode: selectedTenant.code,
        email: email.trim().toLowerCase(),
      });
      setResult({
        message: response.message,
        tempPassword: response.simulatedTemporaryPassword,
      });
    } catch {
      setError('Error al procesar la solicitud. Intente nuevamente.');
    } finally {
      setIsLoading(false);
    }
  };

  const handleCopy = () => {
    if (!result?.tempPassword) return;
    navigator.clipboard.writeText(result.tempPassword);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  // ── Success state ────────────────────────────────────────────────────────
  if (result) {
    return (
      <div className="space-y-4 animate-fadeIn">
        <div className="p-4 rounded-xl border border-emerald-500/30 bg-emerald-500/5 space-y-3">
          <div className="flex items-center gap-2">
            <ShieldAlert className="w-4 h-4 text-emerald-500" />
            <span className="text-sm font-semibold text-emerald-600 dark:text-emerald-400">
              Solicitud procesada
            </span>
          </div>
          <p className="text-xs text-m3-secondary leading-relaxed">{result.message}</p>

          {result.tempPassword && (
            <div className="mt-2 p-3 rounded-lg border border-amber-500/30 bg-amber-500/5 space-y-2">
              <p className="text-[10px] font-semibold text-amber-600 dark:text-amber-400 uppercase tracking-wider">
                Modo simulado — contraseña temporal generada
              </p>
              <div className="flex items-center gap-2">
                <code className="flex-1 text-sm font-mono text-m3-on-surface bg-m3-surface-container px-3 py-2 rounded select-all break-all">
                  {result.tempPassword}
                </code>
                <button
                  onClick={handleCopy}
                  className="p-2 rounded border border-m3-outline/20 hover:bg-m3-surface-variant transition-colors shrink-0"
                  title="Copiar"
                >
                  {copied ? (
                    <Check className="w-4 h-4 text-emerald-500" />
                  ) : (
                    <Copy className="w-4 h-4 text-m3-secondary" />
                  )}
                </button>
              </div>
              <p className="text-[10px] text-m3-secondary/70">
                Use esta clave para ingresar. El sistema le pedirá crear una nueva contraseña.
              </p>
            </div>
          )}
        </div>

        <M3Button variant="outlined" className="w-full" onClick={onBack}>
          <ArrowLeft className="w-4 h-4 mr-2" />
          Volver al inicio de sesión
        </M3Button>
      </div>
    );
  }

  // ── Form state ───────────────────────────────────────────────────────────
  return (
    <form onSubmit={handleSubmit} className="space-y-4 animate-fadeIn">
      <div className="flex items-center gap-2 text-m3-primary mb-2">
        <KeyRound className="w-5 h-5" />
        <h2 className="text-sm font-extrabold uppercase tracking-widest text-m3-on-surface">
          Recuperar Contraseña
        </h2>
      </div>

      <p className="text-xs text-m3-secondary leading-relaxed">
        Ingrese su tenant y correo electrónico. Si existe una cuenta asociada, recibirá
        instrucciones para restablecer su contraseña.
      </p>

      <TenantSelect
        label="Tenant"
        value={tenantId}
        onChange={setTenantId}
        tenants={DEV_TENANTS}
        placeholder="Buscar por código o nombre..."
        disabled={isLoading}
      />

      <M3TextField
        label="Correo electrónico"
        type="email"
        required
        value={email}
        onChange={e => {
          setEmail(e.target.value);
          setError('');
        }}
        placeholder="usuario@empresa.com"
        autoComplete="email"
        disabled={isLoading}
        icon={<Mail className="w-4 h-4" />}
      />

      {error && <p className="text-xs text-m3-error px-1">{error}</p>}

      <M3Button variant="filled" className="w-full" type="submit" disabled={isLoading}>
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
            Procesando...
          </span>
        ) : (
          'Enviar instrucciones'
        )}
      </M3Button>

      <button
        type="button"
        onClick={onBack}
        className="w-full flex items-center justify-center gap-1.5 text-xs text-m3-secondary hover:text-m3-primary transition-colors py-1"
      >
        <ArrowLeft className="w-3.5 h-3.5" />
        Volver al inicio de sesión
      </button>
    </form>
  );
};
