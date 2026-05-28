import React, { useCallback } from 'react';
import { Flag } from 'lucide-react';
import { type FeatureFlag } from '@domain/configuration/models/feature-flag.model';
import { StatusBadge } from '@shared/components/StatusBadge';
import { M3DataView, FilterOption, SortOption } from '@shared/components/M3DataView';
import { EntityRow } from '@shared/components/EntityRow';
import { EntityCard } from '@shared/components/EntityCard';
import { ApiErrorBanner } from '@shared/components/ApiErrorBanner';
import { CodeBadge } from '@shared/components/CodeBadge';
import {
  FLAG_TYPE_LABELS,
  FLAG_STATUS_LABELS,
} from '@domain/configuration/constants/feature-flag.constants';

const STATUS_LABEL: Record<string, string> = {
  Inactive: 'Inactivo',
  Active:   'Activo',
  Archived: 'Archivado',
};

const STATUS_COLOR_MAP = {
  Active:   { bg: 'bg-emerald-500/10', border: 'border-emerald-500/25', text: 'text-emerald-500' },
  Inactive: { bg: 'bg-amber-500/10',   border: 'border-amber-500/25',   text: 'text-amber-500' },
  Archived: { bg: 'bg-rose-500/10',    border: 'border-rose-500/25',    text: 'text-rose-500' },
};

const TYPE_COLOR: Record<string, string> = {
  Boolean:    'bg-blue-500/10 text-blue-500',
  Variant:    'bg-purple-500/10 text-purple-500',
  Percentage: 'bg-emerald-500/10 text-emerald-500',
};

interface Props {
  flags:       FeatureFlag[];
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
  onSelectFlag: (id: string) => void;
}

export const FeatureFlagListPanel: React.FC<Props> = ({
  flags, selectedId, isLoading, error,
  viewMode, onViewModeChange,
  searchValue, onSearchValueChange, onSearchSubmit,
  onRegisterNew, activeFilter, onFilterChange,
  sortBy, onSortByChange, sortOrder, onSortOrderToggle,
  page, pageSize, totalItems, totalPages, startIndex,
  appliedTerm, onPageChange, onResetQuery, onSelectFlag,
}) => {
  const filterOptions: FilterOption[] = [
    { label: 'Todos', value: 'all' },
    { label: 'Activo', value: 'Active' },
    { label: 'Inactivo', value: 'Inactive' },
    { label: 'Archivado', value: 'Archived' },
  ];

  const sortOptions: SortOption[] = [
    { label: 'Código', value: 'flagCode' },
    { label: 'Tipo', value: 'flagType' },
    { label: 'Estado', value: 'status' },
  ];

  const renderRow = useCallback((flag: FeatureFlag) => {
    const isSelected = flag.featureFlagId === selectedId;
    return (
      <EntityRow
        key={flag.featureFlagId}
        selected={isSelected}
        onClick={() => onSelectFlag(flag.featureFlagId)}
        leading={
          <div className={`p-2 rounded-lg shrink-0 transition-colors ${isSelected ? 'bg-m3-primary/15' : 'bg-m3-surface-container/50'}`}>
            <Flag className={`w-4 h-4 ${isSelected ? 'text-m3-primary' : 'text-m3-secondary'}`} />
          </div>
        }
        trailingColumns={[
          {
            content: (
              <span className={`text-[10px] font-bold uppercase px-1.5 py-0.5 rounded ${TYPE_COLOR[flag.flagType] ?? 'bg-m3-surface-variant'}`}>
                {FLAG_TYPE_LABELS[flag.flagType] ?? flag.flagType}
              </span>
            ),
            width: 'w-24',
          },
          {
            content: (
              <StatusBadge
                status={flag.status}
                label={STATUS_LABEL[flag.status] ?? flag.status}
                colorMap={STATUS_COLOR_MAP}
              />
            ),
            width: 'w-24',
          },
        ]}
      >
        <span className="text-sm font-bold text-m3-on-surface">{flag.flagCode}</span>
        <div className="flex items-center gap-2 mt-0.5 flex-wrap">
          <span className="inline-flex items-center gap-1 text-[10px] text-m3-secondary/70">
            <span className="text-[9px] font-semibold uppercase tracking-wide text-m3-secondary/50">Suite</span>
            <span>{flag.systemSuiteId.slice(0, 8)}…</span>
          </span>
          {flag.rolloutPercentage != null && (
            <>
              <span className="text-[9px] text-m3-secondary/30">·</span>
              <span className="text-[10px] text-m3-secondary/70">
                Rollout: {flag.rolloutPercentage}%
              </span>
            </>
          )}
          {flag.criteria.length > 0 && (
            <>
              <span className="text-[9px] text-m3-secondary/30">·</span>
              <span className="text-[10px] text-m3-secondary/70">
                {flag.criteria.length} criterio{flag.criteria.length > 1 ? 's' : ''}
              </span>
            </>
          )}
        </div>
      </EntityRow>
    );
  }, [selectedId, onSelectFlag]);

  const renderCard = useCallback((flag: FeatureFlag) => {
    const isSelected = flag.featureFlagId === selectedId;
    return (
      <EntityCard
        key={flag.featureFlagId}
        selected={isSelected}
        onClick={() => onSelectFlag(flag.featureFlagId)}
        icon={<Flag className="w-5 h-5" />}
        title={flag.flagCode}
        subtitle={
          <span className="text-[10px] text-m3-secondary/60">
            {FLAG_TYPE_LABELS[flag.flagType]} · Suite {flag.systemSuiteId.slice(0, 8)}…
          </span>
        }
        badges={
          <div className="flex items-center gap-1">
            {flag.rolloutPercentage != null && (
              <CodeBadge code={`${flag.rolloutPercentage}%`} size="xs" />
            )}
            <StatusBadge
              status={flag.status}
              label={STATUS_LABEL[flag.status] ?? flag.status}
              colorMap={STATUS_COLOR_MAP}
            />
          </div>
        }
      />
    );
  }, [selectedId, onSelectFlag]);

  const pagination = totalItems > 0 ? {
    page, pageSize, totalItems, totalPages, startIndex,
    onPageChange,
  } : undefined;

  return (
    <div className="h-full flex flex-col">
      {error && <ApiErrorBanner error={error} className="mb-2" />}
      <M3DataView
        isLoading={isLoading}
        items={flags}
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
        emptyTitle="Sin feature flags"
        emptyMessage="Cree el primer feature flag para controlar funcionalidades."
        renderList={() => (
          <div className="flex flex-col gap-0.5">
            {flags.map(renderRow)}
          </div>
        )}
        renderThumbnail={() => (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
            {flags.map(renderCard)}
          </div>
        )}
      />
    </div>
  );
};
