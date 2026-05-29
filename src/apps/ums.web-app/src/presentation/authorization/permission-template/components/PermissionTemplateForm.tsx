/**
 * PermissionTemplateForm — creates a new permission template (Draft).
 * Wrapped in M3FormDialog; uses suite/role dropdowns — no raw GUIDs.
 */
import React, { useState } from 'react';
import { ShieldPlus } from 'lucide-react';
import { M3Button, M3Select } from '@shared/components';
import { M3FormDialog } from '@shared/components/M3FormDialog';
import { useCreatePermissionTemplate } from '@app/authorization/hooks/use-permission-template';
import { useGetAllSystemSuites } from '@app/authorization/hooks/use-system-suite';
import { useRolesBySystemSuite } from '@app/authorization/hooks/use-role';
import { useAuthStore } from '@app/stores/auth.store';

interface Props {
  isOpen:    boolean;
  onClose:   () => void;
  onSuccess: (templateId: string) => void;
  tenantId?: string;
}

interface SelectProps {
  label:    string;
  value:    string;
  onChange: (v: string) => void;
  options:  { value: string; label: string }[];
  disabled?: boolean;
  required?: boolean;
  error?:    string;
}

const FieldSelect: React.FC<SelectProps> = ({ label, value, onChange, options, disabled, required, error }) => (
  <M3Select
    label={label}
    value={value}
    onChange={e => onChange(e.target.value)}
    disabled={disabled}
    required={required}
    error={error}
  >
    <option value="">— Seleccionar —</option>
    {options.map(o => (
      <option key={o.value} value={o.value}>{o.label}</option>
    ))}
  </M3Select>
);

export const PermissionTemplateForm: React.FC<Props> = ({ isOpen, onClose, onSuccess, tenantId }) => {
  const sessionTenantId = useAuthStore((state) => state.user?.tenantId);
  const effectiveTenantId = tenantId || sessionTenantId;

  const [systemSuiteIdVal, setSystemSuiteIdVal] = useState('');
  const [roleIdVal,        setRoleIdVal]        = useState('');
  const [error,            setError]            = useState('');

  const createMutation = useCreatePermissionTemplate();

  const { data: suitesPage, isLoading: loadingSuites } = useGetAllSystemSuites({
    page: 1, pageSize: 100,
    tenantId: effectiveTenantId,
  });

  const { data: roles = [], isLoading: loadingRoles } = useRolesBySystemSuite(systemSuiteIdVal);

  const suiteOptions = (suitesPage?.items ?? []).map(s => ({
    value: s.systemSuiteId,
    label: s.name,
  }));

  const roleOptions = roles.map(r => ({
    value: r.roleId,
    label: r.value,
  }));

  const handleSuiteChange = (id: string) => {
    setSystemSuiteIdVal(id);
    setRoleIdVal('');
    setError('');
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!effectiveTenantId)     { setError('Tenant context no disponible'); return; }
    if (!systemSuiteIdVal.trim()) { setError('Suite del Sistema es requerida'); return; }
    if (!roleIdVal.trim())        { setError('Rol es requerido');               return; }

    try {
      const result = await createMutation.mutateAsync({
        tenantId:      effectiveTenantId,
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
