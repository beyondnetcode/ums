import React, { useState, useEffect } from 'react';
import { UserCheck, ShieldAlert, Shield, ShieldCheck, ToggleLeft, ToggleRight, Loader2 } from 'lucide-react';
import { M3Button } from '@shared/components/M3Button';
import { M3FormDialog } from '@shared/components/M3FormDialog';
import { useGetAllTenants } from '@app/identity/hooks/use-tenant';
import { useGetAllUserAccounts } from '@app/identity/hooks/use-user-account';
import { useGetAllSystemSuites } from '@app/authorization/hooks/use-system-suite';
import { useRolesBySystemSuite } from '@app/authorization/hooks/use-role';
import { useGetAllPermissionTemplates, useGetPermissionTemplate } from '@app/authorization/hooks/use-permission-template';
import { useCreateProfile, useAssignProfileTemplate, useOverrideProfilePermission, useActivateProfilePermission, useDeactivateProfilePermission } from '@app/authorization/hooks/use-profile';
import profileService from '@infra/authorization/services/profile.service';

interface Props {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (profileId: string) => void;
}

interface LocalPermission {
  targetType: string;
  targetId: string;
  targetName: string;
  actionId: string;
  actionName: string;
  isAllowed: boolean;
  isDenied: boolean;
  isActive: boolean;
}

