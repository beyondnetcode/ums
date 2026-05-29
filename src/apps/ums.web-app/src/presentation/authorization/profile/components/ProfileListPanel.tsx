import React, { useCallback } from 'react';
import { UserCheck, Share2, Info, LayoutList, LayoutGrid, Building2, Shield, Key } from 'lucide-react';
import { type Profile } from '@domain/authorization/schemas/profile.schema';
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

const STATUS_LABEL: Record<string, string> = {
  active: 'Activo',
  inactive: 'Inactivo',
};

const STATUS_COLOR_MAP = {
  active: { bg: 'bg-emerald-500/10', border: 'border-emerald-500/25', text: 'text-emerald-500' },
  inactive: { bg: 'bg-rose-500/10', border: 'border-rose-500/25', text: 'text-rose-500' },
};

interface Props {
  profiles: Profile[];
  selectedId: string;
  isLoading: boolean;
  error: Error | null;
  viewMode: 'list' | 'thumbnail';
  onViewModeChange: (m: 'list' | 'thumbnail') => void;
  queryState: ReturnType<typeof useQueryState<string, string>>;
  paginationState: ReturnType<typeof usePaginationState> & { totalItems: number; totalPages: number };
  onRegisterNew: () => void;
  onSelectProfile: (id: string) => void;
  onOpenGraph: (id: string) => void;
  requiresFilter?: boolean;
}

