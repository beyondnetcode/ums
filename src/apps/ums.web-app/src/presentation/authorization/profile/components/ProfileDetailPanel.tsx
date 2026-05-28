import React, { useState } from 'react';
import { Shield, ShieldAlert, ShieldCheck, ToggleLeft, ToggleRight, User, Key, Globe, Info, Loader2 } from 'lucide-react';
import { type Profile } from '@domain/authorization/schemas/profile.schema';
import { M3SegmentedButton } from '@shared/components/M3SegmentedButton';
import { M3Button } from '@shared/components/M3Button';
import { DetailPanelShell } from '@shared/components';
import { useOverrideProfilePermission, useActivateProfilePermission, useDeactivateProfilePermission, useActivateProfile, useDeactivateProfile } from '@app/authorization/hooks/use-profile';

interface Props {
  profile: Profile | null;
  isLoading: boolean;
}

type DetailTab = 'overview' | 'permissions';

export const ProfileDetailPanel: React.FC<Props> = ({ profile, isLoading }) => {
  const [activeTab, setActiveTab] = useState<DetailTab>('overview');

  const overrideMutation = useOverrideProfilePermission();
  const activatePermMutation = useActivateProfilePermission();
  const deactivatePermMutation = useDeactivateProfilePermission();
  const activateProfileMutation = useActivateProfile();
  const deactivateProfileMutation = useDeactivateProfile();

  if (isLoading) {
    return (
      <div className="h-full flex items-center justify-center">
        <Loader2 className="w-8 h-8 animate-spin text-m3-primary" />
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="h-full flex flex-col items-center justify-center p-6 text-center text-m3-on-surface/40">
        <User className="w-12 h-12 mb-3 stroke-[1.5] text-m3-primary/30" />
        <h3 className="text-sm font-bold text-m3-on-surface/70">Ningún perfil seleccionado</h3>
        <p className="text-xs max-w-xs mt-1">Seleccione un perfil del panel izquierdo para visualizar su configuración y grafo de autorización.</p>
      </div>
    );
  }

  const handleToggleProfileStatus = async () => {
    if (profile.isActive) {
      await deactivateProfileMutation.mutateAsync(profile.profileId);
    } else {
      await activateProfileMutation.mutateAsync(profile.profileId);
    }
  };

  const handleTogglePermissionActive = async (permissionId: string, currentActive: boolean) => {
    if (currentActive) {
      await deactivatePermMutation.mutateAsync({ profileId: profile.profileId, permissionId });
    } else {
      await activatePermMutation.mutateAsync({ profileId: profile.profileId, permissionId });
    }
  };

  const handleChangePermissionEffect = async (permissionId: string, newEffect: 'allow' | 'deny' | 'neutral') => {
    await overrideMutation.mutateAsync({ profileId: profile.profileId, permissionId, effect: newEffect });
  };

  return (
    <DetailPanelShell
      title={`Perfil Efectivo`}
      subtitle={`Usuario: ${profile.userId.slice(0, 8)}... · ${profile.scope}`}
      headerActions={
        <M3Button
          type="button"
          variant={profile.isActive ? 'tonal' : 'filled'}
          onClick={handleToggleProfileStatus}
          disabled={activateProfileMutation.isPending || deactivateProfileMutation.isPending}
        >
          {profile.isActive ? 'Desactivar Perfil' : 'Activar Perfil'}
        </M3Button>
      }
    >
      <div className="h-full flex flex-col">
        {/* Navigation Tabs using Segmented Buttons */}
        <div className="flex justify-center my-3 px-4">
          <M3SegmentedButton
            segments={[
              { id: 'overview', label: 'Resumen' },
              { id: 'permissions', label: 'Permisos Efectivos' },
            ]}
            activeId={activeTab}
            onChange={(id) => setActiveTab(id as DetailTab)}
          />
        </div>

        <div className="flex-1 overflow-y-auto px-4 pb-6">
          {activeTab === 'overview' && (
            <div className="space-y-4">
              {/* Profile Overview Card */}
              <div className="rounded-xl border border-m3-outline/10 bg-m3-surface-container/20 p-4 space-y-3">
                <div className="flex items-center gap-3">
                  <div className="p-2 rounded-lg bg-m3-primary/10 text-m3-primary">
                    <User className="w-5 h-5" />
                  </div>
                  <div>
                    <span className="text-[10px] uppercase font-bold text-m3-secondary/50">ID del Usuario</span>
                    <p className="text-sm font-semibold text-m3-on-surface font-mono">{profile.userId}</p>
                  </div>
                </div>

                <div className="flex items-center gap-3">
                  <div className="p-2 rounded-lg bg-m3-primary/10 text-m3-primary">
                    <Key className="w-5 h-5" />
                  </div>
                  <div>
                    <span className="text-[10px] uppercase font-bold text-m3-secondary/50">ID del Rol Asociado</span>
                    <p className="text-sm font-semibold text-m3-on-surface font-mono">{profile.roleId}</p>
                  </div>
                </div>

                <div className="flex items-center gap-3">
                  <div className="p-2 rounded-lg bg-m3-primary/10 text-m3-primary">
                    <Globe className="w-5 h-5" />
                  </div>
                  <div>
                    <span className="text-[10px] uppercase font-bold text-m3-secondary/50">Inquilino (Tenant ID)</span>
                    <p className="text-sm font-semibold text-m3-on-surface font-mono">{profile.tenantId}</p>
                  </div>
                </div>

                <div className="flex items-center gap-3">
                  <div className="p-2 rounded-lg bg-m3-primary/10 text-m3-primary">
                    <Info className="w-5 h-5" />
                  </div>
                  <div>
                    <span className="text-[10px] uppercase font-bold text-m3-secondary/50">Estado / Alcance</span>
                    <p className="text-sm font-semibold text-m3-on-surface">
                      {profile.isActive ? 'Activo' : 'Inactivo'} · <span className="font-medium text-m3-primary">{profile.scope}</span>
                    </p>
                  </div>
                </div>
              </div>

              <div className="rounded-xl border border-amber-500/10 bg-amber-500/5 p-4 text-xs text-amber-500/90 leading-relaxed">
                <span className="font-bold block mb-1">Nota del Auditor</span>
                Los permisos listados en la siguiente pestaña representan el conjunto materializado definitivo de privilegios del usuario, fusionando las reglas de la plantilla por defecto con cualquier anulación manual (Override) grabada en caliente.
              </div>
            </div>
          )}

          {activeTab === 'permissions' && (
            <div className="space-y-3">
              {profile.permissions.length === 0 ? (
                <div className="text-center py-10 text-xs text-m3-on-surface/50">
                  Ningún permiso materializado. Asocie una plantilla primero.
                </div>
              ) : (
                <div className="rounded-xl border border-m3-outline/10 bg-m3-surface-container/10 overflow-hidden divide-y divide-m3-outline/5">
                  {profile.permissions.map((p) => (
                    <div key={p.permissionId} className="flex flex-col md:flex-row md:items-center justify-between gap-3 p-3 text-xs hover:bg-m3-surface-container/20 transition-colors">
                      <div className="space-y-0.5">
                        <div className="flex items-center gap-1.5 flex-wrap">
                          <span className="font-bold text-m3-on-surface">{p.targetName}</span>
                          <span className="text-[9px] uppercase font-bold text-m3-primary bg-m3-primary/10 px-1.5 py-0.2 rounded border border-m3-primary/20">
                            {p.targetType}
                          </span>
                          {p.isOverride && (
                            <span className="text-[8px] uppercase font-bold text-amber-500 bg-amber-500/10 px-1.5 py-0.2 rounded border border-amber-500/20">
                              Override
                            </span>
                          )}
                        </div>
                        <div className="text-[10px] text-m3-on-surface/60">
                          Acción: <span className="font-medium text-m3-secondary">{p.actionName}</span>
                        </div>
                      </div>

                      <div className="flex items-center gap-4 self-end md:self-auto">
                        {/* Interactive Allow/Deny Selector in DB */}
                        <div className="flex items-center bg-m3-surface-container/60 rounded-lg p-0.5 border border-m3-outline/20">
                          <button
                            type="button"
                            onClick={() => handleChangePermissionEffect(p.permissionId, 'allow')}
                            disabled={overrideMutation.isPending}
                            className={`px-2 py-1 rounded-md text-[9px] font-bold transition-all ${p.isAllowed ? 'bg-emerald-500 text-white' : 'text-m3-on-surface/60 hover:text-m3-on-surface'}`}
                          >
                            <ShieldCheck className="w-3 h-3 inline mr-0.5" />
                            Allow
                          </button>
                          <button
                            type="button"
                            onClick={() => handleChangePermissionEffect(p.permissionId, 'neutral')}
                            disabled={overrideMutation.isPending}
                            className={`px-2 py-1 rounded-md text-[9px] font-bold transition-all ${!p.isAllowed && !p.isDenied ? 'bg-m3-outline/30 text-m3-on-surface' : 'text-m3-on-surface/60 hover:text-m3-on-surface'}`}
                          >
                            <Shield className="w-3 h-3 inline mr-0.5" />
                            Neutral
                          </button>
                          <button
                            type="button"
                            onClick={() => handleChangePermissionEffect(p.permissionId, 'deny')}
                            disabled={overrideMutation.isPending}
                            className={`px-2 py-1 rounded-md text-[9px] font-bold transition-all ${p.isDenied ? 'bg-rose-500 text-white' : 'text-m3-on-surface/60 hover:text-m3-on-surface'}`}
                          >
                            <ShieldAlert className="w-3 h-3 inline mr-0.5" />
                            Deny
                          </button>
                        </div>

                        {/* Interactive Toggle Switch in DB */}
                        <div className="flex items-center gap-1.5">
                          <button
                            type="button"
                            onClick={() => handleTogglePermissionActive(p.permissionId, p.isActive)}
                            disabled={activatePermMutation.isPending || deactivatePermMutation.isPending}
                            className="text-m3-primary hover:opacity-85 transition-opacity focus:outline-none"
                          >
                            {p.isActive ? (
                              <ToggleRight className="w-6 h-6 text-emerald-500 fill-emerald-500/20" />
                            ) : (
                              <ToggleLeft className="w-6 h-6 text-m3-outline" />
                            )}
                          </button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </DetailPanelShell>
  );
};
