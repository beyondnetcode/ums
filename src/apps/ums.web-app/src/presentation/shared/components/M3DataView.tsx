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

export interface SortOption {
  label: string;
  value: string;
}

export interface FilterOption {
  label: string;
  value: string;
}

export interface QueryCriteriaOption {
  label: string;
  value: string;
}

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
  
  // Search & Query Criteria Props
  searchPlaceholder?: string;
  searchCriteria?: QueryCriteriaOption[];
  activeCriteria?: string;
  onCriteriaChange?: (criteria: string) => void;
  searchValue: string;
  onSearchValueChange: (value: string) => void;
  onSearchSubmit: (e: React.FormEvent) => void;
  
  // Action/Register Button
  onRegisterNew?: () => void;
  registerLabel?: string;

  // View Mode Control
  viewMode: 'list' | 'thumbnail';
  onViewModeChange: (mode: 'list' | 'thumbnail') => void;

  // Sorting Control
  sortOptions?: SortOption[];
  sortBy?: string;
  onSortByChange?: (val: string) => void;
  sortOrder?: 'asc' | 'desc';
  onSortOrderToggle?: () => void;

  // Filtering Control
  filterOptions?: FilterOption[];
  activeFilter?: string;
  onFilterChange?: (val: string) => void;

  // Content rendering
  isLoading?: boolean;
  isEmpty?: boolean;
  emptyLabel?: string;
  renderList: () => React.ReactNode;
  renderThumbnail: () => React.ReactNode;

  // Pagination & Telemetry
  pagination?: PaginationData;
  telemetryInfo?: React.ReactNode;
}

