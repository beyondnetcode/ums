import React, { useCallback } from 'react';
import { ShieldCheck, Info, LayoutList, LayoutGrid } from 'lucide-react';
import { type PermissionTemplate } from '@domain/authorization/models/permission-template.model';
import { StatusBadge } from '@shared/components/StatusBadge';
import {
  DataViewShell,
  SearchBar,
  FilterPanel,
  DataList,
  AtomicFilterOption,
  AtomicSortOption,
  AtomicQueryCriteriaOption,
} from '@shared/components';
import { useQueryState } from '@app/shared/hooks/use-query-state';
import { usePaginationState } from '@app/shared/hooks/use-pagination-state';
import { EntityRow } from '@shared/components/EntityRow';
import { EntityCard } from '@shared/components/EntityCard';
import { ApiErrorBanner } from '@shared/components/ApiErrorBanner';

// ─── Status helpers ───────────────────────────────────────────────────────────

const STATUS_LABEL: Record<string, string> = {
  Draft:      'Borrador',
  Published:  'Publicada',
  Deprecated: 'Descontinuada',
};

const STATUS_COLOR_MAP = {
  Published:  { bg: 'bg-emerald-500/10', border: 'border-emerald-500/25', text: 'text-emerald-500' },
  Deprecated: { bg: 'bg-rose-500/10',    border: 'border-rose-500/25',    text: 'text-rose-500' },
  Draft:      { bg: 'bg-amber-500/10',   border: 'border-amber-500/25',   text: 'text-amber-500' },
};

// ─── Props ────────────────────────────────────────────────────────────────────

interface Props {
  templates:   PermissionTemplate[];
  selectedId:  string;
  isLoading:   boolean;
  error:       Error | null;
  viewMode:    'list' | 'thumbnail';
  onViewModeChange: (m: 'list' | 'thumbnail') => void;
  queryState: ReturnType<typeof useQueryState<string, string>>;
  paginationState: ReturnType<typeof usePaginationState> & { totalItems: number; totalPages: number };
  onRegisterNew: () => void;
  onSelectTemplate: (id: string) => void;
}

