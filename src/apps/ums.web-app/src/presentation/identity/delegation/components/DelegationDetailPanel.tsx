import React from 'react';
import { Shield, Key } from 'lucide-react';
import { Delegation } from '@domain/identity/models/delegation.model';
import { DetailPanelShell, DetailTab } from '@shared/components/DetailPanelShell';
import { DelegationProfileCard } from './DelegationProfileCard';
import { useI18n } from '@app/i18n/use-i18n';

export type DelegationConsoleTab = 'overview' | 'permissions';

const formatDate = (isoString: string | null) => {
  if (!isoString) return 'Indefinido';
  try {
    return new Intl.DateTimeFormat('es-PE', { 
      year: 'numeric', month: 'short', day: 'numeric', 
      hour: '2-digit', minute: '2-digit' 
    }).format(new Date(isoString));
  } catch {
    return isoString;
  }
};

interface DelegationDetailPanelProps {
  selectedId: string;
  activeDelegation: Delegation | undefined;
  isLoading: boolean;
  activeConsoleTab: DelegationConsoleTab;
  consoleTabs: DelegationConsoleTab[];
  onConsoleTabChange: (tab: DelegationConsoleTab) => void;
}

export const DelegationDetailPanel: React.FC<DelegationDetailPanelProps> = ({
  selectedId,
  activeDelegation,
  isLoading,
  activeConsoleTab,
  consoleTabs,
  onConsoleTabChange,
}) => {
  const t = useI18n();

  const icons: Record<DelegationConsoleTab, React.ReactNode> = {
    overview: <Shield className="w-3.5 h-3.5" />,
    permissions: <Key className="w-3.5 h-3.5" />,
  };

  const labels: Record<DelegationConsoleTab, string> = {
    overview: 'Overview',
    permissions: 'Permissions',
  };

  const tabs: DetailTab<DelegationConsoleTab>[] = consoleTabs.map((tab) => ({
    key: tab,
    label: labels[tab],
    icon: icons[tab],
  }));

  return (
    <DetailPanelShell<DelegationConsoleTab>
      isLoading={isLoading}
      isEmpty={!activeDelegation}
      loadingLabel={t.loadingProfile ?? 'Loading...'}
      emptyLabel={'Select a delegation'}
      entityKey={selectedId}
      tabs={tabs}
      activeTab={activeConsoleTab}
      onTabChange={onConsoleTabChange}
      header={
        activeDelegation ? (
          <DelegationProfileCard
            delegation={activeDelegation}
          />
        ) : undefined
      }
    >
      {activeConsoleTab === 'overview' && activeDelegation && (
        <div className="p-4 text-sm text-m3-on-surface-variant">
          <p><strong>Valid From:</strong> {formatDate(activeDelegation.validFrom)}</p>
          <p><strong>Valid Until:</strong> {formatDate(activeDelegation.validUntil)}</p>
          <p><strong>Requires Approval:</strong> {activeDelegation.requiresApproval ? 'Yes' : 'No'}</p>
        </div>
      )}

      {activeConsoleTab === 'permissions' && activeDelegation && (
        <div className="p-4">
          <h4 className="text-sm font-semibold mb-2 text-m3-on-surface">Allowed Actions</h4>
          <ul className="list-disc list-inside text-sm text-m3-on-surface-variant">
            {activeDelegation.allowedActions.map((action) => (
              <li key={action}>{action}</li>
            ))}
          </ul>
        </div>
      )}
    </DetailPanelShell>
  );
};
