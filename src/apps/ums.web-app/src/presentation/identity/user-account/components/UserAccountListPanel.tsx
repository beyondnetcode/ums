import React, { useCallback } from 'react';
import { Mail, Layers, ArrowRight, Info, Building2 } from 'lucide-react';
import { UserAccount } from '@domain/identity/models/user-account.model';
import { Tenant } from '@domain/identity/models/tenant.model';
import { StatusBadge } from '@shared/components/StatusBadge';
import { CodeBadge } from '@shared/components/CodeBadge';
import {
  DataViewShell,
  DataList,
  AtomicQueryCriteriaOption,
  AtomicFilterOption,
  AtomicSortOption,
  PaginationFooter,
  RequiresFilterPrompt,
} from '@shared/components';
import { ListToolbar } from '@shared/components/ListToolbar';
import { EntityRow } from '@shared/components/EntityRow';
import { EntityCard } from '@shared/components/EntityCard';
import { M3Card } from '@shared/components/M3Card';
import { useI18n } from '@app/i18n/use-i18n';
import { useStatusLabel } from '@app/hooks/use-status-label';
import { TenantSelector } from '@presentation/identity/tenant/components/TenantSelector';
import { ApiErrorBanner } from '@shared/components/ApiErrorBanner';
import { useQueryState } from '@app/shared/hooks/use-query-state';
import { usePaginationState } from '@app/shared/hooks/use-pagination-state';

interface UserAccountListPanelProps {
  accounts: UserAccount[];
  selectedId: string;
  isLoading: boolean;
  error: Error | null;
  viewMode: 'list' | 'thumbnail';
  onViewModeChange: (mode: 'list' | 'thumbnail') => void;
  queryState: ReturnType<typeof useQueryState<string, string>>;
  paginationState: ReturnType<typeof usePaginationState> & {
    totalItems: number;
    totalPages: number;
  };
  onRegisterNew: () => void;
  onSelectAccount: (accountId: string) => void;
  criteriaOptions: AtomicQueryCriteriaOption[];
  filterOptions: AtomicFilterOption[];
  sortOptions: AtomicSortOption[];
  tenants?: Tenant[];
  selectedTenantId?: string;
  onTenantChange?: (tenantId: string) => void;
  sessionTenantName?: string;
  requiresFilter?: boolean;
}

