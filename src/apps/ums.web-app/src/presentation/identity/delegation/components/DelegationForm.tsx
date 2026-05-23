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

  const [tenantId, setTenantId] = useState(defaultTenantId);
  const [delegatingAdminId, setDelegatingAdminId] = useState(defaultDelegatingAdminId);
  const [delegatedAdminId, setDelegatedAdminId] = useState('');
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
      title="Create Delegation"
      icon={<Shield className="w-5 h-5" />}
      footer={
        <>
          <M3Button variant="text" onClick={onClose} type="button">
            {t.cancelBtn}
          </M3Button>
          <M3Button variant="filled" onClick={handleSubmit} loading={createDelegationMutation.isPending}>
            Create
          </M3Button>
        </>
      }
    >
      <form onSubmit={handleSubmit} className="space-y-0">
        <M3TextField
          label="Delegating Admin ID"
          required
          value={delegatingAdminId}
          onChange={(e) => setDelegatingAdminId(e.target.value)}
          placeholder="UUID of the admin granting authority"
          error={errors.delegatingAdminId}
        />

        <M3TextField
          label="Delegated Admin ID"
          required
          value={delegatedAdminId}
          onChange={(e) => setDelegatedAdminId(e.target.value)}
          placeholder="UUID of the admin receiving authority"
          error={errors.delegatedAdminId}
        />

        <M3Select
          label="Scope Type"
          value={scopeType}
          onChange={(e) => setScopeType(e.target.value as typeof scopeType)}
        >
          {SCOPE_TYPES.map((s) => (
            <option key={s} value={s}>{s}</option>
          ))}
        </M3Select>

        <div className="grid grid-cols-2 gap-4">
          <M3TextField
            label="Valid From"
            required
            type="datetime-local"
            value={validFrom}
            onChange={(e) => setValidFrom(e.target.value)}
            error={errors.validFrom}
          />
          <M3TextField
            label="Valid Until"
            required
            type="datetime-local"
            value={validUntil}
            onChange={(e) => setValidUntil(e.target.value)}
            error={errors.validUntil}
          />
        </div>

        <div className="mt-4">
          <p className="text-xs font-medium text-m3-on-surface-variant mb-2">Allowed Actions</p>
          <div className="flex flex-wrap gap-2">
            {DELEGATED_ACTIONS.map((action) => (
              <button
                key={action}
                type="button"
                onClick={() => toggleAction(action)}
                className={`px-3 py-1 text-xs rounded-full border transition-colors ${
                  selectedActions.includes(action)
                    ? 'bg-m3-primary text-m3-on-primary border-m3-primary'
                    : 'bg-transparent text-m3-secondary border-m3-outline'
                }`}
              >
                {action}
              </button>
            ))}
          </div>
          {errors.allowedActions && (
            <p className="text-xs text-rose-600 mt-1">{errors.allowedActions}</p>
          )}
        </div>

        <div className="flex items-center gap-2 mt-4">
          <input
            id="requiresApproval"
            type="checkbox"
            checked={requiresApproval}
            onChange={(e) => setRequiresApproval(e.target.checked)}
            className="w-4 h-4"
          />
          <label htmlFor="requiresApproval" className="text-sm text-m3-on-surface">
            Requires Approval
          </label>
        </div>
      </form>
    </M3FormDialog>
  );
};
