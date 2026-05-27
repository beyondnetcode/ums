import React, { useCallback } from 'react';
import { ShieldCheck } from 'lucide-react';
import { type PermissionTemplate } from '@domain/authorization/models/permission-template.model';
import { StatusBadge } from '@shared/components/StatusBadge';
import { M3DataView, FilterOption, SortOption } from '@shared/components/M3DataView';
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
  onSelectTemplate: (id: string) => void;
}

export const PermissionTemplateListPanel: React.FC<Props> = ({
  templates, selectedId, isLoading, error,
  viewMode, onViewModeChange,
  searchValue, onSearchValueChange, onSearchSubmit,
  onRegisterNew, activeFilter, onFilterChange,
  sortBy, onSortByChange, sortOrder, onSortOrderToggle,
  page, pageSize, totalItems, totalPages, startIndex,
  appliedTerm, onPageChange, onResetQuery, onSelectTemplate,
}) => {

  const filterOptions: FilterOption[] = [
    { label: 'Todos', value: 'all' },
    { label: 'Borrador', value: 'Draft' },
    { label: 'Publicada', value: 'Published' },
    { label: 'Descontinuada', value: 'Deprecated' },
  ];

  const sortOptions: SortOption[] = [
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

  const pagination = totalItems > 0 ? {
    page, pageSize, totalItems, totalPages, startIndex,
    onPageChange,
  } : undefined;

  return (
    <div className="h-full flex flex-col">
      {error && <ApiErrorBanner error={error} className="mb-2" />}
      <M3DataView
        isLoading={isLoading}
        items={templates}
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
        emptyTitle="Sin plantillas de permisos"
        emptyMessage="Cree la primera plantilla para este contexto de autorización."
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
      />
    </div>
  );
};
