import React, { useCallback } from 'react';
import { Tenant } from '@domain/identity/models/tenant.model';
import {
  HierarchicalList,
} from '@shared/components/HierarchicalList';
import {
  renderTenantParentRow,
  renderTenantChildRow,
  renderTenantParentCard,
  renderTenantChildCard,
} from './tenant-list-renders';
import type { TreeNode } from '@app/hooks/use-tree-nodes';
import {
  M3DataView,
  SortOption,
  FilterOption,
  QueryCriteriaOption,
} from '@shared/components/M3DataView';
import { ApiErrorBanner } from '@shared/components/ApiErrorBanner';
import { useI18n } from '@app/i18n/use-i18n';
import { useStatusLabel } from '@app/hooks/use-status-label';

interface TenantListPanelProps {
  tenants: Tenant[];
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
  onSelectTenant: (tenantId: string) => void;
  criteriaOptions: QueryCriteriaOption[];
  filterOptions: FilterOption[];
  sortOptions: SortOption[];
}

export const TenantListPanel: React.FC<TenantListPanelProps> = ({
  tenants,
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
  onSelectTenant,
  criteriaOptions,
  filterOptions,
  sortOptions,
}) => {
  const t = useI18n();
  const statusLabel = useStatusLabel();

  const renderParentRow = useCallback(
    (node: TreeNode<Tenant>, isSelected: boolean, isExpanded: boolean, onToggle: () => void) => 
      renderTenantParentRow(node, isSelected, isExpanded, onToggle, onSelectTenant, statusLabel, t),
    [onSelectTenant, statusLabel, t]
  );

  const renderChildRow = useCallback(
    (child: Tenant, isChildSelected: boolean) => 
      renderTenantChildRow(child, isChildSelected, onSelectTenant, statusLabel, t),
    [onSelectTenant, statusLabel, t]
  );

  const renderParentCard = useCallback(
    (node: TreeNode<Tenant>, isSelected: boolean, isExpanded: boolean, onToggle: () => void) => 
      renderTenantParentCard(node, isSelected, isExpanded, onToggle, onSelectTenant, statusLabel, t),
    [onSelectTenant, statusLabel, t]
  );

  const renderChildCard = useCallback(
    (child: Tenant, isChildSelected: boolean) => 
      renderTenantChildCard(child, isChildSelected, onSelectTenant, statusLabel, t),
    [onSelectTenant, statusLabel, t]
  );

  const footerTelemetry = (
    <div className="flex items-center gap-3">
      <div className="flex items-center gap-1.5">
        <span className="h-2 w-2 rounded-full bg-m3-primary animate-pulse" />
        <span className="text-xs font-medium text-m3-secondary/80">
          {t.showing} {totalItems === 0 ? 0 : startIndex + 1}-{Math.min(startIndex + pageSize, totalItems)} {t.of} {totalItems} {t.tenants}
        </span>
      </div>
      {appliedTerm.trim() && (
        <button onClick={onResetQuery} className="text-xs font-medium text-rose-500 hover:underline flex items-center gap-1">
          <Info className="w-3 h-3" /> {t.clearFilter}
        </button>
      )}
    </div>
  );

  return (
    <M3DataView
      title={t.tenantMaintenance}
      subtitle={t.tenantMaintenanceSubtitle}
      searchPlaceholder={t.searchPlaceholder}
      searchCriteria={criteriaOptions}
      activeCriteria={searchCriteria}
      onCriteriaChange={onSearchCriteriaChange}
      searchValue={searchValue}
      onSearchValueChange={onSearchValueChange}
      onSearchSubmit={onSearchSubmit}
      onRegisterNew={onRegisterNew}
      registerLabel={t.newBtn}
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
      emptyLabel={t.noRecords}
      emptyTitle={t.dataViewEmptyTitle}
      loadingLabel={t.dataViewLoading}
      criteriaLabel={t.dataViewCriteriaLabel}
      searchTermLabel={t.dataViewSearchTermLabel}
      searchButtonLabel={t.dataViewSearchBtn}
      renderList={() => (
        <>
          {error && <ApiErrorBanner error={error} />}
          <div className="flex flex-col gap-0.5">
            <HierarchicalList<Tenant>
              items={tenants}
              idKey="tenantId"
              parentIdKey="parentTenantId"
              selectedId={selectedId}
              onSelect={onSelectTenant}
              renderParentRow={renderParentRow}
              renderChildRow={renderChildRow}
              renderParentCard={renderParentCard}
              renderChildCard={renderChildCard}
              viewMode={viewMode}
            />
          </div>
        </>
      )}
      renderThumbnail={() => (
        <HierarchicalList<Tenant>
          items={tenants}
          idKey="tenantId"
          parentIdKey="parentTenantId"
          selectedId={selectedId}
          onSelect={onSelectTenant}
          renderParentRow={renderParentRow}
          renderChildRow={renderChildRow}
          renderParentCard={renderParentCard}
          renderChildCard={renderChildCard}
          viewMode={viewMode}
        />
      )}
      pagination={{ page, pageSize, totalItems, totalPages, onPageChange }}
      telemetryInfo={footerTelemetry}
    />
  );
};
