import React, { useState } from 'react';
import { User, Shield, Key, LayoutGrid } from 'lucide-react';
import { UserAccount } from '@domain/identity/models/user-account.model';
import { UserAccountProfileCard } from './UserAccountProfileCard';
import { UserAccountPasswordPanel } from './UserAccountPasswordPanel';
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
          <UserAccountPasswordPanel account={activeAccount} />
        )}
      </div>
    </DetailPanelShell>
  );
};
