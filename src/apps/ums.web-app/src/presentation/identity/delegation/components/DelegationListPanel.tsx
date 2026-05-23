import React, { useCallback } from 'react';
import { ArrowRight, Shield, Info, WifiOff, AlertTriangle } from 'lucide-react';
import type { Delegation } from '@domain/identity/models/delegation.model';
import { StatusBadge } from '@shared/components/StatusBadge';
import { CodeBadge } from '@shared/components/CodeBadge';
import { EntityRow } from '@shared/components/EntityRow';
import { useI18n } from '@app/i18n/use-i18n';
import { useStatusLabel } from '@app/hooks/use-status-label';
import { GraphQlValidationError, GraphQlUnavailableError } from '@infra/http/graphqlClient';

interface DelegationListPanelProps {
  delegations: Delegation[];
  selectedId: string;
  isLoading: boolean;
  error: Error | null;
  title: string;
  emptyLabel: string;
  onSelectDelegation: (delegationId: string) => void;
  onRegisterNew?: () => void;
}

function formatErrorMessage(error: Error): string {
  if (error instanceof GraphQlValidationError) {
    return error.details.join('. ');
  }
  return error.message;
}

function ErrorBanner({ error }: { error: Error }) {
  const isUnavailable = error instanceof GraphQlUnavailableError;
  const isValidation = error instanceof GraphQlValidationError;

  const icon = isUnavailable ? (
    <WifiOff className="w-5 h-5 text-amber-500 mt-0.5 flex-shrink-0" />
  ) : isValidation ? (
    <AlertTriangle className="w-5 h-5 text-rose-500 mt-0.5 flex-shrink-0" />
  ) : (
    <Info className="w-5 h-5 text-rose-500 mt-0.5 flex-shrink-0" />
  );

  const title = isUnavailable
    ? 'Backend API Unavailable'
    : isValidation
      ? 'Invalid Request'
      : 'Error';

  const hint = isUnavailable
    ? 'Start the backend API and refresh.'
    : isValidation
      ? 'Check the request parameters and try again.'
      : 'Ensure the backend API is running and try again.';

  return (
    <div className="mb-4 p-4 rounded-xl border border-rose-200 bg-rose-50">
      <div className="flex items-start gap-3">
        {icon}
        <div>
          <p className="text-sm font-medium text-rose-800">{title}</p>
          <p className="text-xs text-rose-700 mt-1">{formatErrorMessage(error)}</p>
          <p className="text-xs text-rose-600 mt-2">{hint}</p>
        </div>
      </div>
    </div>
  );
}

export const DelegationListPanel: React.FC<DelegationListPanelProps> = ({
  delegations,
  selectedId,
  isLoading,
  error,
  title,
  emptyLabel,
  onSelectDelegation,
  onRegisterNew,
}) => {
  const t = useI18n();
  const getStatusLabel = useStatusLabel();

  const renderDelegationRow = useCallback((delegation: Delegation) => {
    const isSelected = delegation.delegationId === selectedId;
    return (
      <EntityRow
        key={delegation.delegationId}
        selected={isSelected}
        onClick={() => onSelectDelegation(delegation.delegationId)}
        leading={
          <div className="flex items-center gap-3">
            <div className={`p-2 rounded-lg ${isSelected ? 'bg-m3-primary/15' : 'bg-m3-surface-container/50'}`}>
              <Shield className={`w-4 h-4 ${isSelected ? 'text-m3-primary' : 'text-m3-secondary'}`} />
            </div>
            <div>
              <span className="text-sm font-semibold text-m3-on-surface">
                {delegation.delegationId.substring(0, 8)}...
              </span>
              <span className="ml-2 text-[10px] text-m3-secondary/70 font-mono">
                {delegation.scopeType}
              </span>
            </div>
          </div>
        }
        trailing={
          <div className="flex items-center gap-2">
            <CodeBadge label={delegation.scopeType} />
            <StatusBadge status={delegation.status} label={getStatusLabel(delegation.status)} />
            <ArrowRight className={`w-4 h-4 ${isSelected ? 'text-m3-primary' : 'text-m3-outline'}`} />
          </div>
        }
      />
    );
  }, [selectedId, onSelectDelegation, getStatusLabel]);

  return (
    <div className="flex flex-col h-full">
      <div className="flex items-center justify-between px-4 py-3 border-b border-m3-outline/10">
        <h2 className="text-sm font-semibold text-m3-on-surface">{title}</h2>
        {onRegisterNew && (
          <button
            onClick={onRegisterNew}
            className="text-xs text-m3-primary hover:underline"
            type="button"
          >
            {t.registerNew ?? 'New'}
          </button>
        )}
      </div>

      <div className="flex-1 overflow-y-auto px-2 py-2">
        {isLoading && (
          <p className="text-xs text-m3-secondary px-2 py-4 text-center">
            {t.loadingAccounts ?? 'Loading...'}
          </p>
        )}

        {!isLoading && error && <ErrorBanner error={error} />}

        {!isLoading && !error && delegations.length === 0 && (
          <p className="text-xs text-m3-secondary px-2 py-4 text-center">{emptyLabel}</p>
        )}

        {!isLoading && delegations.length > 0 && (
          <div className="divide-y divide-m3-outline/10">
            {delegations.map(renderDelegationRow)}
          </div>
        )}
      </div>
    </div>
  );
};
