import React, { useState } from 'react';
import { User, Shield, Key } from 'lucide-react';
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
}

export const UserAccountDetailPanel: React.FC<UserAccountDetailPanelProps> = ({
  activeAccount,
  isLoading,
  onAccountActivate,
  onAccountBlock,
  onAccountRestore,
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

  return (
    <DetailPanelShell
      isLoading={false}
      isEmpty={false}
      tabs={tabs}
      activeTab={activeTab}
      onTabChange={setActiveTab}
      header={
        <UserAccountProfileCard
          account={activeAccount}
          onActivate={() => onAccountActivate()}
          onBlock={() => onAccountBlock(activeAccount.userAccountId)}
          onRestore={() => onAccountRestore(activeAccount.userAccountId)}
        />
      }
    >
      <div className="space-y-4">
        {activeTab === 'overview' && (
          <div>
            <h3 className="text-sm font-medium text-m3-on-surface mb-2">{t.accountDetails}</h3>
            <dl className="space-y-2 text-sm">
              <div className="flex justify-between">
                <dt className="text-m3-secondary">{t.email}</dt>
                <dd className="text-m3-on-surface">{activeAccount.email}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-m3-secondary">{t.category}</dt>
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
          <p className="text-sm text-m3-secondary">{t.noPermissionsAssigned}</p>
        )}
        {activeTab === 'credentials' && (
          <p className="text-sm text-m3-secondary">{t.noCredentialsConfigured}</p>
        )}
      </div>
    </DetailPanelShell>
  );
};
