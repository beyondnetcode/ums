import React from 'react';
import {
  useActivateTenant,
  useSuspendTenant,
} from '@app/identity/hooks/use-tenant';
import { useI18n } from '@app/i18n/use-i18n';
import { useStatusLabel } from '@app/hooks/use-status-label';
import { useInlineEdit } from '@app/hooks/use-inline-edit';
import { useResetOnChange } from '@app/hooks/use-reset-on-change';
import { M3Card } from '@shared/components/M3Card';
import { M3Button } from '@shared/components/M3Button';
import { M3TextField } from '@shared/components/M3TextField';
import { M3Select } from '@shared/components/M3Select';
import { M3Dialog } from '@shared/components/M3Dialog';
import { StatusBadge } from '@shared/components/StatusBadge';
import { CodeBadge } from '@shared/components/CodeBadge';
import { IconButton } from '@shared/components/Tooltip';
import { useNotificationStore } from '@app/stores/notification.store';
import { getHttpErrorMessage } from '@app/errors/http-error';
import { Tenant } from '@domain/identity/models/tenant.model';
import { TENANT_TYPES } from '@domain/identity/constants/tenant.constants';
import {
  Building,
  Sliders,
  ShieldAlert,
  CheckCircle2,
  Pencil,
  Save,
  X,
  GitBranch,
} from 'lucide-react';

// ─── Props ───────────────────────────────────────────────────────────────────

interface TenantProfileCardProps {
  tenant: Tenant;
  parentTenant: Tenant | null;
  onTenantUpdate: (tenantId: string, patch: Partial<Tenant>) => void;
  onEditingChange?: (isEditing: boolean) => void;
}

// ─── Inline-edit draft shape ────────────────────────────────────────────────

interface TenantDraft {
  name: string;
  code: string;
  companyReference: string;
  type: string;
}

// ─── Component ───────────────────────────────────────────────────────────────