export const M3DataView: React.FC<M3DataViewProps> = ({
  title,
  subtitle,
  searchPlaceholder = "Search...",
  searchCriteria,
  activeCriteria,
  onCriteriaChange,
  searchValue,
  onSearchValueChange,
  onSearchSubmit,
  onRegisterNew,
  registerLabel = "New",
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
  emptyLabel = "No matching records found in database.",
  renderList,
  renderThumbnail,
  pagination,
  telemetryInfo,
}) => {
  return (
    <div className="space-y-6 select-none animate-fadeIn">
      {/* 1. HEADER SECTION with options to do queries by criteria */}
      <M3Card variant="elevated" className="p-6 border border-m3-outline/25 bg-m3-surface-container/20 shadow-sm">
        <div className="flex flex-col md:flex-row justify-between md:items-center gap-6">
          <div className="space-y-1">
            <h2 className="text-xl font-extrabold tracking-tight text-m3-on-surface flex items-center gap-2">
              <Database className="w-5.5 h-5.5 text-m3-primary" />
              {title}
            </h2>
            {subtitle && (
              <p className="text-xs text-m3-secondary font-medium max-w-xl">
                {subtitle}
              </p>
            )}
          </div>
          {onRegisterNew && (
            <M3Button 
              variant="filled" 
              onClick={onRegisterNew}
              className="self-start md:self-auto shadow-md rounded-full font-bold flex items-center gap-2 transition-transform duration-150 active:scale-95"
            >
              <Plus className="w-4 h-4" /> {registerLabel}
            </M3Button>
          )}
        </div>

        {/* Query by criteria form */}
        <form onSubmit={onSearchSubmit} className="mt-5 grid grid-cols-1 sm:grid-cols-12 gap-4 items-start">
          {searchCriteria && onCriteriaChange && activeCriteria && (
            <div className="sm:col-span-3 flex flex-col w-full relative">
              <label className="block text-[11px] font-bold text-m3-primary dark:text-m3-primary/80 uppercase tracking-wider mb-2 ml-1">
                Query Criteria
              </label>
              <select
                value={activeCriteria}
                onChange={(e) => onCriteriaChange(e.target.value)}
                className="w-full px-4 py-3.5 text-sm rounded-2xl border border-m3-outline bg-m3-surface-container/40 dark:bg-m3-surface-container/20 text-m3-on-surface focus:outline-none focus:border-m3-primary focus:ring-2 focus:ring-m3-primary/20 transition-all duration-300 select-none cursor-pointer"
              >
                {searchCriteria.map((c) => (
                  <option key={c.value} value={c.value}>
                    {c.label}
                  </option>
                ))}
              </select>
            </div>
          )}

          <div className={`${searchCriteria ? 'sm:col-span-6' : 'sm:col-span-9'}`}>
            <M3TextField
              label="Search Term"
              value={searchValue}
              onChange={(e) => onSearchValueChange(e.target.value)}
              placeholder={searchPlaceholder}
              required
            />
          </div>

          <div className="sm:col-span-3 flex flex-col w-full relative">
            <label className="block text-[11px] font-bold text-transparent select-none mb-2 pointer-events-none">
              &nbsp;
            </label>
            <M3Button 
              variant="outlined" 
              type="submit" 
              className="w-full py-3.5 text-sm font-bold rounded-2xl border-m3-outline text-m3-primary hover:bg-m3-primary/10 transition-all duration-300"
            >
              <Search className="w-4 h-4 mr-2" /> Search
            </M3Button>
          </div>
        </form>
      </M3Card>

      {/* 2. CONTROL ROW: view mode, filter, and order options */}
      <div className="flex flex-col sm:flex-row justify-between items-stretch sm:items-center gap-4 bg-m3-surface-container/30 border border-m3-outline/20 rounded-2xl p-4">
        {/* Left Side: Filter and Sort dropdowns */}
        <div className="flex flex-wrap items-center gap-3.5">
          {/* Filter Option */}
          {filterOptions && onFilterChange && activeFilter !== undefined && (
            <div className="flex items-center gap-2">
              <div className="p-2 bg-m3-primary/10 rounded-lg text-m3-primary border border-m3-primary/10">
                <Filter className="w-3.5 h-3.5" />
              </div>
              <select
                value={activeFilter}
                onChange={(e) => onFilterChange(e.target.value)}
                className="h-9 px-3 rounded-xl border border-m3-outline bg-m3-surface text-[11px] font-semibold text-m3-secondary focus:outline-none focus:border-m3-primary focus:ring-1 focus:ring-m3-primary/20 transition-all cursor-pointer"
              >
                {filterOptions.map((f) => (
                  <option key={f.value} value={f.value}>
                    {f.label}
                  </option>
                ))}
              </select>
            </div>
          )}

          {/* Sort Option */}
          {sortOptions && onSortByChange && sortBy !== undefined && (
            <div className="flex items-center gap-2">
              <div className="p-2 bg-m3-primary/10 rounded-lg text-m3-primary border border-m3-primary/10">
                <ArrowUpDown className="w-3.5 h-3.5" />
              </div>
              <select
                value={sortBy}
                onChange={(e) => onSortByChange(e.target.value)}
                className="h-9 px-3 rounded-xl border border-m3-outline bg-m3-surface text-[11px] font-semibold text-m3-secondary focus:outline-none focus:border-m3-primary focus:ring-1 focus:ring-m3-primary/20 transition-all cursor-pointer"
                title="Sort attribute"
              >
                {sortOptions.map((s) => (
                  <option key={s.value} value={s.value}>
                    {s.label}
                  </option>
                ))}
              </select>

              {onSortOrderToggle && sortOrder && (
                <button
                  type="button"
                  onClick={onSortOrderToggle}
                  className="h-9 w-9 flex items-center justify-center rounded-xl border border-m3-outline bg-m3-surface hover:bg-m3-primary/10 hover:border-m3-primary/30 transition-all text-m3-secondary"
                  title={`Order: ${sortOrder === 'asc' ? 'Ascending' : 'Descending'}`}
                >
                  <ArrowUpDown className={`w-3.5 h-3.5 transition-transform duration-200 ${sortOrder === 'desc' ? 'rotate-180 text-m3-primary' : ''}`} />
                </button>
              )}
            </div>
          )}
        </div>

        {/* Right Side: View Mode Segmented Controls */}
        <div className="flex items-center gap-1.5 bg-m3-surface-container rounded-xl p-1 border border-m3-outline/25 self-end sm:self-auto">
          <button
            type="button"
            onClick={() => onViewModeChange('list')}
            className={`p-2 rounded-lg transition-all duration-200 flex items-center justify-center ${
              viewMode === 'list'
                ? 'bg-m3-primary text-m3-on-primary font-bold shadow-sm'
                : 'text-m3-secondary hover:bg-m3-primary/10'
            }`}
            title="List Table view"
          >
            <LayoutList className="w-4 h-4" />
          </button>
          <button
            type="button"
            onClick={() => onViewModeChange('thumbnail')}
            className={`p-2 rounded-lg transition-all duration-200 flex items-center justify-center ${
              viewMode === 'thumbnail'
                ? 'bg-m3-primary text-m3-on-primary font-bold shadow-sm'
                : 'text-m3-secondary hover:bg-m3-primary/10'
            }`}
            title="Thumbnail Grid view"
          >
            <LayoutGrid className="w-4 h-4" />
          </button>
        </div>
      </div>

      {/* 3. MAIN CONTENT SLOT */}
      <div className="min-h-[200px] relative">
        {isLoading ? (
          <div className="py-24 text-center text-xs text-m3-secondary animate-pulse">
            <svg className="animate-spin h-8 w-8 text-m3-primary mx-auto mb-3" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
            </svg>
            Fetching matching database configurations...
          </div>
        ) : isEmpty ? (
          <M3Card variant="elevated" className="p-8 text-center text-xs text-m3-secondary border border-m3-outline/20 bg-m3-surface-container/10">
            <div className="w-12 h-12 bg-m3-primary/10 border border-m3-primary/20 text-m3-primary rounded-full flex items-center justify-center mx-auto mb-3.5">
              <Info className="w-6 h-6" />
            </div>
            <h4 className="text-sm font-extrabold text-m3-on-surface">No Records Found</h4>
            <p className="mt-1 font-medium leading-relaxed max-w-sm mx-auto">
              {emptyLabel}
            </p>
          </M3Card>
        ) : (
          <div className="animate-fadeIn">
            {viewMode === 'list' ? renderList() : renderThumbnail()}
          </div>
        )}
      </div>

      {/* 4. FOOTER: pagination and telemetry info */}
      {(pagination || telemetryInfo) && (
        <div className="flex flex-col sm:flex-row justify-between items-center gap-4 bg-m3-surface-container/20 border-t border-m3-outline/20 p-4 rounded-xl text-xs font-bold text-m3-secondary">
          {/* Custom Telemetry Left */}
          <div className="flex items-center gap-2">
            {telemetryInfo ? (
              telemetryInfo
            ) : (
              pagination && (
                <div className="flex items-center gap-1">
                  <span className="h-2 w-2 rounded-full bg-emerald-500 animate-pulse" />
                  <span className="text-[10px] uppercase tracking-wider text-m3-secondary/70">
                    Total Records: {pagination.totalItems} | Size: {pagination.pageSize} per page
                  </span>
                </div>
              )
            )}
          </div>

          {/* Pagination Right */}
          {pagination && pagination.totalPages > 1 && (
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={() => pagination.onPageChange(Math.max(1, pagination.page - 1))}
                disabled={pagination.page === 1}
                className="p-1.5 rounded-lg border border-m3-outline bg-m3-surface text-m3-secondary disabled:opacity-40 disabled:cursor-not-allowed hover:bg-m3-primary/10 hover:text-m3-primary transition-all"
                title="Previous page"
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
                      className={`h-7 min-w-7 px-2 text-[10px] font-bold rounded-lg flex items-center justify-center transition-all ${
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
                title="Next page"
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
