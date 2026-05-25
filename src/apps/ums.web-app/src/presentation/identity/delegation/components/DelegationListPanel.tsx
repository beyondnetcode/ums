import React, { useCallback } from 'react';
import { Shield, ArrowRight, Info } from 'lucide-react';
import type { Delegation } from '@domain/identity/models/delegation.model';
import { StatusBadge } from '@shared/components/StatusBadge';
import { CodeBadge } from '@shared/components/CodeBadge';
import { EntityRow } from '@shared/components/EntityRow';
import { useI18n } from '@app/i18n/use-i18n';
import { useStatusLabel } from '@app/hooks/use-status-label';
import {
  M3DataView,
  SortOption,
  FilterOption,
  QueryCriteriaOption,
} from '@shared/components/M3DataView';
import { ApiErrorBanner } from '@shared/components/ApiErrorBanner';
import { DelegationViewType } from '@app/identity/hooks/use-delegation-dashboard';
import { Tooltip } from '@shared/components/Tooltip';

interface DelegationListPanelProps {
  delegations: Delegation[];
  selectedId: string;
  isLoading: boolean;
  error: Error | null;
  viewMode: 'list' | 'thumbnail';
  onViewModeChange: (mode: 'list' | 'thumbnail') => void;
  searchCriteria: string;
  onSearchCriteriaChange: (criteria: string) => void;
  searchValue: string;
  onSearchValueChange: (value: string) => void;
  onSearchSubmit: (event: React.FormEvent) => void;
  onRegisterNew: () => void;
  sortBy: string;
  onSortByChange: (value: string) => void;
  sortOrder: 'asc' | 'desc';
  onSortOrderToggle: () => void;
  activeFilter: string;
  onFilterChange: (value: string) => void;
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  startIndex: number;
  appliedTerm: string;
  onPageChange: (page: number) => void;
  onResetQuery: () => void;
  onSelectDelegation: (delegationId: string) => void;
  criteriaOptions: QueryCriteriaOption[];
  filterOptions: FilterOption[];
  sortOptions: SortOption[];
  delegationViewType: DelegationViewType;
  onDelegationViewTypeChange: (type: DelegationViewType) => void;
}