export const UserAccountListPanel: React.FC<UserAccountListPanelProps> = ({
  accounts,
  selectedId,
  isLoading,
  error,
  viewMode,
  onViewModeChange,
  queryState,
  paginationState,
  onRegisterNew,
  onSelectAccount,
  criteriaOptions,
  filterOptions,
  sortOptions,
  tenants,
  selectedTenantId,
  onTenantChange,
  sessionTenantName,
  requiresFilter = false,
}) => {
  const t = useI18n();
  const getStatusLabel = useStatusLabel();

  const renderAccountRow = useCallback(
    (account: UserAccount) => {
      const isSelected = account.userAccountId === selectedId;
      return (
        <EntityRow
          key={account.userAccountId}
          id={account.userAccountId}
          isActive={account.status === 'Active'}
          selected={isSelected}
          onClick={() => onSelectAccount(account.userAccountId)}
          leading={
            <div
              className={`p-2 rounded-lg transition-colors ${isSelected ? 'bg-m3-primary/15' : 'bg-m3-surface-container/50'}`}
            >
              <Mail className={`w-4 h-4 ${isSelected ? 'text-m3-primary' : 'text-m3-secondary'}`} />
            </div>
          }
          trailingColumns={[
            { content: <CodeBadge code={account.category} />, width: 'w-20' },
            {
              content: (
                <StatusBadge status={account.status} label={getStatusLabel(account.status)} />
              ),
              width: 'w-20',
            },
            {
              content: (
                <ArrowRight
                  className={`w-4 h-4 transition-transform ${isSelected ? 'text-m3-primary translate-x-0.5' : 'text-m3-outline/30'}`}
                />
              ),
              width: 'w-5',
            },
          ]}
        >
          <span className="text-sm font-semibold text-m3-on-surface line-clamp-1">
            {account.email}
          </span>
        </EntityRow>
      );
    },
    [selectedId, onSelectAccount, getStatusLabel]
  );

  const renderAccountCard = useCallback(
    (account: UserAccount) => {
      const isSelected = account.userAccountId === selectedId;
      return (
        <EntityCard
          key={account.userAccountId}
          selected={isSelected}
          onClick={() => onSelectAccount(account.userAccountId)}
          icon={<Mail className="w-5 h-5" />}
          title={account.email}
          subtitle={account.category}
          badges={
            <>
              <CodeBadge code={account.category} />
              <StatusBadge status={account.status} label={getStatusLabel(account.status)} />
            </>
          }
        />
      );
    },
    [selectedId, onSelectAccount, getStatusLabel]
  );

  const totalItems = paginationState.totalItems;

  const pagination =
    totalItems > 0
      ? {
          page: paginationState.page,
          pageSize: paginationState.pageSize,
          totalItems,
          totalPages: paginationState.totalPages ?? 1,
          onPageChange: paginationState.handlePageChange ?? paginationState.setPage,
          onPageSizeChange: paginationState.handlePageSizeChange,
        }
      : undefined;

  const filterPrompt = requiresFilter ? (
    <RequiresFilterPrompt title={t.applyFilterTitle} message={t.applyFilterMessage} />
  ) : null;

  return (
    <div className="flex flex-col h-full gap-4">
      <M3Card
        variant="elevated"
        className="flex-shrink-0 border border-m3-outline/25 bg-m3-surface-container/20 shadow-sm p-4"
      >
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div className="flex items-center gap-2">
            <div className="p-1.5 bg-m3-primary/10 rounded-lg text-m3-primary border border-m3-primary/10 flex-shrink-0">
              <Layers className="w-4 h-4" />
            </div>
            <div>
              <span className="text-xs font-semibold text-m3-secondary uppercase tracking-wider block">
                {t.activeTenant}
              </span>
              <span className="text-[10px] text-m3-secondary/70">
                {onTenantChange ? t.filterAccountsByTenant : t.sessionTenantContext}
              </span>
            </div>
          </div>
          <div className="w-full sm:w-72">
            {onTenantChange && tenants ? (
              <TenantSelector
                tenants={tenants}
                selectedTenantId={selectedTenantId}
                onTenantChange={onTenantChange}
                label={t.activeTenant}
                className="mb-0"
              />
            ) : (
              <div className="flex items-center gap-2 px-3 py-2 rounded-lg border border-m3-outline/30 bg-m3-surface-container/30">
                <Building2 className="w-4 h-4 text-m3-primary" />
                <span className="text-sm font-medium text-m3-on-surface">
                  {sessionTenantName || selectedTenantId?.substring(0, 8) || 'No disponible'}
                </span>
                <span className="text-[10px] text-m3-secondary/60 ml-auto">Solo lectura</span>
              </div>
            )}
          </div>
        </div>
      </M3Card>

      <div className="flex-1 min-h-0">
        <DataViewShell
          title={t.userAccounts}
          subtitle={t.userAccountMaintenanceSubtitle}
          onRegisterNew={onRegisterNew}
          registerLabel={t.registerNew}
          controls={
            <ListToolbar
              itemCount={totalItems}
              itemLabel="cuenta"
              viewMode={viewMode}
              onViewModeChange={onViewModeChange}
              searchOptions={criteriaOptions}
              activeSearchCriteria={queryState.searchCriteria}
              onSearchCriteriaChange={queryState.setSearchCriteria}
              searchValue={queryState.searchValue}
              onSearchValueChange={queryState.setSearchValue}
              onSearchSubmit={queryState.handleQuerySubmit}
              onSearchClear={queryState.handleResetQuery}
              filterOptions={filterOptions}
              activeFilter={queryState.activeFilter}
              onFilterChange={queryState.setActiveFilter}
              sortOptions={sortOptions}
              sortBy={queryState.sortBy}
              onSortByChange={queryState.setSortBy}
              sortOrder={queryState.sortOrder}
              onSortOrderToggle={queryState.toggleSortOrder}
              onAdd={onRegisterNew}
            />
          }
          content={
            requiresFilter ? (
              filterPrompt
            ) : (
              <DataList
                isLoading={isLoading}
                isEmpty={!isLoading && accounts.length === 0}
                emptyLabel={queryState.appliedQuery.term ? t.noResultsFound : t.noAccountsFound}
                emptyTitle={queryState.appliedQuery.term ? t.noResultsTitle : t.noAccountsTitle}
                viewMode={viewMode}
                renderList={() => (
                  <>
                    {error && <ApiErrorBanner error={error} />}
                    <div className="flex flex-col gap-0.5">{accounts.map(renderAccountRow)}</div>
                  </>
                )}
                renderThumbnail={() => (
                  <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                    {accounts.map(renderAccountCard)}
                  </div>
                )}
                pagination={pagination}
              />
            )
          }
        />
      </div>
    </div>
  );
};