// Reusable Select Field
interface SelectProps {
  label: string;
  value: string;
  onChange: (v: string) => void;
  options: { value: string; label: string }[];
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

export const ProfileForm: React.FC<Props> = ({ isOpen, onClose, onSuccess }) => {
  const [tenantId, setTenantId] = useState('');
  const [userId, setUserId] = useState('');
  const [systemSuiteId, setSystemSuiteId] = useState('');
  const [roleId, setRoleId] = useState('');
  const [templateId, setTemplateId] = useState('');
  const [localPermissions, setLocalPermissions] = useState<LocalPermission[]>([]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  // Load Queries
  const { data: tenantPage, isLoading: loadingTenants } = useGetAllTenants({ page: 1, pageSize: 100 });
  const { data: userPage, isLoading: loadingUsers } = useGetAllUserAccounts({ page: 1, pageSize: 100, tenantId: tenantId || undefined });
  const { data: suitesPage, isLoading: loadingSuites } = useGetAllSystemSuites({ page: 1, pageSize: 100 });
  const { data: roles = [], isLoading: loadingRoles } = useRolesBySystemSuite(systemSuiteId);
  const { data: templatesPage, isLoading: loadingTemplates } = useGetAllPermissionTemplates({
    page: 1,
    pageSize: 100,
    tenantId: tenantId || undefined,
    systemSuiteId: systemSuiteId || undefined,
    roleId: roleId || undefined,
    status: 'Published'
  });

  const { data: activeTemplate, isLoading: loadingTemplateDetail } = useGetPermissionTemplate(templateId || null);

  const createMutation = useCreateProfile();
  const assignTemplateMutation = useAssignProfileTemplate();
  const overrideMutation = useOverrideProfilePermission();
  const activatePermMutation = useActivateProfilePermission();
  const deactivatePermMutation = useDeactivateProfilePermission();

  // Populate Options
  const tenantOptions = (tenantPage?.items ?? []).map(t => ({ value: t.tenantId, label: t.name }));
  const userOptions = (userPage?.items ?? []).map(u => ({ value: u.userId, label: `${u.userName} (${u.email})` }));
  const suiteOptions = (suitesPage?.items ?? []).map(s => ({ value: s.systemSuiteId, label: s.name }));
  const roleOptions = roles.map(r => ({ value: r.roleId, label: r.value }));
  const templateOptions = (templatesPage?.items ?? []).map(t => ({
    value: t.templateId,
    label: `Versión ${t.version} (${t.status})`
  }));

  // Load template items into memory when a template is selected
  useEffect(() => {
    if (activeTemplate) {
      const items = (activeTemplate.items ?? []).map(item => ({
        targetType: item.targetType,
        targetId: item.targetId,
        targetName: item.targetName,
        actionId: item.actionId,
        actionName: item.actionName,
        isAllowed: item.isAllowed,
        isDenied: item.isDenied,
        isActive: item.isActive,
      }));
      setLocalPermissions(items);
    } else {
      setLocalPermissions([]);
    }
  }, [activeTemplate]);

  const handleToggleActive = (index: number) => {
    setLocalPermissions(prev => prev.map((p, idx) => idx === index ? { ...p, isActive: !p.isActive } : p));
  };

  const handleChangeEffect = (index: number, effect: 'Allow' | 'Deny' | 'Neutral') => {
    setLocalPermissions(prev => prev.map((p, idx) => {
      if (idx === index) {
        return {
          ...p,
          isAllowed: effect === 'Allow',
          isDenied: effect === 'Deny'
        };
      }
      return p;
    }));
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (!tenantId) { setError('Inquilino es requerido'); return; }
    if (!userId) { setError('Usuario es requerido'); return; }
    if (!systemSuiteId) { setError('Sistema es requerido'); return; }
    if (!roleId) { setError('Rol es requerido'); return; }
    if (!templateId) { setError('Plantilla es requerida'); return; }

    setSaving(true);
    try {
      // 1. Create the profile aggregate in backend
      const createRes = await createMutation.mutateAsync({
        tenantId,
        userId,
        roleId,
        branchId: null
      });

      const profileId = createRes.profileId;

      // 2. Link the template in backend
      await assignTemplateMutation.mutateAsync({ profileId, templateId });

      // 3. Fetch the saved profile details from DB to retrieve generated permission IDs
      const savedProfile = await profileService.getById(profileId);

      // 4. Compare customized UI state and apply manual overrides
      for (const local of localPermissions) {
        // Find matching persisted record
        const dbPerm = savedProfile.permissions.find(p => p.targetId === local.targetId && p.actionId === local.actionId);
        if (!dbPerm) continue;

        const original = activeTemplate?.items.find(i => i.targetId === local.targetId && i.actionId === local.actionId);
        if (!original) continue;

        // Check if effect changed
        const effectChanged = local.isAllowed !== original.isAllowed || local.isDenied !== original.isDenied;
        if (effectChanged) {
          const newEffect = local.isAllowed ? 'allow' : local.isDenied ? 'deny' : 'neutral';
          await overrideMutation.mutateAsync({ profileId, permissionId: dbPerm.permissionId, effect: newEffect });
        }

        // Check if active status changed
        const activeChanged = local.isActive !== original.isActive;
        if (activeChanged) {
          if (local.isActive) {
            await activatePermMutation.mutateAsync({ profileId, permissionId: dbPerm.permissionId });
          } else {
            await deactivatePermMutation.mutateAsync({ profileId, permissionId: dbPerm.permissionId });
          }
        }
      }

      onSuccess(profileId);
    } catch (err: any) {
      logger.error('Error persisting profile authorizations', err);
      setError(err?.message || 'Error al persistir el perfil. Intente nuevamente.');
    } finally {
      setSaving(false);
    }
  };

  return (
    <M3FormDialog
      open={isOpen}
      onClose={onClose}
      title="Nuevo Perfil de Autorización"
      icon={<UserCheck className="w-4 h-4" />}
      maxWidth="max-w-3xl"
      footer={
        <>
          <M3Button type="button" variant="text" onClick={onClose} disabled={saving}>
            Cancelar
          </M3Button>
          <M3Button
            type="submit"
            form="profile-form"
            variant="filled"
            disabled={saving || loadingTemplateDetail}
          >
            {saving ? (
              <>
                <Loader2 className="w-4 h-4 animate-spin mr-1.5" />
                Persistiendo...
              </>
            ) : 'Crear & Persistir'}
          </M3Button>
        </>
      }
    >
      <form id="profile-form" onSubmit={handleSave} className="space-y-4">
        {error && (
          <div className="flex items-center gap-2 rounded-lg bg-m3-error-container/30 p-3 text-xs text-m3-error border border-m3-error/20">
            <ShieldAlert className="w-4 h-4 flex-shrink-0" />
            <span>{error}</span>
          </div>
        )}

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <FieldSelect
            label={loadingTenants ? 'Cargando inquilinos…' : 'Inquilino (Tenant)'}
            value={tenantId}
            onChange={(v) => { setTenantId(v); setUserId(''); setRoleId(''); setTemplateId(''); }}
            options={tenantOptions}
            disabled={loadingTenants || saving}
            required
          />

          <FieldSelect
            label={!tenantId ? 'Usuario (seleccione un Inquilino primero)' : loadingUsers ? 'Cargando usuarios…' : 'Usuario'}
            value={userId}
            onChange={setUserId}
            options={userOptions}
            disabled={!tenantId || loadingUsers || saving}
            required
          />

          <FieldSelect
            label={loadingSuites ? 'Cargando sistemas…' : 'Suite de Sistema (System)'}
            value={systemSuiteId}
            onChange={(v) => { setSystemSuiteId(v); setRoleId(''); setTemplateId(''); }}
            options={suiteOptions}
            disabled={loadingSuites || saving}
            required
          />

          <FieldSelect
            label={!systemSuiteId ? 'Rol (seleccione un Sistema primero)' : loadingRoles ? 'Cargando roles…' : 'Rol'}
            value={roleId}
            onChange={(v) => { setRoleId(v); setTemplateId(''); }}
            options={roleOptions}
            disabled={!systemSuiteId || loadingRoles || saving}
            required
          />
        </div>

        {tenantId && systemSuiteId && roleId && (
          <div className="pt-2">
            <FieldSelect
              label={loadingTemplates ? 'Cargando plantillas…' : 'Seleccionar Plantilla de Permisos'}
              value={templateId}
              onChange={setTemplateId}
              options={templateOptions}
              disabled={loadingTemplates || saving}
              required
            />
          </div>
        )}

        {/* Local interactive tree preview (unpersisted state) */}
        {templateId && (
          <div className="mt-4 rounded-xl border border-m3-outline/20 bg-m3-surface-container/20 overflow-hidden">
            <div className="border-b border-m3-outline/10 bg-m3-surface-container/40 px-4 py-2.5 flex items-center justify-between">
              <span className="text-xs font-semibold text-m3-on-surface">Vista Previa Local de Permisos (Sin Persistir)</span>
              <span className="text-[10px] text-amber-500 font-bold bg-amber-500/10 px-2 py-0.5 rounded-full border border-amber-500/20">Edición en Caliente</span>
            </div>

            {loadingTemplateDetail ? (
              <div className="flex flex-col items-center justify-center py-10 gap-2">
                <Loader2 className="w-6 h-6 animate-spin text-m3-primary" />
                <span className="text-xs text-m3-on-surface/60">Cargando árbol de permisos...</span>
              </div>
            ) : localPermissions.length === 0 ? (
              <div className="py-6 text-center text-xs text-m3-on-surface/50">La plantilla seleccionada no posee permisos asociados.</div>
            ) : (
              <div className="divide-y divide-m3-outline/5 max-h-[300px] overflow-y-auto px-1">
                {localPermissions.map((p, idx) => {
                  let effectVal: 'Allow' | 'Deny' | 'Neutral' = 'Neutral';
                  if (p.isAllowed) effectVal = 'Allow';
                  else if (p.isDenied) effectVal = 'Deny';

                  return (
                    <div key={idx} className="flex flex-col md:flex-row md:items-center justify-between gap-3 px-3 py-2 text-xs hover:bg-m3-surface-container/30 transition-colors">
                      <div className="flex flex-col gap-0.5">
                        <div className="flex items-center gap-1.5">
                          <span className="font-semibold text-m3-on-surface">{p.targetName}</span>
                          <span className="text-[9px] uppercase font-bold text-m3-primary bg-m3-primary/10 px-1.5 py-0.2 rounded border border-m3-primary/20">
                            {p.targetType}
                          </span>
                        </div>
                        <div className="text-[10px] text-m3-on-surface/60">
                          Acción: <span className="font-medium text-m3-secondary">{p.actionName}</span>
                        </div>
                      </div>

                      <div className="flex items-center gap-4 self-end md:self-auto">
                        {/* Allowed/Denied Toggle */}
                        <div className="flex items-center bg-m3-surface-container/60 rounded-lg p-0.5 border border-m3-outline/20">
                          <button
                            type="button"
                            onClick={() => handleChangeEffect(idx, 'Allow')}
                            className={`px-2 py-1 rounded-md text-[10px] font-bold transition-all ${p.isAllowed ? 'bg-emerald-500 text-white shadow-sm' : 'text-m3-on-surface/60 hover:text-m3-on-surface'}`}
                          >
                            <ShieldCheck className="w-3.5 h-3.5 inline mr-1" />
                            Allow
                          </button>
                          <button
                            type="button"
                            onClick={() => handleChangeEffect(idx, 'Neutral')}
                            className={`px-2 py-1 rounded-md text-[10px] font-bold transition-all ${!p.isAllowed && !p.isDenied ? 'bg-m3-outline/30 text-m3-on-surface' : 'text-m3-on-surface/60 hover:text-m3-on-surface'}`}
                          >
                            <Shield className="w-3.5 h-3.5 inline mr-1" />
                            Neutral
                          </button>
                          <button
                            type="button"
                            onClick={() => handleChangeEffect(idx, 'Deny')}
                            className={`px-2 py-1 rounded-md text-[10px] font-bold transition-all ${p.isDenied ? 'bg-rose-500 text-white shadow-sm' : 'text-m3-on-surface/60 hover:text-m3-on-surface'}`}
                          >
                            <ShieldAlert className="w-3.5 h-3.5 inline mr-1" />
                            Deny
                          </button>
                        </div>

                        {/* Active/Inactive Switch */}
                        <div className="flex items-center gap-1.5">
                          <span className={`text-[10px] font-medium ${p.isActive ? 'text-emerald-500' : 'text-m3-on-surface/40'}`}>
                            {p.isActive ? 'Activo' : 'Inactivo'}
                          </span>
                          <button
                            type="button"
                            onClick={() => handleToggleActive(idx)}
                            className="text-m3-primary hover:opacity-80 transition-opacity focus:outline-none"
                          >
                            {p.isActive ? (
                              <ToggleRight className="w-6 h-6 text-emerald-500 fill-emerald-500/25" />
                            ) : (
                              <ToggleLeft className="w-6 h-6 text-m3-outline" />
                            )}
                          </button>
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        )}
      </form>
    </M3FormDialog>
  );
};