export const DelegationListPanel: React.FC<DelegationListPanelProps> = ({
  delegations,
  selectedId,
  isLoading,
  error,
  viewMode,
  onViewModeChange,
  searchCriteria,
  onSearchCriteriaChange,
  searchValue,
  onSearchValueChange,
  onSearchSubmit,
  onRegisterNew,
  sortBy,
  onSortByChange,
  sortOrder,
  onSortOrderToggle,
  activeFilter,
  onFilterChange,
  page,
  pageSize,
  totalItems,
  totalPages,
  startIndex,
  appliedTerm,
  onPageChange,
  onResetQuery,
  onSelectDelegation,
  criteriaOptions,
  filterOptions,
  sortOptions,
  delegationViewType,
  onDelegationViewTypeChange,
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
                Delegación de Acceso
              </span>
              <span className="ml-2 text-[10px] text-m3-secondary/70 font-mono">
                {delegation.scopeType}
              </span>
            </div>
          </div>
        }
        trailing={
          <div className="flex items-center gap-2">
            <CodeBadge code={delegation.scopeType} />
            <StatusBadge status={delegation.status} label={getStatusLabel(delegation.status)} />
            <ArrowRight className={`w-4 h-4 ${isSelected ? 'text-m3-primary' : 'text-m3-outline'}`} />
          </div>
        }
      />
    );
  }, [selectedId, onSelectDelegation, getStatusLabel]);

  const renderDelegationCard = useCallback((delegation: Delegation) => {
    const isSelected = delegation.delegationId === selectedId;
    return (
      <div
        key={delegation.delegationId}
        onClick={() => onSelectDelegation(delegation.delegationId)}
        className={`p-4 rounded-xl border cursor-pointer transition-colors ${
          isSelected 
            ? 'border-m3-primary bg-m3-primary/5' 
            : 'border-m3-outline/20 bg-m3-surface hover:bg-m3-surface-container'
        }`}
      >
        <div className="flex items-start justify-between mb-3">
          <div className="flex items-center gap-2">
            <Shield className={`w-5 h-5 ${isSelected ? 'text-m3-primary' : 'text-m3-secondary'}`} />
            <span className="text-sm font-medium">Delegación de Acceso</span>
          </div>
          <StatusBadge status={delegation.status} label={getStatusLabel(delegation.status)} />
        </div>
        <div className="flex items-center gap-2 mt-2">
          <span className="text-xs text-m3-on-surface-variant">Scope:</span>
          <CodeBadge code={delegation.scopeType} />
        </div>
      </div>
    );
  }, [selectedId, onSelectDelegation, getStatusLabel]);

  const footerTelemetry = (
    <div className="flex items-center gap-3">
      <div className="flex items-center gap-1.5">
        <span className="h-2 w-2 rounded-full bg-m3-primary animate-pulse" />
        <span className="text-xs font-medium text-m3-secondary/80">
          {t.showing ?? 'Showing'} {totalItems === 0 ? 0 : startIndex + 1}-{Math.min(startIndex + pageSize, totalItems)} {t.of ?? 'of'} {totalItems} Delegations
        </span>
      </div>
      {appliedTerm.trim() && (
        <button onClick={onResetQuery} className="text-xs font-medium text-rose-500 hover:underline flex items-center gap-1">
          <Info className="w-3 h-3" /> {t.clearFilter ?? 'Clear Filters'}
        </button>
      )}
    </div>
  );

  return (
    <div className="flex flex-col h-full">
      <div className="flex border-b border-m3-outline/15 px-4 pt-2 mb-2 bg-m3-surface-container/10">
        <Tooltip content="Delegaciones que has recibido (donde tú eres el administrador delegado).">
          <button 
            onClick={() => onDelegationViewTypeChange('received')}
            className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${delegationViewType === 'received' ? 'border-m3-primary text-m3-primary' : 'border-transparent text-m3-on-surface-variant hover:text-m3-on-surface'}`}
          >
            Received Delegations
          </button>
        </Tooltip>
        <Tooltip content="Delegaciones que has otorgado a otros (donde tú eres el administrador delegador).">
          <button 
            onClick={() => onDelegationViewTypeChange('granted')}
            className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors ${delegationViewType === 'granted' ? 'border-m3-primary text-m3-primary' : 'border-transparent text-m3-on-surface-variant hover:text-m3-on-surface'}`}
          >
            Granted Delegations
          </button>
        </Tooltip>
      </div>
      <div className="flex-1 min-h-0">
        <M3DataView
          title={'Delegation Management'}
          subtitle={delegationViewType === 'received' ? 'Delegations granted to you' : 'Delegations you have granted to others'}
          searchPlaceholder={t.searchPlaceholder ?? 'Search...'}
          searchCriteria={criteriaOptions}
          activeCriteria={searchCriteria}
          onCriteriaChange={onSearchCriteriaChange}
          searchValue={searchValue}
          onSearchValueChange={onSearchValueChange}
          onSearchSubmit={onSearchSubmit}
          onRegisterNew={onRegisterNew}
          registerLabel={t.newBtn ?? 'New'}
          viewMode={viewMode}
          onViewModeChange={onViewModeChange}
          sortOptions={sortOptions}
          sortBy={sortBy}
          onSortByChange={onSortByChange}
          sortOrder={sortOrder}
          onSortOrderToggle={onSortOrderToggle}
          filterOptions={filterOptions}
          activeFilter={activeFilter}
          onFilterChange={onFilterChange}
          isLoading={isLoading}
          isEmpty={totalItems === 0}
          emptyLabel={t.noRecords ?? 'No records found'}
          emptyTitle={t.dataViewEmptyTitle ?? 'No Results'}
          emptyTooltip={delegationViewType === 'received' 
            ? 'Es posible que no tengas delegaciones recibidas o que hayan expirado/sido revocadas. Revisa también tus delegaciones otorgadas.'
            : 'Aún no has otorgado ninguna delegación a otros usuarios o las que otorgaste expiraron y se depuraron.'}
          loadingLabel={t.dataViewLoading ?? 'Loading...'}
          criteriaLabel={t.dataViewCriteriaLabel ?? 'Search by'}
          searchTermLabel={t.dataViewSearchTermLabel ?? 'Search term'}
          searchButtonLabel={t.dataViewSearchBtn ?? 'Search'}
          renderList={() => (
            <>
              {error && <ApiErrorBanner error={error} />}
              <div className="overflow-x-auto border border-m3-outline/25 rounded-xl bg-m3-surface-container/20">
                <div className="divide-y divide-m3-outline/10 text-sm">
                  {delegations.map(renderDelegationRow)}
                </div>
              </div>
            </>
          )}
          renderThumbnail={() => (
            <>
              {error && <ApiErrorBanner error={error} />}
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                {delegations.map(renderDelegationCard)}
              </div>
            </>
          )}
          pagination={{ page, pageSize, totalItems, totalPages, onPageChange }}
          telemetryInfo={footerTelemetry}
        />
      </div>
    </div>
  );
};
