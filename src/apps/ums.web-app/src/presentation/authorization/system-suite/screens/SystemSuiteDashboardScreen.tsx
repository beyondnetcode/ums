import React from 'react';
import { useI18n } from '@app/i18n/use-i18n';
import { useSystemSuiteDashboard } from '@app/authorization/hooks/use-system-suite-dashboard';
import { SystemSuiteForm } from '../components/SystemSuiteForm';
import { SystemSuiteDetailPanel } from '../components/SystemSuiteDetailPanel';
import { SystemSuiteListPanel } from '../components/SystemSuiteListPanel';
import { PageShell } from '@shared/layouts/PageShell';
import { MasterDetailLayout } from '@shared/layouts/MasterDetailLayout';
import { M3Dialog } from '@shared/components/M3Dialog';
import { SortOption, FilterOption, QueryCriteriaOption } from '@shared/components/M3DataView';

export default function SystemSuiteDashboardScreen(): React.JSX.Element {
  const t = useI18n();
  const dashboard = useSystemSuiteDashboard();

  const criteriaOptions: QueryCriteriaOption[] = [
    { label: t.byName, value: 'name' },
    { label: t.byCode, value: 'code' },
  ];
  const filterOptions: FilterOption[] = [
    { label: t.allStatuses, value: 'all' },
    { label: t.active, value: 'Active' },
    { label: t.maintenance, value: 'Maintenance' },
    { label: t.deprecated, value: 'Deprecated' },
  ];
  const sortOptions: SortOption[] = [
    { label: t.sortByName, value: 'name' },
    { label: t.sortByCode, value: 'code' },
    { label: t.sortByStatus, value: 'status' },
  ];

  return (
    <PageShell>
      <MasterDetailLayout
        splitterLabel="Resize system suite detail panel"
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
            <SystemSuiteForm
              isOpen={dashboard.isCreateOpen}
              onClose={() => dashboard.setIsCreateOpen(false)}
              onSuccess={dashboard.handleCreateSuccess}
            />
          </>
        }
        master={
          <SystemSuiteListPanel
            systemSuites={dashboard.knownSystemSuites}
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
            onSelectSystemSuite={dashboard.handleSelectSystemSuite}
            criteriaOptions={criteriaOptions}
            filterOptions={filterOptions}
            sortOptions={sortOptions}
          />
        }
        detail={
          <SystemSuiteDetailPanel
            activeSystemSuite={dashboard.activeSystemSuite}
            isLoading={dashboard.isLoadingList}
            onSystemSuiteUpdate={dashboard.patchSystemSuite}
            onEditingChange={dashboard.setIsEditing}
          />
        }
      />
    </PageShell>
  );
}
