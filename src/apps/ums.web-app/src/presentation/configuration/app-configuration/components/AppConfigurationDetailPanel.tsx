/**
 * AppConfigurationDetailPanel
 */
import React from 'react';
import { Settings, Lock, Link, Clock, ShieldCheck, Edit2, Trash2 } from 'lucide-react';
import type { AppConfiguration } from '@domain/configuration/schemas/app-configuration.schema';
import { useI18n } from '@app/i18n/use-i18n';
import { DetailPanelShell } from '@shared/components/DetailPanelShell';
import { StatusBadge } from '@shared/components/StatusBadge';
import { CodeBadge } from '@shared/components/CodeBadge';
import { M3Button } from '@shared/components/M3Button';
import { SmartConfigInput } from '@presentation/shared/components/SmartConfigInput';
import { appConfigurationService } from '@infra/configuration/services/app-configuration.service';
import {
  usePublishAppConfiguration,
  useArchiveAppConfiguration,
} from '@app/configuration/hooks/use-app-configuration';
import { useNotificationStore } from '@app/stores/notification.store';

interface AppConfigurationDetailPanelProps {
  config?: AppConfiguration;
  onEdit?: () => void;
  onDelete?: () => void;
  triggerEditValue?: boolean;
}

export function AppConfigurationDetailPanel({
  config,
  onEdit,
  onDelete,
  triggerEditValue,
}: AppConfigurationDetailPanelProps): React.JSX.Element {
  const t = useI18n();
  const addNotification = useNotificationStore(s => s.addNotification);
  const publishMutation = usePublishAppConfiguration();
  const archiveMutation = useArchiveAppConfiguration();

  const [isEditingValue, setIsEditingValue] = React.useState(false);
  const [editValue, setEditValue] = React.useState('');

  React.useEffect(() => {
    if (triggerEditValue && config) {
      setEditValue(config.value);
      setIsEditingValue(true);
    }
  }, [triggerEditValue, config]);

  const handleStartEditValue = () => {
    if (config) {
      setEditValue(config.value);
      setIsEditingValue(true);
    }
  };

  const handleSaveValue = async () => {
    if (!config) return;
    try {
      await appConfigurationService.updateAppConfiguration(config.appConfigurationId, {
        value: editValue,
        description: config.description || '',
      });
      addNotification({
        title: t.success ?? 'Success',
        message: t.parameterUpdated ?? 'Parameter value updated',
        type: 'success',
      });
      setIsEditingValue(false);
    } catch {
      addNotification({
        title: t.error ?? 'Error',
        message: t.parameterUpdateFailed ?? 'Failed to update parameter value',
        type: 'error',
      });
    }
  };

  const handleCancelEditValue = () => {
    setIsEditingValue(false);
    setEditValue('');
  };

  const handlePublish = async () => {
    if (!config) return;
    try {
      await publishMutation.mutateAsync(config.appConfigurationId);
      addNotification({
        title: t.parameterPublished ?? 'Parameter Published',
        message: `${config.code} is now active`,
        type: 'success',
      });
    } catch {
      addNotification({
        title: t.publishFailed ?? 'Publish Failed',
        message: t.publishFailedMsg ?? 'Could not publish the parameter',
        type: 'error',
      });
    }
  };

  const handleArchive = async () => {
    if (!config) return;
    try {
      await archiveMutation.mutateAsync(config.appConfigurationId);
      addNotification({
        title: t.parameterArchived ?? 'Parameter Archived',
        message: `${config.code} has been archived`,
        type: 'warning',
      });
    } catch {
      addNotification({
        title: t.archiveFailed ?? 'Archive Failed',
        message: t.archiveFailedMsg ?? 'Could not archive the parameter',
        type: 'error',
      });
    }
  };

  const isLoading = publishMutation.isPending || archiveMutation.isPending;

  if (!config) {
    return (
      <DetailPanelShell
        isEmpty={true}
        emptyLabel={t.selectParameter ?? 'Select a parameter to view details'}
        entityKey=""
      />
    );
  }

  return (
    <DetailPanelShell
      isLoading={false}
      isEmpty={false}
      entityKey={config.appConfigurationId}
      header={
        <div className="flex items-center justify-between gap-2">
          <div className="flex items-center gap-2">
            <div className="p-1.5 rounded bg-m3-surface-container">
              <Settings className="w-4 h-4 text-m3-primary" />
            </div>
            <div className="min-w-0">
              <h2 className="font-semibold text-[12px] truncate">{config.code}</h2>
              <div className="flex items-center gap-1 mt-0.5">
                <CodeBadge code={config.scope} size="xs" />
                <StatusBadge status={config.status} label={config.status} size="xs" />
                {config.isEncrypted && (
                  <Lock className="w-3 h-3 text-amber-500" />
                )}
              </div>
            </div>
          </div>
          <div className="flex items-center gap-0.5">
            {onEdit && (
              <button
                onClick={onEdit}
                className="p-1 rounded hover:bg-m3-surface-variant text-m3-secondary hover:text-m3-primary transition-colors"
                title={t.editBtn ?? 'Edit'}
              >
                <Edit2 className="w-3.5 h-3.5" />
              </button>
            )}
            {onDelete && (
              <button
                onClick={onDelete}
                className="p-1 rounded hover:bg-rose-50 text-m3-secondary hover:text-rose-500 transition-colors"
                title={t.deleteBtn ?? 'Delete'}
              >
                <Trash2 className="w-3.5 h-3.5" />
              </button>
            )}
          </div>
        </div>
      }
    >
      <div className="space-y-3 p-3">
        {/* Description */}
        <div className="text-[10px] text-m3-on-surface-variant">
          {config.description || '-'}
        </div>

        {/* Value */}
        <div>
          <h3 className="text-[10px] font-semibold text-m3-on-surface-variant uppercase mb-1">
            {t.value ?? 'Value'}
          </h3>
          {config.isEncrypted ? (
            <div className="flex items-center gap-1 text-amber-600">
              <Lock className="w-3 h-3" />
              <span className="text-[10px]">[Encrypted]</span>
            </div>
          ) : isEditingValue ? (
            <div className="flex items-center gap-1.5">
              <SmartConfigInput
                code={config.code}
                value={editValue}
                onChange={setEditValue}
              />
              <button
                onClick={handleSaveValue}
                className="h-6 px-2.5 text-[10px] font-medium bg-m3-primary text-white rounded hover:bg-m3-primary/90 transition-colors"
              >
                {t.save ?? 'Save'}
              </button>
              <button
                onClick={handleCancelEditValue}
                className="h-6 px-2.5 text-[10px] font-medium text-m3-secondary hover:bg-m3-surface-variant rounded transition-colors"
              >
                {t.cancel ?? 'Cancel'}
              </button>
            </div>
          ) : (
            <div
              className="inline-flex items-center gap-1.5 bg-m3-surface-container px-2 py-0.5 rounded cursor-pointer hover:bg-m3-surface-container-low transition-colors"
              onClick={handleStartEditValue}
            >
              <span className="text-[12px] text-m3-on-surface truncate w-56">{config.value}</span>
              <Edit2 className="w-3.5 h-3.5 text-m3-outline flex-shrink-0" />
            </div>
          )}
        </div>

        {/* Metadata */}
        <div className="flex gap-4 text-[10px]">
          <div className="flex items-center gap-1">
            <Clock className="w-3 h-3 text-m3-secondary" />
            <span className="text-m3-on-surface-variant">v</span>
            <span className="text-m3-on-surface">{config.version}</span>
          </div>
          <div className="flex items-center gap-1">
            {config.isInheritable ? (
              <>
                <Link className="w-3 h-3 text-green-600" />
                <span className="text-green-600">Inheritable</span>
              </>
            ) : (
              <>
                <ShieldCheck className="w-3 h-3 text-m3-outline" />
                <span className="text-m3-outline">Local</span>
              </>
            )}
          </div>
        </div>

        {/* Tenant/Suite/Module info */}
        {(config.tenantId || config.systemSuiteId || config.moduleId) && (
          <div>
            <h3 className="text-[10px] font-semibold text-m3-on-surface-variant uppercase mb-1">
              {t.scopeDetails ?? 'Scope Details'}
            </h3>
            <div className="flex flex-wrap gap-1.5">
              {config.tenantId && (
                <CodeBadge code={`Tenant: ${config.tenantId.substring(0, 8)}...`} size="sm" />
              )}
              {config.systemSuiteId && (
                <CodeBadge code={`Suite: ${config.systemSuiteId.substring(0, 8)}...`} size="sm" />
              )}
              {config.moduleId && (
                <CodeBadge code={`Module: ${config.moduleId.substring(0, 8)}...`} size="sm" />
              )}
            </div>
          </div>
        )}

        {/* Actions */}
        <div className="flex gap-1.5 pt-2 border-t border-m3-outline/20">
          {config.status === 'Draft' && (
            <button
              onClick={handlePublish}
              disabled={isLoading}
              className="h-6 px-3 text-[10px] font-medium bg-m3-primary text-white rounded hover:bg-m3-primary/90 disabled:opacity-50 transition-colors"
            >
              {t.publish ?? 'Publish'}
            </button>
          )}
          {config.status === 'Published' && (
            <button
              onClick={handleArchive}
              disabled={isLoading}
              className="h-6 px-3 text-[10px] font-medium text-m3-secondary border border-m3-outline/30 rounded hover:bg-m3-surface-variant disabled:opacity-50 transition-colors"
            >
              {t.archive ?? 'Archive'}
            </button>
          )}
        </div>
      </div>
    </DetailPanelShell>
  );
}
