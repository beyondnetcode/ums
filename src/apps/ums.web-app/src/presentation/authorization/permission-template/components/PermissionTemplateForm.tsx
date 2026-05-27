/**
 * PermissionTemplateForm — creates a new permission template (Draft).
 * Wrapped in M3FormDialog; uses suite/role dropdowns — no raw GUIDs.
 */
import React, { useState } from 'react';
import { ShieldPlus } from 'lucide-react';
import { M3Button } from '@shared/components/M3Button';
import { M3FormDialog } from '@shared/components/M3FormDialog';
import { useCreatePermissionTemplate } from '@app/authorization/hooks/use-permission-template';
import { useGetAllSystemSuites } from '@app/authorization/hooks/use-system-suite';
import { useRolesBySystemSuite } from '@app/authorization/hooks/use-role';

interface Props {
  isOpen:    boolean;
  onClose:   () => void;
  onSuccess: (templateId: string) => void;
  tenantId?: string;
}

// ── Small reusable select ─────────────────────────────────────────────────────

interface SelectProps {
  label:    string;
  value:    string;
  onChange: (v: string) => void;
  options:  { value: string; label: string }[];
  disabled?: boolean;
  required?: boolean;
}

const FieldSelect: React.FC<SelectProps> = ({ label, value, onChange, options, disabled, required }) => (
  <div className="flex flex-col gap-1">
    <label className="text-xs font-medium text-m3-on-surface/70">
      {label}{required && <span className="text-m3-error ml-0.5">*</span>}
    </label>
    <select
      value={value}
      onChange={e => onChange(e.target.value)}
      disabled={disabled}
      required={required}
      className="w-full rounded-lg border border-m3-outline/40 bg-m3-surface-container/40 px-3 py-2 text-sm text-m3-on-surface focus:outline-none focus:ring-2 focus:ring-m3-primary/40 focus:border-m3-primary disabled:opacity-50 disabled:cursor-not-allowed"
    >
      <option value="">— Seleccionar —</option>
      {options.map(o => (
        <option key={o.value} value={o.value}>{o.label}</option>
      ))}
    </select>
  </div>
);

// ── Main form ─────────────────────────────────────────────────────────────────

export const PermissionTemplateForm: React.FC<Props> = ({ isOpen, onClose, onSuccess, tenantId }) => {
  const [tenantIdVal,      setTenantIdVal]      = useState(tenantId ?? '');
  const [systemSuiteIdVal, setSystemSuiteIdVal] = useState('');
  const [roleIdVal,        setRoleIdVal]        = useState('');
  const [error,            setError]            = useState('');

  const createMutation = useCreatePermissionTemplate();

  // Load suites (large page to get all available)
  const { data: suitesPage, isLoading: loadingSuites } = useGetAllSystemSuites({
    page: 1, pageSize: 100,
  });

  // Load roles for the selected suite
  const { data: roles = [], isLoading: loadingRoles } = useRolesBySystemSuite(systemSuiteIdVal);

  const suiteOptions = (suitesPage?.items ?? []).map(s => ({
    value: s.systemSuiteId,
    label: s.name,
  }));

  const roleOptions = roles.map(r => ({
    value: r.roleId,
    label: r.value,   // role display name
  }));

  const handleSuiteChange = (id: string) => {
    setSystemSuiteIdVal(id);
    setRoleIdVal('');   // reset role when suite changes
    setError('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!tenantIdVal.trim())      { setError('Tenant ID es requerido');        return; }
    if (!systemSuiteIdVal.trim()) { setError('Suite del Sistema es requerida'); return; }
    if (!roleIdVal.trim())        { setError('Rol es requerido');               return; }

    try {
      const result = await createMutation.mutateAsync({
        tenantId:      tenantIdVal.trim(),
        systemSuiteId: systemSuiteIdVal.trim(),
        roleId:        roleIdVal.trim(),
      });
      onSuccess(result.templateId);
    } catch { /* handled by hook */ }
  };

  return (
    <M3FormDialog
      open={isOpen}
      onClose={onClose}
      title="Nueva Plantilla de Permisos"
      icon={<ShieldPlus className="w-4 h-4" />}
      maxWidth="max-w-md"
      footer={
        <>
          <M3Button type="button" variant="text" onClick={onClose} disabled={createMutation.isPending}>
            Cancelar
          </M3Button>
          <M3Button
            type="submit"
            form="permission-template-form"
            variant="filled"
            disabled={createMutation.isPending}
          >
            {createMutation.isPending ? 'Creando…' : 'Crear Plantilla'}
          </M3Button>
        </>
      }
    >
      <form id="permission-template-form" onSubmit={handleSubmit} className="space-y-4">
        <p className="text-[11px] text-m3-secondary">
          La plantilla se crea en estado <span className="font-bold text-amber-500">Borrador</span>.
          Agrega los ítems de permiso antes de publicarla.
        </p>

        {/* Tenant ID — text field only when not pre-supplied */}
        {!tenantId && (
          <div className="flex flex-col gap-1">
            <label className="text-xs font-medium text-m3-on-surface/70">
              Tenant ID<span className="text-m3-error ml-0.5">*</span>
            </label>
            <input
              type="text"
              required
              value={tenantIdVal}
              onChange={e => { setTenantIdVal(e.target.value); setError(''); }}
              placeholder="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
              className="w-full rounded-lg border border-m3-outline/40 bg-m3-surface-container/40 px-3 py-2 text-sm text-m3-on-surface focus:outline-none focus:ring-2 focus:ring-m3-primary/40 focus:border-m3-primary font-mono"
            />
          </div>
        )}

        <FieldSelect
          label={loadingSuites ? 'Cargando suites…' : 'Suite del Sistema'}
          value={systemSuiteIdVal}
          onChange={handleSuiteChange}
          options={suiteOptions}
          disabled={loadingSuites}
          required
        />

        <FieldSelect
          label={
            !systemSuiteIdVal
              ? 'Rol (selecciona una Suite primero)'
              : loadingRoles
                ? 'Cargando roles…'
                : 'Rol'
          }
          value={roleIdVal}
          onChange={v => { setRoleIdVal(v); setError(''); }}
          options={roleOptions}
          disabled={!systemSuiteIdVal || loadingRoles}
          required
        />

        {error && <p className="text-[11px] text-m3-error">{error}</p>}
      </form>
    </M3FormDialog>
  );
};