export const PermissionTemplateListPanel: React.FC<Props> = ({
  templates, selectedId, isLoading, error,
  viewMode, onViewModeChange,
  templates, selectedId, isLoading, error,
  viewMode, onViewModeChange,
  queryState, paginationState,
  onRegisterNew, onSelectTemplate,
}) => {

  const criteriaOptions: AtomicQueryCriteriaOption[] = [
    { label: 'Versión', value: 'version' },
  ];

  const filterOptions: AtomicFilterOption[] = [
    { label: 'Todos', value: 'all' },
    { label: 'Borrador', value: 'Draft' },
    { label: 'Publicada', value: 'Published' },
    { label: 'Descontinuada', value: 'Deprecated' },
  ];

  const sortOptions: AtomicSortOption[] = [
    { label: 'Versión', value: 'version' },
    { label: 'Estado', value: 'status' },
  ];

  const renderRow = useCallback((tpl: PermissionTemplate) => {
    const isSelected = tpl.templateId === selectedId;
    return (
      <EntityRow
        key={tpl.templateId}
        selected={isSelected}
        onClick={() => onSelectTemplate(tpl.templateId)}
        leading={
          <div className={`p-2 rounded-lg shrink-0 transition-colors ${isSelected ? 'bg-m3-primary/15' : 'bg-m3-surface-container/50'}`}>
            <ShieldCheck className={`w-4 h-4 ${isSelected ? 'text-m3-primary' : 'text-m3-secondary'}`} />
          </div>
        }
        trailingColumns={[
          {
            content: (
              <StatusBadge
                status={tpl.status}
                label={STATUS_LABEL[tpl.status] ?? tpl.status}
                colorMap={STATUS_COLOR_MAP}
              />
            ),
            width: 'w-24',
          },
        ]}
      >
        <span className="text-sm font-bold text-m3-on-surface">v{tpl.version}</span>
        <div className="flex items-center gap-2 mt-0.5 flex-wrap">
          <span className="inline-flex items-center gap-1 text-[10px] text-m3-secondary/70">
            <span className="text-[9px] font-semibold uppercase tracking-wide text-m3-secondary/50">Rol</span>
            <span>{tpl.roleName}</span>
          </span>
          <span className="text-[9px] text-m3-secondary/30">·</span>
          <span className="inline-flex items-center gap-1 text-[10px] text-m3-secondary/70">
            <span className="text-[9px] font-semibold uppercase tracking-wide text-m3-secondary/50">Suite</span>
            <span>{tpl.systemSuiteName}</span>
          </span>
        </div>
      </EntityRow>
    );
  }, [selectedId, onSelectTemplate]);

  const renderCard = useCallback((tpl: PermissionTemplate) => {
    const isSelected = tpl.templateId === selectedId;
    return (
      <EntityCard
        key={tpl.templateId}
        selected={isSelected}
        onClick={() => onSelectTemplate(tpl.templateId)}
        icon={<ShieldCheck className="w-5 h-5" />}
        title={`v${tpl.version}`}
        subtitle={
          <span className="text-[10px] text-m3-secondary/60">
            {tpl.roleName} · {tpl.systemSuiteName}
          </span>
        }
        badges={
          <StatusBadge
            status={tpl.status}
            label={STATUS_LABEL[tpl.status] ?? tpl.status}
            colorMap={STATUS_COLOR_MAP}
          />
        }
      />
    );
  }, [selectedId, onSelectTemplate]);

  const pagination = paginationState.totalPages > 0 ? {
    page: paginationState.page,
    pageSize: paginationState.pageSize,
    totalItems: paginationState.totalItems,
    totalPages: paginationState.totalPages,
    onPageChange: paginationState.handlePageChange ?? paginationState.setPage,
  } : undefined;

  const totalItems = paginationState.totalItems;
  const startIndex = paginationState.startIndex ?? 0;
  const pageSize = paginationState.pageSize;

  const footerTelemetry = (
    <div className="flex items-center gap-3">
      <div className="flex items-center gap-1.5">
        <span className="h-2 w-2 rounded-full bg-m3-primary animate-pulse" />
        <span className="text-xs font-medium text-m3-secondary/80">
          Mostrando {totalItems === 0 ? 0 : startIndex + 1}-{Math.min(startIndex + pageSize, totalItems)} de {totalItems} plantillas
        </span>
      </div>
      {queryState.appliedQuery.term.trim() && (
        <button onClick={queryState.handleResetQuery} className="text-xs font-medium text-rose-500 hover:underline flex items-center gap-1">
          <Info className="w-3 h-3" /> Limpiar filtros
        </button>
      )}
    </div>
  );

  return (
    <div className="h-full flex flex-col">
      {error && <ApiErrorBanner error={error} className="mb-2" />}
      <DataViewShell
        title="Plantillas"
        subtitle="Mantenimiento de plantillas de permisos."
        onRegisterNew={onRegisterNew}
        registerLabel="Nueva Plantilla"
        controls={
          <>
            <SearchBar
              criteriaOptions={criteriaOptions}
              activeCriteria={queryState.searchCriteria}
              onCriteriaChange={queryState.setSearchCriteria}
              searchValue={queryState.searchValue}
              onSearchValueChange={queryState.setSearchValue}
              onSubmit={queryState.handleQuerySubmit}
              criteriaLabel="Criterio"
              searchTermLabel="Término"
              searchButtonLabel="Buscar"
            />
            <FilterPanel
              filterOptions={filterOptions}
              activeFilter={queryState.activeFilter}
              onFilterChange={queryState.setActiveFilter}
              sortOptions={sortOptions}
              sortBy={queryState.sortBy}
              onSortByChange={queryState.setSortBy}
              sortOrder={queryState.sortOrder}
              onSortOrderToggle={queryState.toggleSortOrder}
              viewModeOptions={[
                { value: 'list', label: <LayoutList className="w-4 h-4" /> },
                { value: 'thumbnail', label: <LayoutGrid className="w-4 h-4" /> }
              ]}
              viewMode={viewMode}
              onViewModeChange={onViewModeChange}
            />
          </>
        }
        content={
          <DataList
            isLoading={isLoading}
            isEmpty={totalItems === 0}
            emptyTitle="Sin plantillas de permisos"
            emptyLabel="Cree la primera plantilla para este contexto de autorización."
            viewMode={viewMode}
            renderList={() => (
              <div className="flex flex-col gap-0.5">
                {templates.map(renderRow)}
              </div>
            )}
            renderThumbnail={() => (
              <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                {templates.map(renderCard)}
              </div>
            )}
            pagination={pagination}
            footerElement={footerTelemetry}
          />
        }
      />
    </div>
  );
};
