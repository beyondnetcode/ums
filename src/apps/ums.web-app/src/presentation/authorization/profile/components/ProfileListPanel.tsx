import React, { useCallback } from 'react';
import { UserCheck, Share2 } from 'lucide-react';
import { type Profile } from '@domain/authorization/schemas/profile.schema';
import { StatusBadge } from '@shared/components/StatusBadge';
import { M3DataView, FilterOption, SortOption } from '@shared/components/M3DataView';
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
  searchValue: string;
  onSearchValueChange: (v: string) => void;
  onSearchSubmit: (e: React.FormEvent) => void;
  onRegisterNew: () => void;
  activeFilter: string;
  onFilterChange: (v: string) => void;
  sortBy: string;
  onSortByChange: (v: string) => void;
  sortOrder: 'asc' | 'desc';
  onSortOrderToggle: () => void;
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  startIndex: number;
  appliedTerm: string;
  onPageChange: (p: number) => void;
  onResetQuery: () => void;
  onSelectProfile: (id: string) => void;
  onOpenGraph: (id: string) => void;
}

export const ProfileListPanel: React.FC<Props> = ({
  profiles, selectedId, isLoading, error,
  viewMode, onViewModeChange,
  searchValue, onSearchValueChange, onSearchSubmit,
  onRegisterNew, activeFilter, onFilterChange,
  sortBy, onSortByChange, sortOrder, onSortOrderToggle,
  page, pageSize, totalItems, totalPages, startIndex,
  appliedTerm, onPageChange, onResetQuery, onSelectProfile, onOpenGraph,
}) => {

  const filterOptions: FilterOption[] = [
    { label: 'Todos', value: 'all' },
    { label: 'Activos', value: 'active' },
    { label: 'Inactivos', value: 'inactive' },
  ];

  const sortOptions: SortOption[] = [
    { label: 'Usuario', value: 'userId' },
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
            width: 'w-32',
          },
        ]}
      >
        <span className="text-sm font-bold text-m3-on-surface">ID Usuario: {prof.userId.slice(0, 8)}...</span>
        <div className="flex items-center gap-2 mt-0.5 flex-wrap">
          <span className="inline-flex items-center gap-1 text-[10px] text-m3-secondary/70">
            <span className="text-[9px] font-semibold uppercase tracking-wide text-m3-secondary/50">Alcance</span>
            <span>{prof.scope}</span>
          </span>
          <span className="text-[9px] text-m3-secondary/30">·</span>
          <span className="inline-flex items-center gap-1 text-[10px] text-m3-secondary/70">
            <span className="text-[9px] font-semibold uppercase tracking-wide text-m3-secondary/50">Rol</span>
            <span>{prof.roleId.slice(0, 8)}...</span>
          </span>
          <span className="text-[9px] text-m3-secondary/30">·</span>
          <span className="inline-flex items-center gap-1 text-[10px] text-m3-secondary/70">
            <span className="text-[9px] font-semibold uppercase tracking-wide text-m3-secondary/50">Tenant</span>
            <span>{prof.tenantId.slice(0, 8)}...</span>
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
        title={`Usuario: ${prof.userId.slice(0, 8)}...`}
        subtitle={
          <span className="text-[10px] text-m3-secondary/60">
            Alcance: {prof.scope} · Rol: {prof.roleId.slice(0, 8)}...
          </span>
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

  const pagination = totalItems > 0 ? {
    page, pageSize, totalItems, totalPages, startIndex,
    onPageChange,
  } : undefined;

  return (
    <div className="h-full flex flex-col">
      {error && <ApiErrorBanner error={error} className="mb-2" />}
      <M3DataView
        isLoading={isLoading}
        items={profiles}
        viewMode={viewMode}
        onViewModeChange={onViewModeChange}
        searchValue={searchValue}
        onSearchValueChange={onSearchValueChange}
        onSearchSubmit={onSearchSubmit}
        onRegisterNew={onRegisterNew}
        filterOptions={filterOptions}
        activeFilter={activeFilter}
        onFilterChange={onFilterChange}
        sortOptions={sortOptions}
        sortBy={sortBy}
        onSortByChange={onSortByChange}
        sortOrder={sortOrder}
        onSortOrderToggle={onSortOrderToggle}
        appliedTerm={appliedTerm}
        onResetQuery={onResetQuery}
        pagination={pagination}
        emptyTitle="Sin perfiles de autorización"
        emptyMessage="Registre el primer perfil efectivo para iniciar la asignación de accesos."
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
      />
    </div>
  );
};
