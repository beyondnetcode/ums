import React, { useState } from 'react';
import { User, Shield, Key, CheckCircle2, Fingerprint, Clock, Laptop, XCircle, LayoutGrid } from 'lucide-react';
import { UserAccount } from '@domain/identity/models/user-account.model';
import { UserAccountProfileCard } from './UserAccountProfileCard';
import { DetailPanelShell, DetailTab } from '@shared/components/DetailPanelShell';
import { useI18n } from '@app/i18n/use-i18n';

type UserAccountTab = 'overview' | 'permissions' | 'credentials';

interface UserAccountDetailPanelProps {
  activeAccount: UserAccount | undefined;
  isLoading: boolean;
  onAccountActivate: () => void;
  onAccountBlock: (userAccountId: string) => void;
  onAccountRestore: (userAccountId: string) => void;
  onAccountUpdate: (accountId: string, patch: Partial<UserAccount>) => void;
}

export const UserAccountDetailPanel: React.FC<UserAccountDetailPanelProps> = ({
  activeAccount,
  isLoading,
  onAccountActivate,
  onAccountBlock,
  onAccountRestore,
  onAccountUpdate,
}) => {
  const t = useI18n();
  const [activeTab, setActiveTab] = useState<UserAccountTab>('overview');

  const tabs: DetailTab<UserAccountTab>[] = [
    { key: 'overview', label: t.overview, icon: <User className="w-4 h-4" /> },
    { key: 'permissions', label: t.permissions, icon: <Shield className="w-4 h-4" /> },
    { key: 'credentials', label: t.credentials, icon: <Key className="w-4 h-4" /> },
  ];

  if (isLoading || !activeAccount) {
    return (
      <DetailPanelShell
        isLoading={isLoading}
        isEmpty={!activeAccount}
        tabs={[]}
        activeTab={activeTab}
        onTabChange={setActiveTab}
        header={
          <div className="flex items-center justify-center h-32">
            <p className="text-sm text-m3-secondary">{t.selectAccountToView}</p>
          </div>
        }
      >
        <div />
      </DetailPanelShell>
    );
  }

  // Determine realistic mock role and permissions based on user email/category
  const isManager = activeAccount.email.includes('gerente') || activeAccount.email.includes('admin');
  const isAnalyst = activeAccount.email.includes('analista') || activeAccount.email.includes('inventario');

  const userRoleName = isManager
    ? 'Administrador Global (SysAdmin)'
    : isAnalyst
    ? 'Analista de Operaciones WMS'
    : 'Visualizador Externo (Partner)';

  const userRoleScope = isManager
    ? 'Corporativo (Todos los inquilinos)'
    : isAnalyst
    ? 'Sucursal Callao HQ (RANSA_CALLAO_HQ)'
    : 'Acceso Restringido a Sede';

  const userSuites = isManager
    ? [
        { code: 'LOGISTICS_CORE', name: 'Logistics Core', actions: ['VIEW', 'MANAGE', 'APPROVE'] },
        { code: 'WMS', name: 'Warehouse Management', actions: ['INVENTORY_VIEW', 'INVENTORY_EDIT'] }
      ]
    : isAnalyst
    ? [
        { code: 'WMS', name: 'Warehouse Management', actions: ['INVENTORY_VIEW', 'INVENTORY_EDIT'] }
      ]
    : [
        { code: 'LOGISTICS_CORE', name: 'Logistics Core', actions: ['VIEW'] }
      ];

  // Credentials and active auth telemetry
  const isSso = activeAccount.email.includes('ransa.pe') || activeAccount.email.includes('neptunia.pe');
  const activeSsoProvider = activeAccount.email.includes('ransa.pe') ? 'Microsoft Entra ID (Azure AD)' : 'Okta Integration';

  return (
    <DetailPanelShell
      isLoading={false}
      isEmpty={false}
      tabs={tabs}
      activeTab={activeTab}
      onTabChange={setActiveTab}
      entityKey={activeAccount.userAccountId}
      header={
        <UserAccountProfileCard
          account={activeAccount}
          onActivate={() => onAccountActivate()}
          onBlock={() => onAccountBlock(activeAccount.userAccountId)}
          onRestore={() => onAccountRestore(activeAccount.userAccountId)}
          onAccountUpdate={onAccountUpdate}
        />
      }
    >
      <div className="space-y-4">
        {activeTab === 'overview' && (
          <div>
            <h3 className="text-sm font-medium text-m3-on-surface mb-2">{t.accountDetails}</h3>
            <dl className="space-y-2 text-sm">
              <div className="flex justify-between">
                <dt className="text-m3-secondary">{t.userEmail}</dt>
                <dd className="text-m3-on-surface font-mono">{activeAccount.email}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-m3-secondary">{t.userCategory}</dt>
                <dd className="text-m3-on-surface">{activeAccount.category}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-m3-secondary">{t.status}</dt>
                <dd className="text-m3-on-surface">{activeAccount.status}</dd>
              </div>
            </dl>
          </div>
        )}

        {activeTab === 'permissions' && (
          <div className="space-y-4 animate-fadeIn">
            {/* Active Security Profile Card */}
            <div className="p-3.5 rounded-xl border border-m3-outline/20 bg-m3-surface-container/5 space-y-3">
              <div className="flex justify-between items-center border-b border-m3-outline/10 pb-2">
                <div className="flex items-center gap-2">
                  <Shield className="w-4 h-4 text-m3-primary" />
                  <span className="text-xs font-semibold text-m3-on-surface">Perfil de Seguridad Activo</span>
                </div>
                <span className="text-[10px] uppercase font-bold tracking-wider px-2 py-0.5 rounded-full bg-emerald-500/10 text-emerald-500 border border-emerald-500/20">
                  Asignado
                </span>
              </div>
              
              <div className="space-y-2 text-xs">
                <div className="flex justify-between">
                  <span className="text-m3-secondary">Rol Funcional</span>
                  <span className="font-semibold text-m3-on-surface">{userRoleName}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-m3-secondary">Alcance de Autorización</span>
                  <span className="font-mono text-m3-on-surface">{userRoleScope}</span>
                </div>
              </div>
            </div>

            {/* Mapped Permission Suite Graph */}
            <div className="space-y-2">
              <span className="text-[10px] font-bold text-m3-secondary uppercase tracking-wider block">Permisos en Suites del Sistema</span>
              
              <div className="space-y-2.5">
                {userSuites.map((suite) => (
                  <div key={suite.code} className="p-3 rounded-lg border border-m3-outline/15 bg-m3-surface-container/5 space-y-2 hover:border-m3-outline/30 transition-colors">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-1.5">
                        <LayoutGrid className="w-3.5 h-3.5 text-m3-secondary" />
                        <span className="text-xs font-semibold text-m3-on-surface">{suite.name}</span>
                        <span className="text-[9px] font-mono px-1 rounded bg-m3-surface-variant text-m3-on-surface-variant">{suite.code}</span>
                      </div>
                      <span className="text-[9px] text-m3-secondary/50">Herencia Directa</span>
                    </div>

                    <div className="flex flex-wrap gap-1.5 pt-1">
                      {suite.actions.map((act) => (
                        <span key={act} className="flex items-center gap-1 text-[9px] font-mono font-semibold px-2 py-0.5 rounded bg-m3-primary/10 text-m3-primary border border-m3-primary/20">
                          <Key className="w-2.5 h-2.5" />
                          {act}
                        </span>
                      ))}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        )}

        {activeTab === 'credentials' && (
          <div className="space-y-4 animate-fadeIn">
            {/* Identity & SSO Status Card */}
            <div className="p-3.5 rounded-xl border border-m3-outline/20 bg-m3-surface-container/5 space-y-3">
              <div className="flex justify-between items-center border-b border-m3-outline/10 pb-2">
                <div className="flex items-center gap-2">
                  <Fingerprint className="w-4 h-4 text-m3-primary" />
                  <span className="text-xs font-semibold text-m3-on-surface">Métodos de Autenticación</span>
                </div>
                <span className="text-[10px] font-mono px-1.5 py-0.5 rounded bg-m3-surface-variant text-m3-on-surface-variant border border-m3-outline/10">Secure</span>
              </div>

              <div className="space-y-2 text-xs">
                {isSso ? (
                  <div className="flex items-center justify-between">
                    <div className="flex flex-col">
                      <span className="font-semibold text-m3-on-surface">Federación Single Sign-On (SSO)</span>
                      <span className="text-[10px] text-m3-secondary">{activeSsoProvider}</span>
                    </div>
                    <span className="flex items-center gap-1 text-[10px] text-emerald-500 font-bold px-2 py-0.5 rounded bg-emerald-500/10 border border-emerald-500/20">
                      <CheckCircle2 className="w-3 h-3" /> Habilitado
                    </span>
                  </div>
                ) : (
                  <div className="flex items-center justify-between">
                    <div className="flex flex-col">
                      <span className="font-semibold text-m3-on-surface">Contraseña Encriptada</span>
                      <span className="text-[10px] text-m3-secondary">Algoritmo Bcrypt (Costo: 12)</span>
                    </div>
                    <span className="flex items-center gap-1 text-[10px] text-emerald-500 font-bold px-2 py-0.5 rounded bg-emerald-500/10 border border-emerald-500/20">
                      <CheckCircle2 className="w-3 h-3" /> Activo
                    </span>
                  </div>
                )}

                <div className="border-t border-m3-outline/10 pt-2 flex items-center justify-between">
                  <div className="flex flex-col">
                    <span className="font-semibold text-m3-on-surface">Doble Factor (MFA TOTP)</span>
                    <span className="text-[10px] text-m3-secondary">Authenticator App / Hardware Key</span>
                  </div>
                  <span className="flex items-center gap-1 text-[10px] text-emerald-500 font-bold px-2 py-0.5 rounded bg-emerald-500/10 border border-emerald-500/20">
                    <CheckCircle2 className="w-3 h-3" /> Verificado
                  </span>
                </div>
              </div>
            </div>

            {/* Authentication History Timelines */}
            <div className="space-y-2">
              <span className="text-[10px] font-bold text-m3-secondary uppercase tracking-wider block">Historial de Accesos Recientes</span>
              
              <div className="space-y-2 text-[11px]">
                {/* Attempt 1 */}
                <div className="flex items-center justify-between p-2 rounded bg-m3-surface-container/5 border border-m3-outline/5">
                  <div className="flex items-center gap-2">
                    <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500 flex-shrink-0" />
                    <div>
                      <span className="font-semibold text-m3-on-surface">Autenticación Exitosa</span>
                      <div className="flex items-center gap-2 text-[10px] text-m3-secondary/70">
                        <span className="flex items-center gap-0.5"><Laptop className="w-2.5 h-2.5" /> Chrome/macOS</span>
                        <span>·</span>
                        <span>IP: 192.168.1.15</span>
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-1 text-[10px] text-m3-secondary">
                    <Clock className="w-3 h-3" /> 5 min ago
                  </div>
                </div>

                {/* Attempt 2 */}
                <div className="flex items-center justify-between p-2 rounded bg-m3-surface-container/5 border border-m3-outline/5">
                  <div className="flex items-center gap-2">
                    <CheckCircle2 className="w-3.5 h-3.5 text-emerald-500 flex-shrink-0" />
                    <div>
                      <span className="font-semibold text-m3-on-surface">Autenticación Exitosa</span>
                      <div className="flex items-center gap-2 text-[10px] text-m3-secondary/70">
                        <span className="flex items-center gap-0.5"><Laptop className="w-2.5 h-2.5" /> Chrome/macOS</span>
                        <span>·</span>
                        <span>IP: 192.168.1.15</span>
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-1 text-[10px] text-m3-secondary">
                    <Clock className="w-3 h-3" /> 3 hours ago
                  </div>
                </div>

                {/* Attempt 3 */}
                <div className="flex items-center justify-between p-2 rounded bg-m3-surface-container/5 border border-m3-outline/5">
                  <div className="flex items-center gap-2">
                    <XCircle className="w-3.5 h-3.5 text-m3-error flex-shrink-0" />
                    <div>
                      <span className="font-semibold text-m3-on-surface">Fallo de Credenciales (Bcrypt)</span>
                      <div className="flex items-center gap-2 text-[10px] text-m3-secondary/70">
                        <span className="flex items-center gap-0.5"><Laptop className="w-2.5 h-2.5" /> Firefox/Linux</span>
                        <span>·</span>
                        <span>IP: 200.48.55.12 (Lima, PE)</span>
                      </div>
                    </div>
                  </div>
                  <div className="flex items-center gap-1 text-[10px] text-m3-secondary">
                    <Clock className="w-3 h-3" /> 1 day ago
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </DetailPanelShell>
  );
};
