import React from 'react';
import { useI18n } from '@app/i18n/use-i18n';
import { useTenantDashboard } from '@app/identity/hooks/use-tenant-dashboard';
import { TenantForm } from '../components/TenantForm';
import { TenantDetailPanel } from '../components/TenantDetailPanel';
import { TenantListPanel } from '../components/TenantListPanel';
import { PageShell } from '@shared/layouts/PageShell';
import { MasterDetailLayout } from '@shared/layouts/MasterDetailLayout';
import { M3Dialog } from '@shared/components/M3Dialog';
import { SortOption, FilterOption, QueryCriteriaOption } from '@shared/components/M3DataView';

export default function TenantDashboardScreen(): React.JSX.Element {
  const t = useI18n();
  const dashboard = useTenantDashboard();

  const criteriaOptions: QueryCriteriaOption[] = [
    { label: t.byName, value: 'name' },
    { label: t.byCode, value: 'code' },
    { label: t.byTenantId, value: 'id' },
  ];
  const filterOptions: FilterOption[] = [
    { label: t.allStatuses, value: 'all' },
    { label: t.active, value: 'Active' },
    { label: t.suspended, value: 'Suspended' },
  ];
  const sortOptions: SortOption[] = [
    { label: t.sortByName, value: 'name' },
    { label: t.sortByCode, value: 'code' },
    { label: t.sortByStatus, value: 'status' },
  ];

  return (
    <PageShell>
      <MasterDetailLayout
        splitterLabel="Resize tenant detail panel"
        overlay={
          <>
            <M3Dialog
              open={dashboard.showDiscardDialog}
              title={t.unsavedChanges}
              message={t.unsavedChangesMsg}
              onScrimClick={() => dashboard.setShowDiscardDialog(false)}
              actions={[
                { label: t.cancelEdit, variant: 'outlined', onClick: () => dashboard.setShowDiscardDialog(false) },
                { label: t.discardChanges, variant: 'filled', className: 'bg-m3-error hover:bg-m3-error/90 border-0', onClick: dashboard.confirmDiscard },
              ]}
            />
            <TenantForm isOpen={dashboard.isCreateOpen} onClose={() => dashboard.setIsCreateOpen(false)} onSuccess={dashboard.handleCreateSuccess} />
          </>
        }
        master={
          <TenantListPanel
            tenants={dashboard.knownTenants}
            selectedId={dashboard.selectedId}
            isLoading={dashboard.isLoadingList}
            error={dashboard.listError}
            viewMode={dashboard.viewMode}
            onViewModeChange={dashboard.setViewMode}
            searchCriteria={dashboard.searchCriteria}
            onSearchCriteriaChange={dashboard.setSearchCriteria}
            searchValue={dashboard.searchValue}
            onSearchValueChange={dashboard.setSearchValue}
            onSearchSubmit={dashboard.handleQuerySubmit}
            onRegisterNew={() => dashboard.setIsCreateOpen(true)}
            sortBy={dashboard.sortBy}
            onSortByChange={dashboard.setSortBy}
            sortOrder={dashboard.sortOrder}
            onSortOrderToggle={() => dashboard.setSortOrder((o) => o === 'asc' ? 'desc' : 'asc')}
            activeFilter={dashboard.activeFilter}
            onFilterChange={(val) => { dashboard.setActiveFilter(val); dashboard.setPage(1); }}
            page={dashboard.page}
            pageSize={dashboard.pageSize}
            totalItems={dashboard.totalItems}
            totalPages={dashboard.totalPages}
            startIndex={dashboard.startIndex}
            appliedTerm={dashboard.appliedQuery.term}
            onPageChange={dashboard.setPage}
            onResetQuery={dashboard.handleResetQuery}
            onSelectTenant={dashboard.handleSelectTenant}
            criteriaOptions={criteriaOptions}
            filterOptions={filterOptions}
            sortOptions={sortOptions}
          />
        }
        detail={
          <TenantDetailPanel
            selectedId={dashboard.selectedId}
            activeTenant={dashboard.activeTenant}
            parentTenant={dashboard.parentTenant}
            isRootTenant={dashboard.isRootTenant}
            isLoading={dashboard.isLoadingList}
            activeConsoleTab={dashboard.activeConsoleTab}
            consoleTabs={dashboard.consoleTabs}
            onConsoleTabChange={dashboard.setActiveConsoleTab}
            onTenantUpdate={dashboard.patchTenant}
            onTenantEditingChange={dashboard.setIsTenantEditing}
          />
        }
      />
    </PageShell>
  );
};
