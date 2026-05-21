import React, { useState } from 'react';
import {
  useGetBranches,
  useAddBranch,
  useRemoveBranch,
  useDeactivateBranch,
  useReactivateBranch,
} from '@app/identity/hooks/use-branch';
import { useI18n } from '@app/i18n/use-i18n';
import { useInlineEdit } from '@app/hooks/use-inline-edit';
import { useNotificationStore } from '@app/stores/notification.store';
import { M3Button } from '@shared/components/M3Button';
import { M3TextField } from '@shared/components/M3TextField';
import { StatusBadge } from '@shared/components/StatusBadge';
import { EntityRow } from '@shared/components/EntityRow';
import { InlineAddForm } from '@shared/components/InlineAddForm';
import { EmptyState } from '@shared/components/EmptyState';
import { SectionHeader } from '@shared/components/SectionHeader';
import { CodeBadge } from '@shared/components/CodeBadge';
import { IconButton } from '@shared/components/Tooltip';
import { RefreshCw, MapPin, EyeOff, ShieldCheck, Trash2, Pencil, Save, X } from 'lucide-react';

interface BranchManagerProps {
  tenantId: string;
}

interface BranchDraft {
  name: string;
  code: string;
  geofencingMetadata: string;
}

export const BranchManager: React.FC<BranchManagerProps> = ({ tenantId }) => {
  const { data: branches = [], isLoading, refetch, isFetching } = useGetBranches(tenantId);
  const addBranchMutation = useAddBranch(tenantId);
  const removeBranchMutation = useRemoveBranch(tenantId);
  const deactivateBranchMutation = useDeactivateBranch(tenantId);
  const reactivateBranchMutation = useReactivateBranch(tenantId);
  const t = useI18n();
  const addNotification = useNotificationStore((s) => s.addNotification);

  // ── Add form ──────────────────────────────────────────────────────────────
  const [isAdding, setIsAdding] = useState(false);
  const [code, setCode] = useState('');
  const [name, setName] = useState('');
  const [geofencing, setGeofencing] = useState('');
  const [errorMsg, setErrorMsg] = useState('');

  // ── Inline edit ───────────────────────────────────────────────────────────
  const edit = useInlineEdit<BranchDraft>(['name', 'code', 'geofencingMetadata']);

  const openBranchEdit = (b: { branchId: string; name: string; code: string; geofencingMetadata?: string | null }) => {
    edit.openEdit(b.branchId, {
      name: b.name,
      code: b.code,
      geofencingMetadata: b.geofencingMetadata ?? '',
    });
  };

  const saveBranchEdit = () => {
    const result = edit.commitEdit();
    if (result) {
      addNotification({
        title: t.notifBranchUpdated,
        message: `${t.notifBranchUpdated}: ${result.draft.name}`,
        type: 'success',
      });
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg('');

    if (code.length < 3) { setErrorMsg(t.branchCodeHelper); return; }
    if (name.length < 3) { setErrorMsg(t.branchNameHelper); return; }

    try {
      await addBranchMutation.mutateAsync({
        code: code.toUpperCase().replace(/\s+/g, '_'),
        name,
        geofencingMetadata: geofencing || undefined,
      });
      setCode(''); setName(''); setGeofencing(''); setIsAdding(false);
    } catch {
      // Handled in mutation hook
    }
  };

  return (
    <div className="space-y-4 select-none">
      <SectionHeader
        title={t.subLocations}
        subtitle={t.subLocationsSubtitle}
        actions={
          <IconButton tooltip={t.refreshBranches} onClick={() => refetch()} className={isFetching ? 'animate-spin' : ''}>
            <RefreshCw className="w-3.5 h-3.5" />
          </IconButton>
        }
      />

      <div className="grid grid-cols-1 gap-4">
        <InlineAddForm
          isOpen={isAdding}
          onToggle={(open) => { setIsAdding(open); if (!open) setErrorMsg(''); }}
          onSubmit={handleSubmit}
          addLabel={t.addLocation}
          title={t.addingLocation}
          cancelLabel={t.cancelEdit}
          submitLabel={t.saveLocation}
          isLoading={addBranchMutation.isPending}
          error={errorMsg || undefined}
        >
          <M3TextField
            label={t.branchCode}
            required
            value={code}
            onChange={(e) => setCode(e.target.value)}
            placeholder="e.g. SUC_MIRAFLORES"
          />
          <M3TextField
            label={t.branchName}
            required
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="e.g. Sucursal Miraflores"
          />
          <M3TextField
            label={t.geofencingMeta}
            value={geofencing}
            onChange={(e) => setGeofencing(e.target.value)}
            placeholder="e.g. -12.1191,-77.0291;r=500"
          />
        </InlineAddForm>

        <div className="space-y-2.5 pr-1">
          {isLoading ? (
            <div className="py-8 text-center text-[10px] text-m3-secondary flex flex-col items-center justify-center gap-1.5">
              <RefreshCw className="w-5 h-5 animate-spin text-m3-primary" />
              {t.loadingProfile}
            </div>
          ) : branches.length === 0 ? (
            <EmptyState icon={<MapPin className="w-6 h-6 text-m3-outline" />} message={t.noBranches} />
          ) : (
            branches.map((b) =>
              edit.isEditing(b.branchId) ? (
                /* ── Inline edit form ── */
                <div key={b.branchId} className="p-3.5 rounded-xl border border-m3-primary/40 bg-m3-surface-container/60 animate-fadeIn space-y-2.5">
                  <div className="flex justify-between items-center border-b border-m3-outline/15 pb-2">
                    <span className="text-[10px] font-semibold text-m3-primary flex items-center gap-1">
                      <Pencil className="w-3 h-3" /> {t.editBranch}
                    </span>
                    <IconButton tooltip={t.cancelEdit} onClick={edit.cancelEdit}>
                      <X className="w-3.5 h-3.5" />
                    </IconButton>
                  </div>
                  <M3TextField label={t.branchName} value={edit.draft.name ?? ''} onChange={(e) => edit.setField('name', e.target.value)} />
                  <M3TextField label={t.branchCode} value={edit.draft.code ?? ''} onChange={(e) => edit.setField('code', e.target.value)} />
                  <M3TextField label={t.geofencingMeta} value={edit.draft.geofencingMetadata ?? ''} onChange={(e) => edit.setField('geofencingMetadata', e.target.value)} placeholder="e.g. -12.1191,-77.0291;r=500" />
                  <div className="flex gap-2 pt-1">
                    <M3Button variant="filled" onClick={saveBranchEdit} className="flex-1 text-[10px] py-1.5 h-8 font-semibold flex items-center justify-center gap-1">
                      <Save className="w-3 h-3" /> {t.saveBtn}
                    </M3Button>
                    <M3Button variant="outlined" onClick={edit.cancelEdit} className="flex-1 text-[10px] py-1.5 h-8 font-semibold">
                      {t.cancelEdit}
                    </M3Button>
                  </div>
                </div>
              ) : (
                /* ── Read-only row ── */
                <EntityRow
                  key={b.branchId}
                  id={b.branchId}
                  isActive={b.isActive}
                  onDoubleClick={() => openBranchEdit(b)}
                  actions={
                    <>
                      <IconButton tooltip={t.editBtn} onClick={() => openBranchEdit(b)} className="opacity-0 group-hover/row:opacity-100">
                        <Pencil className="w-3.5 h-3.5" />
                      </IconButton>
                      {b.isActive ? (
                        <IconButton tooltip={t.deactivate} onClick={() => deactivateBranchMutation.mutate(b.branchId)} disabled={deactivateBranchMutation.isPending}>
                          <EyeOff className="w-3.5 h-3.5" />
                        </IconButton>
                      ) : (
                        <IconButton tooltip={t.reactivate} onClick={() => reactivateBranchMutation.mutate(b.branchId)} disabled={reactivateBranchMutation.isPending}>
                          <ShieldCheck className="w-3.5 h-3.5" />
                        </IconButton>
                      )}
                      <IconButton tooltip={t.removeLocation} onClick={() => removeBranchMutation.mutate(b.branchId)} disabled={removeBranchMutation.isPending} className="hover:text-m3-error hover:bg-m3-error/10">
                        <Trash2 className="w-3.5 h-3.5" />
                      </IconButton>
                    </>
                  }
                >
                  <div className="flex items-center gap-1.5 flex-wrap">
                    <span className="font-semibold text-[11px] text-m3-on-surface">{b.name}</span>
                    <CodeBadge code={b.code} size="xs" />
                    <StatusBadge
                      status={b.isActive ? 'Active' : 'Suspended'}
                      label={b.isActive ? t.active : t.suspended}
                      className="text-[8px]"
                    />
                  </div>
                  {b.geofencingMetadata && (
                    <p className="text-[9px] text-indigo-400 font-bold flex items-center gap-0.5 truncate" title={b.geofencingMetadata}>
                      <MapPin className="w-3 h-3 text-indigo-400" /> {b.geofencingMetadata}
                    </p>
                  )}
                </EntityRow>
              ),
            )
          )}
        </div>
      </div>
    </div>
  );
};

