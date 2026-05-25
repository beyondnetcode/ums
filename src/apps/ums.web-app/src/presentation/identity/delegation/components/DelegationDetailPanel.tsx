import React from 'react';
import { Shield, Key } from 'lucide-react';
import { Delegation } from '@domain/identity/models/delegation.model';
import { DetailPanelShell, DetailTab } from '@shared/components/DetailPanelShell';
import { DelegationProfileCard } from './DelegationProfileCard';
import { useI18n } from '@app/i18n/use-i18n';
import { KeyValueRow } from '@shared/components/KeyValueRow';
import { CodeBadge } from '@shared/components/CodeBadge';
import { Calendar, AlertCircle } from 'lucide-react';

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
    overview: t.overview ?? 'Resumen',
    permissions: t.permissions ?? 'Permisos',
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
        <div className="p-5 flex flex-col gap-3">
          <KeyValueRow
            icon={<Calendar className="w-3.5 h-3.5" />}
            label={t.validFrom ?? 'Válido desde'}
            value={formatDate(activeDelegation.validFrom)}
          />
          <KeyValueRow
            icon={<Calendar className="w-3.5 h-3.5" />}
            label={t.validUntil ?? 'Válido hasta'}
            value={formatDate(activeDelegation.validUntil)}
          />
          <KeyValueRow
            icon={<AlertCircle className="w-3.5 h-3.5" />}
            label={t.requiresApproval ?? 'Requiere Aprobación'}
            value={activeDelegation.requiresApproval ? (t.yes ?? 'Sí') : (t.no ?? 'No')}
            bordered={false}
          />
        </div>
      )}

      {activeConsoleTab === 'permissions' && activeDelegation && (
        <div className="p-5">
          <h4 className="text-xs font-bold uppercase tracking-wider text-m3-secondary mb-3">
            {t.allowedActions ?? 'Acciones Permitidas'}
          </h4>
          <div className="flex flex-wrap gap-2">
            {activeDelegation.allowedActions.map((action) => (
              <CodeBadge key={action} code={action} />
            ))}
          </div>
        </div>
      )}
    </DetailPanelShell>
  );
};
