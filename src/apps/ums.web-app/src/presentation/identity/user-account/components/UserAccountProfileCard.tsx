import React from 'react';
import { User, Mail, Shield, Hash, ToggleRight, Lock, Unlock } from 'lucide-react';
import { UserAccount } from '@domain/identity/models/user-account.model';
import { M3Card } from '@shared/components/M3Card';
import { StatusBadge } from '@shared/components/StatusBadge';
import { CodeBadge } from '@shared/components/CodeBadge';
import { KeyValueRow } from '@shared/components/KeyValueRow';
import { SectionHeader } from '@shared/components/SectionHeader';
import { M3Button } from '@shared/components/M3Button';
import { useI18n } from '@app/i18n/use-i18n';
import { useStatusLabel } from '@app/hooks/use-status-label';

interface UserAccountProfileCardProps {
  account: UserAccount;
  onActivate: () => void;
  onBlock: () => void;
  onRestore: () => void;
}

export const UserAccountProfileCard: React.FC<UserAccountProfileCardProps> = ({
  account,
  onActivate,
  onBlock,
  onRestore,
}) => {
  const t = useI18n();
  const getStatusLabel = useStatusLabel();

  const renderActions = () => {
    switch (account.status) {
      case 'Pending':
        return (
          <M3Button variant="filled" onClick={onActivate} className="text-[10px] h-7">
            <ToggleRight className="w-3.5 h-3.5 mr-1" /> {t.activateBtn}
          </M3Button>
        );
      case 'Active':
        return (
          <M3Button variant="outlined" onClick={onBlock} className="text-[10px] h-7 text-m3-error border-m3-error/30">
            <Lock className="w-3.5 h-3.5 mr-1" /> {t.blockBtn}
          </M3Button>
        );
      case 'Blocked':
        return (
          <M3Button variant="filled" onClick={onRestore} className="text-[10px] h-7">
            <Unlock className="w-3.5 h-3.5 mr-1" /> {t.restoreBtn}
          </M3Button>
        );
    }
  };

  return (
    <M3Card variant="elevated" className="p-5 border border-m3-outline/25 bg-m3-surface-container/20 space-y-4">
      <SectionHeader title={account.email} subtitle={account.userAccountId.substring(0, 8)} actions={renderActions()} />

      <div className="space-y-3 text-xs">
        <KeyValueRow
          icon={<Mail className="w-3.5 h-3.5" />}
          label={t.userEmail}
          value={
            <span className="font-mono text-m3-on-surface text-xs">{account.email}</span>
          }
        />
        <KeyValueRow
          icon={<Shield className="w-3.5 h-3.5" />}
          label={t.userCategory}
          value={<CodeBadge label={account.category} />}
        />
        <KeyValueRow
          icon={<User className="w-3.5 h-3.5" />}
          label={t.status}
          value={<StatusBadge status={account.status} label={getStatusLabel(account.status)} />}
        />
        {account.identityReference && (
          <KeyValueRow
            icon={<Hash className="w-3.5 h-3.5" />}
            label={t.identityReference}
            value={
              <span className="font-mono text-m3-primary text-xs bg-m3-surface-container px-2 py-0.5 rounded border border-m3-outline/30">
                {account.identityReference}
              </span>
            }
          />
        )}
        {account.identityReferenceType && (
          <KeyValueRow
            icon={<Hash className="w-3.5 h-3.5" />}
            label={t.identityReferenceType}
            value={<CodeBadge label={account.identityReferenceType} />}
            bordered={false}
          />
        )}
      </div>
    </M3Card>
  );
};
