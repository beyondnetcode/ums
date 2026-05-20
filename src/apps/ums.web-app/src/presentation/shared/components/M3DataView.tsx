import React from 'react';
import {
  Search,
  LayoutList,
  LayoutGrid,
  ChevronLeft,
  ChevronRight,
  Plus,
  ArrowUpDown,
  Filter,
  Database,
  Info
} from 'lucide-react';
import { M3Card } from './M3Card';
import { M3Button } from './M3Button';
import { M3TextField } from './M3TextField';

export interface SortOption { label: string; value: string; }
export interface FilterOption { label: string; value: string; }
export interface QueryCriteriaOption { label: string; value: string; }
export interface PaginationData {
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

interface M3DataViewProps {
  title: string;
  subtitle?: string;

  searchPlaceholder?: string;
  searchCriteria?: QueryCriteriaOption[];
  activeCriteria?: string;
  onCriteriaChange?: (criteria: string) => void;
  searchValue: string;
  onSearchValueChange: (value: string) => void;
  onSearchSubmit: (e: React.FormEvent) => void;

  onRegisterNew?: () => void;
  registerLabel?: string;

  viewMode: 'list' | 'thumbnail';
  onViewModeChange: (mode: 'list' | 'thumbnail') => void;

  sortOptions?: SortOption[];
  sortBy?: string;
  onSortByChange?: (val: string) => void;
  sortOrder?: 'asc' | 'desc';
  onSortOrderToggle?: () => void;

  filterOptions?: FilterOption[];
  activeFilter?: string;
  onFilterChange?: (val: string) => void;

  isLoading?: boolean;
  isEmpty?: boolean;
  emptyLabel?: string;
  emptyTitle?: string;
  loadingLabel?: string;
  criteriaLabel?: string;
  searchTermLabel?: string;
  searchButtonLabel?: string;

  renderList: () => React.ReactNode;
  renderThumbnail: () => React.ReactNode;

