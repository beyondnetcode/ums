import React, { useState } from 'react';
import { Box, Shield, Key } from 'lucide-react';
import { SystemSuite } from '@domain/authorization/models/system-suite.model';
import { SystemSuiteProfileCard } from './SystemSuiteProfileCard';
import { DetailPanelShell, DetailTab } from '@shared/components/DetailPanelShell';
import { useI18n } from '@app/i18n/use-i18n';

type SystemSuiteTab = 'overview' | 'modules' | 'actions';

interface SystemSuiteDetailPanelProps {
  activeSystemSuite: SystemSuite | undefined;
  isLoading: boolean;
  onSystemSuiteUpdate: (systemSuiteId: string, patch: Partial<SystemSuite>) => void;
  onEditingChange: (isEditing: boolean) => void;
}

export const SystemSuiteDetailPanel: React.FC<SystemSuiteDetailPanelProps> = ({
  activeSystemSuite,
  isLoading,
  onSystemSuiteUpdate,
  onEditingChange,
}) => {
  const t = useI18n();
  const [activeTab, setActiveTab] = useState<SystemSuiteTab>('overview');

  const tabs: DetailTab<SystemSuiteTab>[] = [
    { key: 'overview', label: t.overview, icon: <Box className="w-4 h-4" /> },
    { key: 'modules', label: t.modules, icon: <Shield className="w-4 h-4" /> },
    { key: 'actions', label: t.actions, icon: <Key className="w-4 h-4" /> },
  ];

  if (isLoading || !activeSystemSuite) {
    return (
      <DetailPanelShell
        isLoading={isLoading}
        isEmpty={!activeSystemSuite}
        tabs={[]}
        activeTab={activeTab}
        onTabChange={setActiveTab}
        header={
          <div className="flex items-center justify-center h-32">
            <p className="text-sm text-m3-secondary">{t.selectSystemSuiteToView}</p>
          </div>
        }
      >
        <div />
      </DetailPanelShell>
    );
  }

  return (
    <DetailPanelShell<SystemSuiteTab>
      isLoading={false}
      isEmpty={false}
      tabs={tabs}
      activeTab={activeTab}
      onTabChange={setActiveTab}
      entityKey={activeSystemSuite.systemSuiteId}
      header={
        <SystemSuiteProfileCard
          systemSuite={activeSystemSuite}
          onSystemSuiteUpdate={onSystemSuiteUpdate}
          onEditingChange={onEditingChange}
        />
      }
    >
      <div className="space-y-4">
        {activeTab === 'overview' && (
          <div>
            <h3 className="text-sm font-medium text-m3-on-surface mb-2">{t.systemSuiteDetails}</h3>
            <dl className="space-y-2 text-sm">
              <div className="flex justify-between">
                <dt className="text-m3-secondary">{t.code}</dt>
                <dd className="text-m3-on-surface font-mono text-xs">{activeSystemSuite.code}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-m3-secondary">{t.name}</dt>
                <dd className="text-m3-on-surface">{activeSystemSuite.name}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-m3-secondary">{t.description}</dt>
                <dd className="text-m3-on-surface">{activeSystemSuite.description || '-'}</dd>
              </div>
              <div className="flex justify-between">
                <dt className="text-m3-secondary">{t.status}</dt>
                <dd className="text-m3-on-surface">{activeSystemSuite.status}</dd>
              </div>
            </dl>
          </div>
        )}
        {activeTab === 'modules' && (
          <p className="text-sm text-m3-secondary">{t.noModulesConfigured}</p>
        )}
        {activeTab === 'actions' && (
          <p className="text-sm text-m3-secondary">{t.noActionsConfigured}</p>
        )}
      </div>
    </DetailPanelShell>
  );
};
