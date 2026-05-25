import React, { useState } from 'react';
import { useCreateDelegation } from '@app/identity/hooks/use-delegation';
import { useI18n } from '@app/i18n/use-i18n';
import { M3Button } from '@shared/components/M3Button';
import { M3TextField } from '@shared/components/M3TextField';
import { M3Select } from '@shared/components/M3Select';
import { M3FormDialog } from '@shared/components/M3FormDialog';
import { CreateDelegationPayloadSchema } from '@domain/identity/schemas/delegation.schema';
import { Shield } from 'lucide-react';

const SCOPE_TYPES = ['Tenant', 'Organization', 'Department', 'System', 'Team'] as const;
const DELEGATED_ACTIONS = ['CreateUser', 'BlockUser', 'AssignProfile', 'ResetPassword', 'RevokeMfa'] as const;

const UNIMAR_USERS: Record<string, string> = {
  '5f4e3d01-1b0a-9f8e-7d6c-543210987654': 'Gerente UNIMAR',
  '5f4e3d02-1b0a-9f8e-7d6c-543210987654': 'Analista UNIMAR',
  '5f4e3d05-1b0a-9f8e-7d6c-543210987654': 'Socio UNIMAR',
};

interface DelegationFormProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  defaultTenantId?: string;
  defaultDelegatingAdminId?: string;
}

export const DelegationForm: React.FC<DelegationFormProps> = ({
  isOpen,
  onClose,
  onSuccess,
  defaultTenantId = '',
  defaultDelegatingAdminId = '',
}) => {
  const createDelegationMutation = useCreateDelegation();
  const t = useI18n();

  const [tenantId] = useState(defaultTenantId);
  const [delegatingAdminId] = useState(defaultDelegatingAdminId);
  // Default to first user in the list (excluding self if possible, but for mock any is fine)
  const [delegatedAdminId, setDelegatedAdminId] = useState('5f4e3d02-1b0a-9f8e-7d6c-543210987654');
  const [scopeType, setScopeType] = useState<(typeof SCOPE_TYPES)[number]>('Tenant');
  const [validFrom, setValidFrom] = useState('');
  const [validUntil, setValidUntil] = useState('');
  const [requiresApproval, setRequiresApproval] = useState(false);
  const [selectedActions, setSelectedActions] = useState<string[]>(['CreateUser']);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const toggleAction = (action: string) => {
    setSelectedActions((prev) =>
      prev.includes(action) ? prev.filter((a) => a !== action) : [...prev, action],
    );
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrors({});

    const payload = {
      tenantId,
      delegatingAdminId,
      delegatedAdminId,
      scopeType: scopeType as (typeof SCOPE_TYPES)[number],
      allowedActions: selectedActions as Array<'CreateUser' | 'BlockUser' | 'AssignProfile' | 'ResetPassword' | 'RevokeMfa'>,
      validFrom,
      validUntil,
      requiresApproval,
    };

    const result = CreateDelegationPayloadSchema.safeParse(payload);
    if (!result.success) {
      const fieldErrors: Record<string, string> = {};
      const flattened = result.error.flatten();
      Object.entries(flattened.fieldErrors).forEach(([key, msgs]) => {
        if (msgs?.[0]) fieldErrors[key] = msgs[0];
      });
      setErrors(fieldErrors);
      return;
    }

    try {
      await createDelegationMutation.mutateAsync(payload);
      setDelegatedAdminId('');
      setScopeType('Tenant');
      setValidFrom('');
      setValidUntil('');
      setRequiresApproval(false);
      setSelectedActions(['CreateUser']);
      onSuccess();
    } catch {
      // Handled by mutation hook
    }
  };

  return (
    <M3FormDialog
      open={isOpen}
      onClose={onClose}
      title={t.createDelegation ?? 'Crear Delegación'}
      icon={<Shield className="w-5 h-5" />}
      footer={
        <>
          <M3Button variant="text" onClick={onClose} type="button">
            {t.cancelBtn ?? 'Cancelar'}
          </M3Button>
          <M3Button variant="filled" onClick={handleSubmit} loading={createDelegationMutation.isPending}>
            {t.createBtn ?? 'Crear'}
          </M3Button>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="flex flex-col gap-5 pt-2">
        <M3Select
          label={t.delegatedAdminId ?? 'Delegar a (Usuario)'}
          value={delegatedAdminId}
          onChange={(e) => setDelegatedAdminId(e.target.value)}
        >
          <option value="" disabled>Seleccione un usuario...</option>
          {Object.entries(UNIMAR_USERS).map(([id, name]) => (
            <option key={id} value={id}>{name}</option>
          ))}
        </M3Select>

        <M3Select
          label={t.scopeType ?? 'Alcance (Scope)'}
          value={scopeType}
          onChange={(e) => setScopeType(e.target.value as typeof scopeType)}
        >
          {SCOPE_TYPES.map((s) => (
            <option key={s} value={s}>{s}</option>
          ))}
        </M3Select>

        <div className="grid grid-cols-2 gap-4">
          <M3TextField
            label={t.validFrom ?? 'Válido desde'}
            required
            type="datetime-local"
            value={validFrom}
            onChange={(e) => setValidFrom(e.target.value)}
            error={errors.validFrom}
          />
          <M3TextField
            label={t.validUntil ?? 'Válido hasta'}
            required
            type="datetime-local"
            value={validUntil}
            onChange={(e) => setValidUntil(e.target.value)}
            error={errors.validUntil}
          />
        </div>

        <div>
          <p className="text-xs font-bold uppercase tracking-wider text-m3-secondary mb-3">
            {t.allowedActions ?? 'Acciones Permitidas'}
          </p>
          <div className="flex flex-wrap gap-2">
            {DELEGATED_ACTIONS.map((action) => (
              <button
                key={action}
                type="button"
                onClick={() => toggleAction(action)}
                className={`px-3 py-1.5 text-xs rounded-full border transition-colors font-medium ${
                  selectedActions.includes(action)
                    ? 'bg-m3-primary/10 text-m3-primary border-m3-primary/30'
                    : 'bg-transparent text-m3-secondary border-m3-outline/30 hover:bg-m3-surface-variant'
                }`}
              >
                {action}
              </button>
            ))}
          </div>
          {errors.allowedActions && (
            <p className="text-xs text-rose-600 mt-2">{errors.allowedActions}</p>
          )}
        </div>

        <div className="flex items-center gap-3 mt-2 p-3 bg-m3-surface-container/30 rounded-xl border border-m3-outline/10">
          <input
            id="requiresApproval"
            type="checkbox"
            checked={requiresApproval}
            onChange={(e) => setRequiresApproval(e.target.checked)}
            className="w-4 h-4 text-m3-primary border-gray-300 rounded focus:ring-m3-primary"
          />
          <label htmlFor="requiresApproval" className="text-sm font-medium text-m3-on-surface cursor-pointer select-none">
            {t.requiresApproval ?? 'Requiere Aprobación'}
          </label>
        </div>
      </form>
    </M3FormDialog>
  );
};
