/**
 * PermissionTemplateDashboardScreen
 *
 * Master-detail layout for the Permission Template aggregate:
 *  • Left panel  — paginated list with filter/sort controls
 *  • Right panel — detail view (overview + items) for the selected template
 *  • Modal       — create new template form
 */
import React from 'react';
import { usePermissionTemplateDashboard } from '@app/authorization/hooks/use-permission-template-dashboard';
import { PermissionTemplateListPanel } from '../components/PermissionTemplateListPanel';
import { PermissionTemplateDetailPanel } from '../components/PermissionTemplateDetailPanel';
import { PermissionTemplateForm } from '../components/PermissionTemplateForm';
import { PageShell } from '@shared/layouts/PageShell';
import { MasterDetailLayout } from '@shared/layouts/MasterDetailLayout';

export default function PermissionTemplateDashboardScreen(): React.JSX.Element {
  const d = usePermissionTemplateDashboard();

  const startIndex = d.totalItems > 0 ? (d.page - 1) * 20 + 1 : 0;

  return (
    <PageShell>
      <PermissionTemplateForm
        isOpen={d.isCreateOpen}
        onClose={() => d.setIsCreateOpen(false)}
        onSuccess={(templateId) => {
          d.handleCreateSuccess();
          d.setSelectedId(templateId);
        }}
      />

      <MasterDetailLayout
        splitterLabel="Resize permission template detail panel"
        master={
          <PermissionTemplateListPanel
            templates={d.knownTemplates}
            selectedId={d.selectedId}
            isLoading={d.isLoadingList}
            error={d.listError}
            viewMode={d.viewMode}
            onViewModeChange={d.setViewMode}
            searchValue={d.searchValue}
            onSearchValueChange={d.setSearchValue}
            onSearchSubmit={d.handleQuerySubmit}
            onRegisterNew={() => d.setIsCreateOpen(true)}
            activeFilter={d.activeFilter}
            onFilterChange={(val) => { d.setActiveFilter(val); d.setPage(1); }}
            sortBy={d.sortBy}
            onSortByChange={d.setSortBy}
            sortOrder={d.sortOrder}
            onSortOrderToggle={() => d.setSortOrder((o) => (o === 'asc' ? 'desc' : 'asc'))}
            page={d.page}
            pageSize={20}
            totalItems={d.totalItems}
            totalPages={d.totalPages}
            startIndex={startIndex}
            appliedTerm={d.appliedSearch}
            onPageChange={d.setPage}
            onResetQuery={d.handleResetQuery}
            onSelectTemplate={d.handleSelect}
          />
        }
        detail={
          <PermissionTemplateDetailPanel
            template={d.activeTemplate}
            isLoading={d.isLoadingDetail}
            onDeleted={() => d.setSelectedId('')}
          />
        }
      />
    </PageShell>
  );
}