export const TenantProfileCard: React.FC<TenantProfileCardProps> = ({
  tenant,
  parentTenant,
  onTenantUpdate,
  onEditingChange,
}) => {
  const t = useI18n();
  const statusLabel = useStatusLabel();
  const addNotification = useNotificationStore((s) => s.addNotification);

  const activateMutation = useActivateTenant(tenant.tenantId);
  const suspendMutation = useSuspendTenant(tenant.tenantId);
  const isPendingMutation = activateMutation.isPending || suspendMutation.isPending;

  const edit = useInlineEdit<TenantDraft>(['name', 'code', 'companyReference', 'type']);
  const [showDiscardDialog, setShowDiscardDialog] = React.useState(false);

  // Reset editing state when the tenant changes
  useResetOnChange(tenant.tenantId, () => {
    edit.cancelEdit();
    onEditingChange?.(false);
  });

  const openTenantEdit = () => {
    edit.openEdit(tenant.tenantId, {
      name: tenant.name,
      code: tenant.code,
      companyReference: tenant.companyReference || '',
      type: tenant.type,
    });
    onEditingChange?.(true);
  };

  const cancelTenantEdit = () => {
    edit.cancelEdit();
    onEditingChange?.(false);
  };

  const saveTenantEdit = () => {
    const name = (edit.draft.name ?? '').trim();
    const code = (edit.draft.code ?? '').trim();
    if (!name || !code) return;

    const result = edit.commitEdit();
    if (!result) return;

    onTenantUpdate(tenant.tenantId, {
      name,
      code: code.toUpperCase(),
      companyReference: (result.draft.companyReference ?? '').trim(),
      type: result.draft.type,
    });
    addNotification({ title: t.notifTenantUpdated, message: t.notifTenantUpdatedMsg(name), type: 'success' });
    onEditingChange?.(false);
  };

  const handleToggleStatus = (newStatus: 'Active' | 'Suspended') => {
    onTenantUpdate(tenant.tenantId, { status: newStatus });
    addNotification({
      title: newStatus === 'Active' ? t.notifActivated : t.notifSuspended,
      message: t.notifStatusSetTo(newStatus),
      type: newStatus === 'Active' ? 'success' : 'warning',
    });
    if (newStatus === 'Active') {
      activateMutation.mutate(undefined, { onError: (err) => console.warn('Handled locally:', getHttpErrorMessage(err, 'Activation failed')) });
    } else {
      suspendMutation.mutate(undefined, { onError: (err) => console.warn('Handled locally:', getHttpErrorMessage(err, 'Suspension failed')) });
    }
  };

  return (
    <>
      {/* Discard-changes dialog */}
      <M3Dialog
        open={showDiscardDialog}
        title={t.unsavedChanges}
        message={t.unsavedChangesMsg}
        onScrimClick={() => setShowDiscardDialog(false)}
        actions={[
          { label: t.cancelEdit, variant: 'outlined', onClick: () => setShowDiscardDialog(false) },
          {
            label: t.discardChanges,
            variant: 'filled',
            className: 'bg-m3-error hover:bg-m3-error/90 border-0',
            onClick: () => {
              setShowDiscardDialog(false);
              cancelTenantEdit();
            },
          },
        ]}
      />

      {/* Profile card */}
      <M3Card
        variant="elevated"
        className="p-5 border border-m3-outline/25 bg-m3-surface-container/20 shadow-sm group"
        onDoubleClick={() => !edit.hasEditing && openTenantEdit()}
      >
        {!edit.hasEditing ? (
          <>
            <div className="flex justify-between items-start gap-4 pb-3.5 border-b border-m3-outline/15 mb-4">
              <div className="flex gap-3 flex-1 min-w-0">
                <div className="p-2 bg-m3-primary/10 rounded-lg text-m3-primary border border-m3-primary/10 self-start flex-shrink-0">
                  <Building className="w-5 h-5" />
                </div>
                <div className="min-w-0">
                  <h3 className="text-sm font-semibold text-m3-on-surface flex items-center gap-1.5 flex-wrap">
                    {tenant.name}
                    <CodeBadge code={tenant.code} />
                  </h3>
                  {tenant.companyReference && (
                    <p className="text-xs text-m3-secondary mt-0.5">{tenant.companyReference}</p>
                  )}
                  {parentTenant && (
                    <div className="flex items-center gap-1 mt-1">
                      <GitBranch className="w-3 h-3 text-m3-tertiary flex-shrink-0" />
                      <span className="text-[10px] text-m3-tertiary font-medium truncate">
                        {parentTenant.name}
                      </span>
                    </div>
                  )}
                </div>
              </div>
              <div className="flex items-center gap-2 flex-shrink-0">
                <StatusBadge status={tenant.status} label={statusLabel(tenant.status)} />
                <IconButton tooltip={t.editTenant} onClick={openTenantEdit} className="opacity-0 group-hover:opacity-100">
                  <Pencil className="w-3.5 h-3.5" />
                </IconButton>
              </div>
            </div>

            <div className="flex items-center justify-between text-xs">
              <div className="flex items-center gap-1.5 text-m3-secondary font-medium">
                <Sliders className="w-3.5 h-3.5" />
                <span>{t.stateControls}</span>
              </div>
              {tenant.status === 'Active' ? (
                <M3Button
                  variant="outlined"
                  onClick={() => handleToggleStatus('Suspended')}
                  loading={isPendingMutation}
                  className="text-rose-500 border-rose-500/30 hover:bg-rose-500/10"
                >
                  <ShieldAlert className="w-3.5 h-3.5 mr-1.5" /> {t.suspendBtn}
                </M3Button>
              ) : (
                <M3Button
                  variant="filled"
                  onClick={() => handleToggleStatus('Active')}
                  loading={isPendingMutation}
                  className="bg-emerald-600 hover:bg-emerald-500"
                >
                  <CheckCircle2 className="w-3.5 h-3.5 mr-1.5" /> {t.activateBtn}
                </M3Button>
              )}
            </div>

            <p className="text-xs text-m3-secondary/50 mt-3 text-center">{t.doubleClickToEdit}</p>
          </>
        ) : (
          /* Tenant inline-edit form */
          <div className="space-y-0 animate-fadeIn">
            <div className="flex items-center justify-between mb-4">
              <span className="text-sm font-medium text-m3-primary flex items-center gap-1.5">
                <Pencil className="w-3.5 h-3.5" /> {t.editTenant}
              </span>
              <IconButton tooltip={t.cancelEdit} onClick={cancelTenantEdit}>
                <X className="w-3.5 h-3.5" />
              </IconButton>
            </div>

            <M3TextField label={t.tenantName} required value={edit.draft.name ?? ''} onChange={(e) => edit.setField('name', e.target.value)} />
            <M3TextField label={t.tenantCode} required value={edit.draft.code ?? ''} onChange={(e) => edit.setField('code', e.target.value.toUpperCase())} />
            <M3TextField label={t.companyReference} value={edit.draft.companyReference ?? ''} onChange={(e) => edit.setField('companyReference', e.target.value)} />

            <M3Select label={t.tenantType} value={edit.draft.type ?? ''} onChange={(e) => edit.setField('type', e.target.value)}>
              {TENANT_TYPES.map((tp) => <option key={tp} value={tp}>{tp}</option>)}
            </M3Select>

            <div className="flex gap-2 pt-1">
              <M3Button variant="filled" onClick={saveTenantEdit} className="flex-1 flex items-center justify-center gap-1.5">
                <Save className="w-3.5 h-3.5" /> {t.saveBtn}
              </M3Button>
              <M3Button variant="outlined" onClick={cancelTenantEdit} className="flex-1">
                {t.cancelEdit}
              </M3Button>
            </div>
          </div>
        )}
      </M3Card>
    </>
  );
};

