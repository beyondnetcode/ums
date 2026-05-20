import React, { useState } from 'react';
import {
  useGetBranches,
  useAddBranch,
  useRemoveBranch,
  useDeactivateBranch,
  useReactivateBranch
} from '../../../application/identity/hooks/use-branch';
import { useI18n } from '../../../application/i18n/use-i18n';
import { useNotificationStore } from '../../../application/stores/notification.store';
import { M3Button } from '../../shared/components/M3Button';
import { M3TextField } from '../../shared/components/M3TextField';
import { M3Card } from '../../shared/components/M3Card';
import { IconButton } from '../../shared/components/Tooltip';
import { RefreshCw, MapPin, EyeOff, ShieldCheck, Trash2, Plus, X, Pencil, Save } from 'lucide-react';
import { sanitizeInput, sanitizeCode } from '../../../application/utils/security';

interface BranchManagerProps {
  tenantId: string;
}

export const BranchManager: React.FC<BranchManagerProps> = ({ tenantId }) => {
  const { data: branches = [], isLoading, refetch, isFetching } = useGetBranches(tenantId);
  const addBranchMutation = useAddBranch(tenantId);
  const removeBranchMutation = useRemoveBranch(tenantId);
  const deactivateBranchMutation = useDeactivateBranch(tenantId);
  const reactivateBranchMutation = useReactivateBranch(tenantId);
  const t = useI18n();
  const addNotification = useNotificationStore((s) => s.addNotification);

  const [isAdding, setIsAdding] = useState(false);
  const [code, setCode] = useState('');
  const [name, setName] = useState('');
  const [geofencing, setGeofencing] = useState('');
  const [errorMsg, setErrorMsg] = useState('');

  const [editingBranchId, setEditingBranchId] = useState<string | null>(null);
  const [editBranchName, setEditBranchName] = useState('');
  const [editBranchCode, setEditBranchCode] = useState('');
  const [editBranchGeo, setEditBranchGeo] = useState('');

  const openBranchEdit = (b: { branchId: string; name: string; code: string; geofencingMetadata?: string }) => {
    setEditingBranchId(b.branchId);
    setEditBranchName(b.name);
    setEditBranchCode(b.code);
    setEditBranchGeo(b.geofencingMetadata ?? '');
  };

  const cancelBranchEdit = () => setEditingBranchId(null);

  const saveBranchEdit = () => {
    setEditingBranchId(null);
    addNotification({
      title: t.notifBranchUpdated,
      message: t.notifBranchUpdatedMsg(editBranchName),
      type: 'info',
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg('');

    const sanitizedCode = sanitizeCode(code);
    const sanitizedName = sanitizeInput(name);
    const sanitizedGeo = geofencing ? sanitizeInput(geofencing) : undefined;

    if (sanitizedCode.length < 3) {
      setErrorMsg(t.branchCodeHelper);
      return;
    }
    if (sanitizedName.length < 3) {
      setErrorMsg(t.branchNameHelper);
      return;
    }

    try {
      await addBranchMutation.mutateAsync({
        code: sanitizedCode,
        name: sanitizedName,
        geofencingMetadata: sanitizedGeo,
      });

      setCode('');
      setName('');
      setGeofencing('');
      setIsAdding(false);
    } catch {
      setErrorMsg(t.branchCreateFailed);
    }
  };

  return (
    <div className="space-y-4 select-none">
      <div className="flex justify-between items-center border-b border-m3-outline/10 pb-3">
        <div>
          <h3 className="text-xs font-semibold text-m3-on-surface">
            {t.subLocations}
          </h3>
          <p className="text-[9px] text-m3-secondary font-bold">
            {t.subLocationsSubtitle}
          </p>
        </div>
        <div className="flex items-center gap-2">
          <IconButton tooltip={t.refreshBranches} onClick={() => refetch()} className={isFetching ? 'animate-spin' : ''}>
            <RefreshCw className="w-3.5 h-3.5" />
          </IconButton>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4">
        {!isAdding ? (
          <M3Button
            variant="tonal"
            onClick={() => setIsAdding(true)}
            className="w-full flex items-center justify-center gap-1.5 py-2.5 text-[10px] font-semibold border border-m3-primary/10 hover:border-m3-primary/30"
          >
            <Plus className="w-4 h-4 text-m3-primary" /> {t.addLocation}
          </M3Button>
        ) : (
          <M3Card variant="outlined" className="p-4.5 rounded-2xl bg-m3-surface-container/20 border-m3-primary/25 animate-fadeIn">
            <div className="flex justify-between items-center border-b border-m3-outline/15 pb-2 mb-3">
              <h4 className="text-[10px] font-semibold text-m3-primary flex items-center gap-1">
                <Plus className="w-3.5 h-3.5" /> {t.addingLocation}
              </h4>
              <IconButton tooltip={t.cancelEdit} onClick={() => { setIsAdding(false); setErrorMsg(''); }}>
                <X className="w-3.5 h-3.5" />
              </IconButton>
            </div>

            {errorMsg && (
              <div className="p-2.5 bg-m3-error/15 border border-m3-error/20 text-m3-error rounded-xl text-[9px] font-bold mb-3">
                {errorMsg}
              </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-3">
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

              <M3Button
                variant="filled"
                type="submit"
                className="w-full text-[10px] py-2 h-9 font-semibold shadow-sm"
                loading={addBranchMutation.isPending}
              >
                {t.saveLocation}
              </M3Button>
            </form>
          </M3Card>
        )}

        <div className="space-y-2.5 pr-1">
          {isLoading ? (
            <div className="py-8 text-center text-[10px] text-m3-secondary flex flex-col items-center justify-center gap-1.5">
              <RefreshCw className="w-5 h-5 animate-spin text-m3-primary" />
              {t.loadingProfile}
            </div>
          ) : branches.length === 0 ? (
            <div className="py-8 text-center text-[10px] text-m3-secondary/75 flex flex-col items-center justify-center gap-1.5 border border-dashed border-m3-outline/25 rounded-2xl p-4">
              <MapPin className="w-6 h-6 text-m3-outline" />
              {t.noBranches}
            </div>
          ) : (
            branches.map((b) => (
              editingBranchId === b.branchId ? (
                <div key={b.branchId} className="p-3.5 rounded-xl border border-m3-primary/40 bg-m3-surface-container/60 animate-fadeIn space-y-2.5">
                  <div className="flex justify-between items-center border-b border-m3-outline/15 pb-2">
                    <span className="text-[10px] font-semibold text-m3-primary flex items-center gap-1">
                      <Pencil className="w-3 h-3" /> {t.editBranch}
                    </span>
                    <IconButton tooltip={t.cancelEdit} onClick={cancelBranchEdit}>
                      <X className="w-3.5 h-3.5" />
                    </IconButton>
                  </div>
                  <M3TextField
                    label={t.branchName}
                    value={editBranchName}
                    onChange={(e) => setEditBranchName(e.target.value)}
                  />
                  <M3TextField
                    label={t.branchCode}
                    value={editBranchCode}
                    onChange={(e) => setEditBranchCode(e.target.value)}
                  />
                  <M3TextField
                    label={t.geofencingMeta}
                    value={editBranchGeo}
                    onChange={(e) => setEditBranchGeo(e.target.value)}
                    placeholder="e.g. -12.1191,-77.0291;r=500"
                  />
                  <div className="flex gap-2 pt-1">
                    <M3Button variant="filled" onClick={saveBranchEdit} className="flex-1 text-[10px] py-1.5 h-8 font-semibold flex items-center justify-center gap-1">
                      <Save className="w-3 h-3" /> {t.saveBtn}
                    </M3Button>
                    <M3Button variant="outlined" onClick={cancelBranchEdit} className="flex-1 text-[10px] py-1.5 h-8 font-semibold">
                      {t.cancelEdit}
                    </M3Button>
                  </div>
                </div>
              ) : (
                <div
                  key={b.branchId}
                  onDoubleClick={() => openBranchEdit(b)}
                  className={`group/branch p-3 rounded-xl border flex items-center justify-between transition-colors bg-m3-surface-container/40 cursor-default ${
                    b.isActive
                      ? 'border-m3-outline/35 hover:border-m3-primary/25'
                      : 'border-m3-outline/15 opacity-65 bg-m3-outline/5'
                  }`}
                >
                  <div className="space-y-0.5 max-w-[75%]">
                    <div className="flex items-center gap-1.5 flex-wrap">
                      <span className="font-semibold text-[11px] text-m3-on-surface">{b.name}</span>
                      <span className="text-[8px] font-medium px-1.5 py-0.5 rounded bg-m3-outline/30 text-m3-secondary font-mono">
                        {b.code}
                      </span>
                      <span className={`text-[8px] font-medium px-1.5 py-0.5 rounded-full border ${
                        b.isActive
                          ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-500'
                          : 'bg-rose-500/10 border-rose-500/20 text-rose-500'
                      }`}>
                        {b.isActive ? t.active : t.suspended}
                      </span>
                    </div>

                    {b.geofencingMetadata && (
                      <p className="text-[9px] text-indigo-400 font-bold flex items-center gap-0.5 truncate" title={b.geofencingMetadata}>
                        <MapPin className="w-3 h-3 text-indigo-400" /> {b.geofencingMetadata}
                      </p>
                    )}
                  </div>

                  <div className="flex items-center gap-0.5">
                    <IconButton tooltip={t.editBtn} onClick={() => openBranchEdit(b)} className="opacity-0 group-hover/branch:opacity-100">
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
                  </div>
                </div>
              )
            ))
          )}
        </div>
      </div>
    </div>
  );
};
export default BranchManager;
