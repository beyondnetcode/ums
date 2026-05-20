import React, { useState } from 'react';
import {
  useGetBranches,
  useAddBranch,
  useRemoveBranch,
  useDeactivateBranch,
  useReactivateBranch
} from '../../../application/identity/hooks/use-branch';
import { M3Button } from '../../shared/components/M3Button';
import { M3TextField } from '../../shared/components/M3TextField';
import { M3Card } from '../../shared/components/M3Card';
import { RefreshCw, MapPin, EyeOff, ShieldCheck, Trash2, Plus, X } from 'lucide-react';

interface BranchManagerProps {
  tenantId: string;
}

export const BranchManager: React.FC<BranchManagerProps> = ({ tenantId }) => {
  const { data: branches = [], isLoading, refetch, isFetching } = useGetBranches(tenantId);
  const addBranchMutation = useAddBranch(tenantId);
  const removeBranchMutation = useRemoveBranch(tenantId);
  const deactivateBranchMutation = useDeactivateBranch(tenantId);
  const reactivateBranchMutation = useReactivateBranch(tenantId);

  const [isAdding, setIsAdding] = useState(false);
  const [code, setCode] = useState('');
  const [name, setName] = useState('');
  const [geofencing, setGeofencing] = useState('');
  const [errorMsg, setErrorMsg] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg('');

    if (code.length < 3) {
      setErrorMsg('Branch code must be at least 3 characters.');
      return;
      
    }
    if (name.length < 3) {
      setErrorMsg('Branch name must be at least 3 characters.');
      return;
    }

    try {
      await addBranchMutation.mutateAsync({
        code: code.toUpperCase().replace(/\s+/g, '_'),
        name,
        geofencingMetadata: geofencing || undefined,
      });

      setCode('');
      setName('');
      setGeofencing('');
      setIsAdding(false);
    } catch (error) {
      // Handled in query hook alerts
    }
  };

  return (
    <div className="space-y-4 select-none">
      {/* Header action panel */}
      <div className="flex justify-between items-center border-b border-m3-outline/10 pb-3">
        <div>
          <h3 className="text-xs font-black uppercase tracking-wider text-m3-on-surface">
            Sub-Locations
          </h3>
          <p className="text-[9px] text-m3-secondary font-bold">
            Manage physical branches and geofences.
          </p>
        </div>
        <div className="flex items-center gap-2">
          <M3Button
            variant="outlined"
            onClick={() => refetch()}
            className={`p-2 h-8 w-8 rounded-full ${isFetching ? 'animate-spin' : ''}`}
            title="Refresh Branches"
          >
            <RefreshCw className="w-3.5 h-3.5" />
          </M3Button>
        </div>
      </div>

      {/* Main Grid: dynamic layout when adding */}
      <div className="grid grid-cols-1 gap-4">
        
        {/* Accordion inline Add Branch panel */}
        {!isAdding ? (
          <M3Button
            variant="tonal"
            onClick={() => setIsAdding(true)}
            className="w-full flex items-center justify-center gap-1.5 py-2.5 text-[10px] font-black uppercase tracking-wider border border-m3-primary/10 hover:border-m3-primary/30"
          >
            <Plus className="w-4 h-4 text-m3-primary" /> Add Branch
          </M3Button>
        ) : (
          <M3Card variant="outlined" className="p-4.5 rounded-2xl bg-m3-surface-container/20 border-m3-primary/25 animate-fadeIn">
            <div className="flex justify-between items-center border-b border-m3-outline/15 pb-2 mb-3">
              <h4 className="text-[10px] font-black uppercase tracking-wider text-m3-primary flex items-center gap-1">
                <Plus className="w-3.5 h-3.5" /> New Branch child
              </h4>
              <button 
                onClick={() => {
                  setIsAdding(false);
                  setErrorMsg('');
                }} 
                className="p-1 rounded-full hover:bg-m3-primary/10 text-m3-secondary hover:text-m3-primary transition-all"
                title="Cancel addition"
              >
                <X className="w-3.5 h-3.5" />
              </button>
            </div>

            {errorMsg && (
              <div className="p-2.5 bg-m3-error/15 border border-m3-error/20 text-m3-error rounded-xl text-[9px] font-bold mb-3">
                {errorMsg}
              </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-3">
              <M3TextField
                label="Branch Code"
                required
                value={code}
                onChange={(e) => setCode(e.target.value)}
                placeholder="e.g. BR_NORTH"
              />

              <M3TextField
                label="Branch Name"
                required
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="e.g. Northern Corporate"
              />

              <M3TextField
                label="Geofencing Metadata"
                value={geofencing}
                onChange={(e) => setGeofencing(e.target.value)}
                placeholder="e.g. 19.4326,-99.1332;r=500"
              />

              <M3Button
                variant="filled"
                type="submit"
                className="w-full text-[10px] py-2 h-9 font-black uppercase tracking-wider shadow-sm"
                loading={addBranchMutation.isPending}
              >
                Save Branch
              </M3Button>
            </form>
          </M3Card>
        )}

        {/* Dynamic Branch List */}
        <div className="space-y-2.5 max-h-[300px] overflow-y-auto pr-1">
          {isLoading ? (
            <div className="py-8 text-center text-[10px] text-m3-secondary flex flex-col items-center justify-center gap-1.5">
              <RefreshCw className="w-5 h-5 animate-spin text-m3-primary" />
              Fetching branch structures...
            </div>
          ) : branches.length === 0 ? (
            <div className="py-8 text-center text-[10px] text-m3-secondary/75 flex flex-col items-center justify-center gap-1.5 border border-dashed border-m3-outline/25 rounded-2xl p-4">
              <MapPin className="w-6 h-6 text-m3-outline" />
              No sub-branches registered for this tenant.
            </div>
          ) : (
            branches.map((b) => (
              <div
                key={b.branchId}
                className={`p-3 rounded-xl border flex items-center justify-between transition-colors bg-m3-surface-container/40 ${
                  b.isActive
                    ? 'border-m3-outline/35 hover:border-m3-primary/25'
                    : 'border-m3-outline/15 opacity-65 bg-m3-outline/5'
                }`}
              >
                <div className="space-y-0.5 max-w-[80%]">
                  <div className="flex items-center gap-1.5 flex-wrap">
                    <span className="font-extrabold text-[11px] text-m3-on-surface">
                      {b.name}
                    </span>
                    <span className="text-[8px] font-black uppercase tracking-widest px-1.5 py-0.5 rounded bg-m3-outline/30 text-m3-secondary font-mono">
                      {b.code}
                    </span>
                    <span
                      className={`text-[8px] font-black uppercase tracking-widest px-1.5 py-0.5 rounded-full border ${
                        b.isActive
                          ? 'bg-emerald-500/10 border-emerald-500/20 text-emerald-500'
                          : 'bg-rose-500/10 border-rose-500/20 text-rose-500'
                      }`}
                    >
                      {b.isActive ? 'Active' : 'Off'}
                    </span>
                  </div>

                  {b.geofencingMetadata && (
                    <p className="text-[9px] text-indigo-400 font-bold flex items-center gap-0.5 truncate" title={b.geofencingMetadata}>
                      <MapPin className="w-3 h-3 text-indigo-400" /> {b.geofencingMetadata}
                    </p>
                  )}
                </div>

                {/* Branch Operations */}
                <div className="flex items-center gap-1">
                  {b.isActive ? (
                    <button
                      onClick={() => deactivateBranchMutation.mutate(b.branchId)}
                      disabled={deactivateBranchMutation.isPending}
                      className="p-1.5 rounded-full hover:bg-m3-primary/10 text-m3-secondary hover:text-m3-primary transition-all"
                      title="Deactivate Branch"
                    >
                      <EyeOff className="w-3.5 h-3.5" />
                    </button>
                  ) : (
                    <button
                      onClick={() => reactivateBranchMutation.mutate(b.branchId)}
                      disabled={reactivateBranchMutation.isPending}
                      className="p-1.5 rounded-full hover:bg-m3-primary/10 text-m3-secondary hover:text-m3-primary transition-all"
                      title="Reactivate Branch"
                    >
                      <ShieldCheck className="w-3.5 h-3.5" />
                    </button>
                  )}

                  <button
                    onClick={() => removeBranchMutation.mutate(b.branchId)}
                    disabled={removeBranchMutation.isPending}
                    className="p-1.5 rounded-full hover:bg-m3-error/15 text-m3-secondary hover:text-m3-error transition-all"
                    title="Delete Branch"
                  >
                    <Trash2 className="w-3.5 h-3.5" />
                  </button>
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
};
export default BranchManager;
