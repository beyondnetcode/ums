import React, { useCallback } from 'react';
import { Building2, ArrowRight, Check, Info, ChevronRight } from 'lucide-react';
import { Tenant } from '@domain/identity/models/tenant.model';
import { M3Card } from '@shared/components/M3Card';
import { StatusBadge } from '@shared/components/StatusBadge';
import {
  HierarchicalList,
  HierarchicalRow,
  HierarchicalExpandButton,
} from '@shared/components/HierarchicalList';
import type { TreeNode } from '@app/hooks/use-tree-nodes';
import {
  M3DataView,
  SortOption,
  FilterOption,
  QueryCriteriaOption,
} from '@shared/components/M3DataView';
import { useI18n } from '@app/i18n/use-i18n';
import { useStatusLabel } from '@app/hooks/use-status-label';

interface TenantListPanelProps {
  tenants: Tenant[];
  selectedId: string;
  isLoading: boolean;
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
    (node: TreeNode<Tenant>, isSelected: boolean, isExpanded: boolean, onToggle: () => void) => {
      const id = node.item.tenantId;
      const tenant = node.item;
      const hasChildren = node.children.length > 0;

      return (
        <HierarchicalRow
          key={id}
          hasChildren={hasChildren}
          isExpanded={isExpanded}
          isChild={false}
          onToggleExpand={onToggle}
          onClick={() => onSelectTenant(id)}
          isSelected={isSelected}
        >
          <td className="py-3.5 px-5">
            <div className="flex items-center gap-3">
              <HierarchicalExpandButton
                hasChildren={hasChildren}
                isExpanded={isExpanded}
                onClick={(e) => { e.stopPropagation(); onToggle(); }}
              />
              <div className={`p-2 rounded-lg border transition-colors ${
                isSelected
                  ? 'bg-m3-primary text-white border-m3-primary'
                  : 'bg-m3-surface-container/60 border-m3-outline/20 text-m3-secondary group-hover:text-m3-primary group-hover:border-m3-primary/30'
              }`}>
                <Building2 className="w-4 h-4" />
              </div>
              <div>
                <p className="font-medium text-m3-on-surface">{tenant.name}</p>
                <p className="text-xs text-m3-secondary/60 truncate max-w-[170px] md:max-w-xs">{tenant.companyReference || tenant.type}</p>
              </div>
            </div>
          </td>
          <td className="py-3.5 px-4 font-mono text-xs font-medium text-m3-on-surface">{tenant.code}</td>
          <td className="py-3.5 px-4 text-xs">{tenant.type}</td>
          <td className="py-3.5 px-4">
            <StatusBadge status={tenant.status} label={statusLabel(tenant.status)} />
          </td>
          <td className="py-3.5 px-5 text-right">
            <div className="flex items-center justify-end gap-1.5">
              {isSelected && (
                <span className="h-5 w-5 bg-m3-primary text-m3-on-primary rounded-full flex items-center justify-center">
                  <Check className="w-3 h-3" />
                </span>
              )}
              <span className="text-xs font-medium text-m3-primary opacity-0 group-hover:opacity-100 group-hover:translate-x-0.5 transition-all flex items-center gap-1">
                {t.manage} <ArrowRight className="w-3.5 h-3.5" />
              </span>
            </div>
          </td>
        </HierarchicalRow>
      );
    },
    [onSelectTenant, statusLabel, t],
  );

  const renderChildRow = useCallback(
    (child: Tenant, isChildSelected: boolean) => {
      const hasChildren = false;
      return (
        <HierarchicalRow
          key={child.tenantId}
          hasChildren={hasChildren}
          isExpanded={false}
          isChild={true}
          onToggleExpand={() => {}}
          onClick={() => onSelectTenant(child.tenantId)}
          isSelected={isChildSelected}
        >
          <td className="py-3.5 pl-10">
            <div className="flex items-center gap-3">
              <span className="w-4" />
              <div className="p-2 rounded-lg border bg-m3-surface-container/40 border-m3-outline/15 text-m3-secondary/70">
                <Building2 className="w-4 h-4" />
              </div>
              <div>
                <p className="text-xs font-medium text-m3-on-surface">{child.name}</p>
                <p className="text-[10px] text-m3-secondary/60 truncate max-w-[170px] md:max-w-xs">{child.companyReference || child.type}</p>
              </div>
            </div>
          </td>
          <td className="py-3.5 px-4 font-mono text-[10px] font-medium text-m3-on-surface opacity-70">{child.code}</td>
          <td className="py-3.5 px-4 text-[10px] opacity-70">{child.type}</td>
          <td className="py-3.5 px-4">
            <StatusBadge status={child.status} label={statusLabel(child.status)} />
          </td>
          <td className="py-3.5 px-5 text-right">
            <div className="flex items-center justify-end gap-1.5">
              {isChildSelected && (
                <span className="h-5 w-5 bg-m3-primary text-m3-on-primary rounded-full flex items-center justify-center">
                  <Check className="w-3 h-3" />
                </span>
              )}
              <span className="text-xs font-medium text-m3-primary opacity-0 group-hover:opacity-100 group-hover:translate-x-0.5 transition-all flex items-center gap-1">
                {t.manage} <ArrowRight className="w-3.5 h-3.5" />
              </span>
            </div>
          </td>
        </HierarchicalRow>
      );
    },
    [onSelectTenant, statusLabel, t],
  );

  const renderParentCard = useCallback(
    (node: TreeNode<Tenant>, isSelected: boolean, _isExpanded: boolean, onToggle: () => void) => {
      const id = node.item.tenantId;
      const tenant = node.item;
      const children = node.children;
      const hasChildren = children.length > 0;

      return (
        <M3Card
          key={id}
          onClick={() => onSelectTenant(id)}
          variant={isSelected ? 'elevated' : 'filled'}
          className={`p-5 cursor-pointer border transition-all duration-150 hover:-translate-y-0.5 hover:shadow-md ${
            isSelected ? 'border-m3-primary bg-m3-primary-container/15' : 'border-m3-outline/25 hover:border-m3-primary/30'
          }`}
        >
          <div className="flex justify-between items-start gap-4">
            <div className="flex gap-3 flex-1">
              <div className={`p-2.5 rounded-lg border ${isSelected ? 'bg-m3-primary text-white border-m3-primary' : 'bg-m3-primary/10 text-m3-primary border-m3-primary/10'}`}>
                <Building2 className="w-5 h-5" />
              </div>
              <div className="flex-1 min-w-0">
                <h4 className="text-sm font-medium text-m3-on-surface line-clamp-1">{tenant.name}</h4>
                <p className="font-mono text-xs text-m3-secondary/70 mt-0.5">{tenant.code}</p>
              </div>
            </div>
            <div className="flex items-center gap-2">
              <StatusBadge status={tenant.status} label={statusLabel(tenant.status)} />
              {hasChildren && (
                <button
                  onClick={(e) => { e.stopPropagation(); onToggle(); }}
                  className="p-1 rounded transition-transform duration-200"
                >
                  <ChevronRight className="w-4 h-4 text-m3-secondary" />
                </button>
              )}
            </div>
          </div>
          <div className="mt-4 pt-3 border-t border-m3-outline/10 grid grid-cols-2 gap-2 text-xs">
            <div>
              <p className="text-m3-secondary font-medium">{t.colCategory}</p>
              <p className="font-medium text-m3-on-surface mt-0.5">{tenant.type}</p>
            </div>
            <div>
              <p className="text-m3-secondary font-medium">{t.ref}</p>
              <p className="font-medium text-m3-on-surface truncate mt-0.5">{tenant.companyReference || '-'}</p>
            </div>
          </div>
          {hasChildren && (
            <div className="mt-2 text-[10px] text-m3-secondary/60">
              {children.length} {children.length === 1 ? t.tenant : t.tenants}
            </div>
          )}
        </M3Card>
      );
    },
    [onSelectTenant, statusLabel, t],
  );

  const renderChildCard = useCallback(
    (child: Tenant, isChildSelected: boolean) => (
      <M3Card
        key={child.tenantId}
        onClick={() => onSelectTenant(child.tenantId)}
        variant={isChildSelected ? 'elevated' : 'outlined'}
        className={`p-4 cursor-pointer border transition-all duration-150 ${
          isChildSelected ? 'border-m3-primary bg-m3-primary-container/10' : 'border-m3-outline/15 hover:border-m3-primary/20'
        }`}
      >
        <div className="flex justify-between items-start gap-3">
          <div className="flex gap-2.5">
            <div className={`p-2 rounded-lg border ${isChildSelected ? 'bg-m3-primary/80 text-white border-m3-primary/80' : 'bg-m3-surface-container/40 border-m3-outline/15 text-m3-secondary/70'}`}>
              <Building2 className="w-4 h-4" />
            </div>
            <div>
              <h4 className="text-xs font-medium text-m3-on-surface">{child.name}</h4>
              <p className="font-mono text-[10px] text-m3-secondary/60 mt-0.5">{child.code}</p>
            </div>
          </div>
          <StatusBadge status={child.status} label={statusLabel(child.status)} />
        </div>
        <div className="mt-3 pt-2 border-t border-m3-outline/10 grid grid-cols-2 gap-2 text-[10px]">
          <div>
            <p className="text-m3-secondary font-medium">{t.colCategory}</p>
            <p className="font-medium text-m3-on-surface mt-0.5">{child.type}</p>
          </div>
          <div>
            <p className="text-m3-secondary font-medium">{t.ref}</p>
            <p className="font-medium text-m3-on-surface truncate mt-0.5">{child.companyReference || '-'}</p>
          </div>
        </div>
      </M3Card>
    ),
    [onSelectTenant, statusLabel, t],
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
        <div className="overflow-x-auto border border-m3-outline/25 rounded-xl bg-m3-surface-container/20">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="border-b border-m3-outline/20 text-xs font-medium text-m3-secondary bg-m3-surface-container/40">
                <th className="py-3.5 px-5">{t.colTenantName}</th>
                <th className="py-3.5 px-4">{t.colCode}</th>
                <th className="py-3.5 px-4">{t.colCategory}</th>
                <th className="py-3.5 px-4">{t.colStatus}</th>
                <th className="py-3.5 px-5 text-right">{t.colAction}</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-m3-outline/10 text-sm">
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
            </tbody>
          </table>
        </div>
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