  pagination?: PaginationData;
  telemetryInfo?: React.ReactNode;
}

export const M3DataView: React.FC<M3DataViewProps> = ({
  title,
  subtitle,
  searchPlaceholder = 'Search...',
  searchCriteria,
  activeCriteria,
  onCriteriaChange,
  searchValue,
  onSearchValueChange,
  onSearchSubmit,
  onRegisterNew,
  registerLabel = 'New',
  viewMode,
  onViewModeChange,
  sortOptions,
  sortBy,
  onSortByChange,
  sortOrder,
  onSortOrderToggle,
  filterOptions,
  activeFilter,
  onFilterChange,
  isLoading = false,
  isEmpty = false,
  emptyLabel = 'No matching records found.',
  emptyTitle = 'No results found',
  loadingLabel = 'Loading...',
  criteriaLabel = 'Criteria',
  searchTermLabel = 'Search term',
  searchButtonLabel = 'Search',
  renderList,
  renderThumbnail,
  pagination,
  telemetryInfo,
}) => {
  return (
    <div className="flex flex-col gap-4 select-none h-full">

      {/* 1 — Header card */}
      <M3Card variant="elevated" className="p-5 border border-m3-outline/25 bg-m3-surface-container/20 shadow-sm">
        <div className="flex flex-col md:flex-row justify-between md:items-center gap-4">
          <div className="space-y-0.5">
            <h2 className="text-base font-semibold text-m3-on-surface flex items-center gap-2">
              <Database className="w-5 h-5 text-m3-primary flex-shrink-0" />
              {title}
            </h2>
            {subtitle && (
              <p className="text-xs text-m3-secondary font-normal max-w-xl pl-7">
                {subtitle}
              </p>
            )}
          </div>
          {onRegisterNew && (
            <M3Button
              variant="filled"
              onClick={onRegisterNew}
              className="self-start md:self-auto shadow-md flex items-center gap-2"
            >
              <Plus className="w-4 h-4" /> {registerLabel}
            </M3Button>
          )}
        </div>

        {/* Query form */}
        <form onSubmit={onSearchSubmit} className="mt-5 grid grid-cols-1 sm:grid-cols-12 gap-3 items-start">
          {searchCriteria && onCriteriaChange && activeCriteria && (
            <div className="sm:col-span-3 relative flex flex-col w-full">
              <label className="block text-xs font-medium text-m3-secondary mb-1 ml-1">
                {criteriaLabel}
              </label>
              <select
                value={activeCriteria}
                onChange={(e) => onCriteriaChange(e.target.value)}
                className="w-full h-14 px-4 text-sm rounded-[4px] border-[1.5px] border-m3-outline bg-m3-surface-container/30 dark:bg-m3-surface-container/20 text-m3-on-surface focus:outline-none focus:border-m3-primary transition-colors cursor-pointer"
              >
                {searchCriteria.map((c) => (
                  <option key={c.value} value={c.value}>{c.label}</option>
                ))}
              </select>
            </div>
          )}

          <div className={`${searchCriteria ? 'sm:col-span-6' : 'sm:col-span-9'}`}>
            <div className="mt-6">
              <M3TextField
                label={searchTermLabel}
                value={searchValue}
                onChange={(e) => onSearchValueChange(e.target.value)}
                placeholder={searchPlaceholder}
                className="mb-0"
              />
            </div>
          </div>

          <div className="sm:col-span-3 mt-7">
            <M3Button
              variant="outlined"
              type="submit"
              className="w-full h-14 rounded-[4px] text-sm font-medium"
            >
              <Search className="w-4 h-4 mr-2" /> {searchButtonLabel}
            </M3Button>
          </div>
        </form>
      </M3Card>

      {/* 2 — Control row */}
      <div className="flex flex-col sm:flex-row justify-between items-stretch sm:items-center gap-3 bg-m3-surface-container/30 border border-m3-outline/20 rounded-xl p-3">
        <div className="flex flex-wrap items-center gap-3">
          {filterOptions && onFilterChange && activeFilter !== undefined && (
            <div className="flex items-center gap-2">
              <div className="p-1.5 bg-m3-primary/10 rounded-lg text-m3-primary border border-m3-primary/10">
                <Filter className="w-3.5 h-3.5" />
              </div>
              <select
                value={activeFilter}
                onChange={(e) => onFilterChange(e.target.value)}
                className="h-9 px-3 rounded-lg border border-m3-outline bg-m3-surface text-xs font-medium text-m3-secondary focus:outline-none focus:border-m3-primary transition-colors cursor-pointer"
              >
                {filterOptions.map((f) => (
                  <option key={f.value} value={f.value}>{f.label}</option>
                ))}
              </select>
            </div>
          )}

          {sortOptions && onSortByChange && sortBy !== undefined && (
            <div className="flex items-center gap-2">
              <div className="p-1.5 bg-m3-primary/10 rounded-lg text-m3-primary border border-m3-primary/10">
                <ArrowUpDown className="w-3.5 h-3.5" />
              </div>
              <select
                value={sortBy}
                onChange={(e) => onSortByChange(e.target.value)}
                className="h-9 px-3 rounded-lg border border-m3-outline bg-m3-surface text-xs font-medium text-m3-secondary focus:outline-none focus:border-m3-primary transition-colors cursor-pointer"
              >
                {sortOptions.map((s) => (
                  <option key={s.value} value={s.value}>{s.label}</option>
                ))}
              </select>

              {onSortOrderToggle && sortOrder && (
                <button
                  type="button"
                  onClick={onSortOrderToggle}
                  className="h-9 w-9 flex items-center justify-center rounded-lg border border-m3-outline bg-m3-surface hover:bg-m3-primary/10 hover:border-m3-primary/30 transition-all text-m3-secondary"
                  title={sortOrder === 'asc' ? 'Ascending' : 'Descending'}
                >
                  <ArrowUpDown className={`w-3.5 h-3.5 transition-transform duration-200 ${sortOrder === 'desc' ? 'rotate-180 text-m3-primary' : ''}`} />
                </button>
              )}
            </div>
          )}
        </div>

        {/* View mode segmented control */}
        <div className="flex items-center gap-1 bg-m3-surface-container rounded-lg p-1 border border-m3-outline/25 self-end sm:self-auto">
          <button
            type="button"
            onClick={() => onViewModeChange('list')}
            className={`p-2 rounded-md transition-all duration-200 flex items-center justify-center ${
              viewMode === 'list'
                ? 'bg-m3-primary text-m3-on-primary shadow-sm'
                : 'text-m3-secondary hover:bg-m3-primary/10'
            }`}
          >
            <LayoutList className="w-4 h-4" />
          </button>
          <button
            type="button"
            onClick={() => onViewModeChange('thumbnail')}
            className={`p-2 rounded-md transition-all duration-200 flex items-center justify-center ${
              viewMode === 'thumbnail'
                ? 'bg-m3-primary text-m3-on-primary shadow-sm'
                : 'text-m3-secondary hover:bg-m3-primary/10'
            }`}
          >
            <LayoutGrid className="w-4 h-4" />
          </button>
        </div>
      </div>

      {/* 3 — Content */}
      <div className="flex-1 relative overflow-auto">
        {isLoading ? (
          <div className="py-24 text-center text-sm text-m3-secondary">
            <svg className="animate-spin h-8 w-8 text-m3-primary mx-auto mb-3" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
            </svg>
            {loadingLabel}
          </div>
        ) : isEmpty ? (
          <M3Card variant="elevated" className="p-8 text-center text-sm text-m3-secondary border border-m3-outline/20 bg-m3-surface-container/10">
            <div className="w-12 h-12 bg-m3-primary/10 border border-m3-primary/20 text-m3-primary rounded-full flex items-center justify-center mx-auto mb-3.5">
              <Info className="w-6 h-6" />
            </div>
            <h4 className="text-sm font-semibold text-m3-on-surface">{emptyTitle}</h4>
            <p className="mt-1 text-xs font-normal leading-relaxed max-w-sm mx-auto text-m3-secondary">
              {emptyLabel}
            </p>
          </M3Card>
        ) : (
          <div className="animate-fadeIn">
            {viewMode === 'list' ? renderList() : renderThumbnail()}
          </div>
        )}
      </div>

      {/* 4 — Footer */}
      {(pagination || telemetryInfo) && (
        <div className="flex flex-col sm:flex-row justify-between items-center gap-3 bg-m3-surface-container/20 border-t border-m3-outline/20 p-3 rounded-xl text-xs font-medium text-m3-secondary">
          <div className="flex items-center gap-2">
            {telemetryInfo ?? (
              pagination && (
                <div className="flex items-center gap-1.5">
                  <span className="h-2 w-2 rounded-full bg-emerald-500 animate-pulse" />
                  <span className="text-xs text-m3-secondary/70">
                    {pagination.totalItems} records · {pagination.pageSize} per page
                  </span>
                </div>
              )
            )}
          </div>

          {pagination && pagination.totalPages > 1 && (
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={() => pagination.onPageChange(Math.max(1, pagination.page - 1))}
                disabled={pagination.page === 1}
                className="p-1.5 rounded-lg border border-m3-outline bg-m3-surface text-m3-secondary disabled:opacity-40 disabled:cursor-not-allowed hover:bg-m3-primary/10 hover:text-m3-primary transition-all"
              >
                <ChevronLeft className="w-4 h-4" />
              </button>

              <div className="flex items-center gap-1 select-none">
                {Array.from({ length: pagination.totalPages }, (_, i) => i + 1).map((p) => {
                  const isActive = p === pagination.page;
                  return (
                    <button
                      key={p}
                      type="button"
                      onClick={() => pagination.onPageChange(p)}
                      className={`h-7 min-w-7 px-2 text-xs font-medium rounded-lg flex items-center justify-center transition-all ${
                        isActive
                          ? 'bg-m3-primary text-m3-on-primary shadow-sm'
                          : 'border border-m3-outline bg-m3-surface hover:bg-m3-primary/10 text-m3-secondary'
                      }`}
                    >
                      {p}
                    </button>
                  );
                })}
              </div>

              <button
                type="button"
                onClick={() => pagination.onPageChange(Math.min(pagination.totalPages, pagination.page + 1))}
                disabled={pagination.page === pagination.totalPages}
                className="p-1.5 rounded-lg border border-m3-outline bg-m3-surface text-m3-secondary disabled:opacity-40 disabled:cursor-not-allowed hover:bg-m3-primary/10 hover:text-m3-primary transition-all"
              >
                <ChevronRight className="w-4 h-4" />
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
};
