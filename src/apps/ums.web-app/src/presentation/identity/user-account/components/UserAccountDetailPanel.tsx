import React from 'react';
import { UserAccount } from '@domain/identity/models/user-account.model';
import { UserAccountProfileCard } from './UserAccountProfileCard';
import { DetailPanelShell } from '@shared/components/DetailPanelShell';
import { useI18n } from '@app/i18n/use-i18n';

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

  if (isLoading || !activeAccount) {
    return (
      <DetailPanelShell
        header={
          <div className="flex items-center justify-center h-32">
            <p className="text-sm text-m3-secondary">{t.selectAccountToView}</p>
          </div>
        }
      />
    );
  }

  return (
    <DetailPanelShell
      header={
        <UserAccountProfileCard
          account={activeAccount}
          onActivate={() => onAccountActivate()}
          onBlock={() => onAccountBlock(activeAccount.userAccountId)}
          onRestore={() => onAccountRestore(activeAccount.userAccountId)}
        />
      }
    />
  );
};