export const ProfileListPanel: React.FC<Props> = ({
  profiles, selectedId, isLoading, error,
  viewMode, onViewModeChange,
  queryState, paginationState,
  onRegisterNew, onSelectProfile, onOpenGraph,
  requiresFilter = false,
}) => {

  const criteriaOptions: AtomicQueryCriteriaOption[] = [
    { label: 'Por Usuario', value: 'user' },
    { label: 'Por Rol', value: 'role' },
    { label: 'Por Tenant', value: 'tenant' },
    { label: 'Por Sistema', value: 'system' },
  ];

  const filterOptions: AtomicFilterOption[] = [
    { label: 'Todos', value: 'all' },
    { label: 'Activos', value: 'active' },
    { label: 'Inactivos', value: 'inactive' },
  ];

  const sortOptions: AtomicSortOption[] = [
    { label: 'Usuario', value: 'user' },
    { label: 'Rol', value: 'role' },
    { label: 'Tenant', value: 'tenant' },
    { label: 'Sistema', value: 'system' },
    { label: 'Alcance', value: 'scope' },
  ];

  const renderRow = useCallback((prof: Profile) => {
    const isSelected = prof.profileId === selectedId;
    const statusKey = prof.isActive ? 'active' : 'inactive';

    return (
      <EntityRow
        key={prof.profileId}
        selected={isSelected}
        onClick={() => onSelectProfile(prof.profileId)}
        leading={
          <div className={`p-2 rounded-lg shrink-0 transition-colors ${isSelected ? 'bg-m3-primary/15' : 'bg-m3-surface-container/50'}`}>
            <UserCheck className={`w-4 h-4 ${isSelected ? 'text-m3-primary' : 'text-m3-secondary'}`} />
          </div>
        }
        trailingColumns={[
          {
            content: (
              <div className="flex items-center gap-2">
                <span className="text-[10px] font-medium text-m3-secondary/70 bg-m3-surface-container/40 px-2 py-0.5 rounded-full">
                  {prof.permissionCount} permisos
                </span>
                <StatusBadge
                  status={statusKey}
                  label={STATUS_LABEL[statusKey] ?? statusKey}
                  colorMap={STATUS_COLOR_MAP}
                />
                <button
                  type="button"
                  title="Ver Grafo de Autorización"
                  onClick={(e) => {
                    e.stopPropagation();
                    onOpenGraph(prof.profileId);
                  }}
                  className="p-1 rounded-lg border border-m3-outline/20 hover:bg-m3-surface-container/80 transition-colors"
                >
                  <Share2 className="w-3.5 h-3.5 text-m3-primary" />
                </button>
              </div>
            ),
            width: 'w-40',
          },
        ]}
      >
        <div className="flex items-center gap-2">
          <span className="text-sm font-bold text-m3-on-surface">{prof.userEmail}</span>
          <span className="text-[9px] text-m3-secondary/30">·</span>
          <span className="inline-flex items-center gap-1 text-[10px] font-semibold text-m3-primary bg-m3-primary/10 px-1.5 py-0.5 rounded">
            {prof.systemSuiteCode}
          </span>
        </div>
        <div className="flex items-center gap-2 mt-0.5 flex-wrap">
          <span className="inline-flex items-center gap-1 text-[10px] text-m3-secondary/70">
            <Shield className="w-3 h-3 text-m3-secondary/50" />
            <span className="font-medium">{prof.roleCode}</span>
            <span className="text-m3-secondary/50">— {prof.roleName}</span>
          </span>
          <span className="text-[9px] text-m3-secondary/30">·</span>
          <span className="inline-flex items-center gap-1 text-[10px] text-m3-secondary/70">
            <Building2 className="w-3 h-3 text-m3-secondary/50" />
            <span>{prof.tenantCode}</span>
          </span>
          <span className="text-[9px] text-m3-secondary/30">·</span>
          <span className="inline-flex items-center gap-1 text-[10px] text-m3-secondary/70">
            <Key className="w-3 h-3 text-m3-secondary/50" />
            <span>{prof.scope}</span>
          </span>
        </div>
      </EntityRow>
    );
  }, [selectedId, onSelectProfile, onOpenGraph]);

  const renderCard = useCallback((prof: Profile) => {
    const isSelected = prof.profileId === selectedId;
    const statusKey = prof.isActive ? 'active' : 'inactive';

    return (
      <EntityCard
        key={prof.profileId}
        selected={isSelected}
        onClick={() => onSelectProfile(prof.profileId)}
        icon={<UserCheck className="w-5 h-5" />}
        title={prof.userEmail}
        subtitle={
          <div className="flex flex-col gap-0.5">
            <span className="inline-flex items-center gap-1 text-[10px] font-semibold text-m3-primary">
              {prof.systemSuiteCode} — {prof.systemSuiteName}
            </span>
            <span className="text-[10px] text-m3-secondary/60">
              {prof.tenantCode} · {prof.roleCode} · {prof.scope}
            </span>
            <span className="text-[10px] text-m3-secondary/50">
              {prof.permissionCount} permisos
            </span>
          </div>
        }
        badges={
          <div className="flex items-center gap-2">
            <StatusBadge
              status={statusKey}
              label={STATUS_LABEL[statusKey] ?? statusKey}
              colorMap={STATUS_COLOR_MAP}
            />
            <button
              type="button"
              title="Ver Grafo"
              onClick={(e) => {
                e.stopPropagation();
                onOpenGraph(prof.profileId);
              }}
              className="p-1 rounded-lg border border-m3-outline/20 hover:bg-m3-surface-container/80 transition-colors"
            >
              <Share2 className="w-3.5 h-3.5 text-m3-primary" />
            </button>
          </div>
        }
      />
    );
  }, [selectedId, onSelectProfile, onOpenGraph]);

  const pagination = paginationState.totalPages > 0 ? {
    page: paginationState.page,
    pageSize: paginationState.pageSize,
    totalItems: paginationState.totalItems,
    totalPages: paginationState.totalPages,
    onPageChange: paginationState.handlePageChange ?? paginationState.setPage,
    onPageSizeChange: paginationState.handlePageSizeChange,
  } : undefined;

  const totalItems = paginationState.totalItems;
  const startIndex = paginationState.startIndex ?? 0;
  const pageSize = paginationState.pageSize;

  const filterPrompt = requiresFilter ? (
    <div className="flex flex-col items-center justify-center h-full text-center py-16">
      <div className="p-4 rounded-2xl bg-m3-primary/5 border border-m3-primary/10 mb-4">
        <Info className="w-8 h-8 text-m3-primary/60" />
      </div>
      <h3 className="text-sm font-semibold text-m3-on-surface mb-1">Aplica un filtro para cargar datos</h3>
      <p className="text-xs text-m3-secondary/70 max-w-xs">
        Selecciona un estado, ingresa un término de búsqueda o cambia el criterio para ver los perfiles.
      </p>
    </div>
  ) : null;

  const footerTelemetry = (
    <div className="flex items-center gap-3">
      <div className="flex items-center gap-1.5">
        <span className="h-2 w-2 rounded-full bg-m3-primary animate-pulse" />
        <span className="text-xs font-medium text-m3-secondary/80">
          Mostrando {totalItems === 0 ? 0 : startIndex + 1}-{Math.min(startIndex + pageSize, totalItems)} de {totalItems} perfiles
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
        title="Perfiles"
        subtitle="Mantenimiento y consulta de perfiles de autorización."
        onRegisterNew={onRegisterNew}
        registerLabel="Nuevo Perfil"
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
              viewMode={viewMode}
              onViewModeChange={onViewModeChange}
            />
          </>
        }
        content={
          requiresFilter ? filterPrompt : (
            <DataList
              isLoading={isLoading}
              isEmpty={totalItems === 0}
              emptyTitle="Sin perfiles de autorización"
              emptyLabel="Registre el primer perfil efectivo para iniciar la asignación de accesos."
              viewMode={viewMode}
              renderList={() => (
                <div className="flex flex-col gap-0.5">
                  {profiles.map(renderRow)}
                </div>
              )}
              renderThumbnail={() => (
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
                  {profiles.map(renderCard)}
                </div>
              )}
              pagination={pagination}
              footerElement={footerTelemetry}
            />
          )
        }
      />
    </div>
  );
};
