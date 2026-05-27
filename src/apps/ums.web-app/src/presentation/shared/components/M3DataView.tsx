import React, { useState, useCallback } from 'react';
import {
  Search,
  LayoutList,
  LayoutGrid,
  ChevronLeft,
  ChevronRight,
  ChevronsUp,
  ChevronsDown,
  Plus,
  ArrowUpDown,
  Filter,
  Database,
} from 'lucide-react';
import { M3Card } from './M3Card';
import { M3Button } from './M3Button';
import { M3TextField } from './M3TextField';
import { EmptyState } from './EmptyState';
import { M3SegmentedButton } from './M3SegmentedButton';
import type { SegmentOption } from './M3SegmentedButton';
import { useDragResize } from '@app/hooks/use-drag-resize';

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

const VIEW_MODE_OPTIONS: SegmentOption<'list' | 'thumbnail'>[] = [
  { value: 'list', label: <LayoutList className="w-4 h-4" /> },
  { value: 'thumbnail', label: <LayoutGrid className="w-4 h-4" /> },
];

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
  emptyTooltip?: string;
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
  emptyTooltip,
  loadingLabel = 'Loading...',
  criteriaLabel = 'Criteria',
  searchTermLabel = 'Search term',
  searchButtonLabel = 'Search',
  renderList,
  renderThumbnail,
  pagination,
  telemetryInfo,
}) => {
  const {
    size: topPx,
    isCollapsed: isHeaderCollapsed,
    isDragging: isDraggingH,
    containerRef: dvContainerRef,
    resizableRef: searchZoneRef,
    handleMouseDown: handleHSplitterMouseDown,
    handleKeyDown: handleHSplitterKeyDown,
    toggleCollapse: toggleHeader,
  } = useDragResize();

  // ── Footer collapse ────────────────────────────────────────────────────────
  const [isFooterCollapsed, setIsFooterCollapsed] = useState(false);
  const toggleFooter = useCallback(() => setIsFooterCollapsed((v) => !v), []);

  return (
    <div
      ref={dvContainerRef}
      className={`flex flex-col h-full${isDraggingH ? ' select-none cursor-row-resize' : ''}`}
    >

      {/*
        ══════════════════════════════════════════════════════════════
        ZONE A — Title bar   (ALWAYS VISIBLE — never collapses)
        Contains: title, subtitle, + New button, and the header toggle.
        Lives outside the collapsible searchZoneRef so the toggle
        button is always reachable regardless of collapsed state.
        ══════════════════════════════════════════════════════════════
      */}
      <M3Card
        variant="elevated"
        className="flex-shrink-0 border border-m3-outline/25 bg-m3-surface-container/20 shadow-sm"
      >
        <div className="px-5 py-4 flex items-center justify-between gap-3">
          {/* Left: title + subtitle */}
          <div className="min-w-0 flex-1 space-y-0.5">
            <h2 className="text-base font-semibold text-m3-on-surface flex items-center gap-2">
              <Database className="w-5 h-5 text-m3-primary flex-shrink-0" />
              <span className="truncate">{title}</span>
            </h2>
            {subtitle && (
              <p className="text-xs text-m3-secondary font-normal pl-7 truncate">
                {subtitle}
              </p>
            )}
          </div>

          {/* Right: + New  +  header collapse toggle */}
          <div className="flex items-center gap-2 flex-shrink-0">
            {onRegisterNew && (
              <button
                type="button"
                onClick={onRegisterNew}
                className="inline-flex h-8 items-center gap-1.5 rounded-lg border border-m3-primary/25 bg-m3-primary/10 px-3 text-xs font-medium text-m3-primary transition-colors hover:border-m3-primary/45 hover:bg-m3-primary/15 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-m3-primary focus-visible:ring-offset-1"
              >
                <Plus className="h-3.5 w-3.5" /> {registerLabel}
              </button>
            )}

            {/* Header collapse / expand button — always reachable */}
            <button
              type="button"
              title={isHeaderCollapsed ? 'Show search & filters' : 'Hide search & filters'}
              onClick={toggleHeader}
              className={[
                'p-1.5 rounded-lg border transition-all duration-150',
                isHeaderCollapsed
                  ? 'bg-m3-primary/10 border-m3-primary/40 text-m3-primary'
                  : 'bg-m3-surface-container/60 border-m3-outline/40 text-m3-secondary hover:bg-m3-primary/10 hover:border-m3-primary/40 hover:text-m3-primary',
              ].join(' ')}
            >
              {isHeaderCollapsed
                ? <ChevronsDown className="w-3.5 h-3.5" />
                : <ChevronsUp   className="w-3.5 h-3.5" />
              }
            </button>
          </div>
        </div>
      </M3Card>

      {/*
        ══════════════════════════════════════════════════════════════
        ZONE B — Search form + control row   (collapsible + draggable)
        searchZoneRef tracks the rendered height for the drag handler.
        When isHeaderCollapsed: height → 0 (overflow hidden).
        ══════════════════════════════════════════════════════════════
      */}
      <div
        ref={searchZoneRef}
        style={topPx !== null ? { height: topPx } : undefined}
        className={[
          'flex flex-col gap-3 flex-shrink-0',
          topPx !== null ? 'overflow-hidden' : 'overflow-visible',
          topPx !== null && !isDraggingH ? 'transition-[height] duration-200 ease-in-out' : '',
        ].join(' ')}
      >
        {/* Search form card */}
        <M3Card
          variant="elevated"
          className="mt-3 p-3 border border-m3-outline/25 bg-m3-surface-container/20 shadow-sm flex-shrink-0"
        >
          <form onSubmit={onSearchSubmit} className="flex flex-col gap-2 sm:flex-row sm:items-end">
            {searchCriteria && onCriteriaChange && activeCriteria && (
              <div className="relative flex w-full flex-col sm:w-40 sm:flex-shrink-0">
                <label className="block text-[11px] font-medium text-m3-secondary mb-1 ml-1">
                  {criteriaLabel}
                </label>
                <select
                  value={activeCriteria}
                  onChange={(e) => onCriteriaChange(e.target.value)}
                  aria-label={criteriaLabel}
                  className="w-full h-10 px-3 text-xs rounded-lg border border-m3-outline bg-m3-surface-container/30 dark:bg-m3-surface-container/20 text-m3-on-surface focus:outline-none focus:border-m3-primary transition-colors cursor-pointer"
                >
                  {searchCriteria.map((c) => (
                    <option key={c.value} value={c.value}>{c.label}</option>
                  ))}
                </select>
              </div>
            )}

            <div className="w-full sm:min-w-0 sm:flex-1">
              <M3TextField
                dense
                label={searchTermLabel}
                value={searchValue}
                onChange={(e) => onSearchValueChange(e.target.value)}
                placeholder={searchPlaceholder}
                className="mb-0"
              />
            </div>

            <div className="flex items-center">
              <button
                type="submit"
                className="inline-flex h-10 items-center justify-center gap-1.5 rounded-lg border border-m3-primary/25 bg-m3-primary/10 px-3 text-xs font-medium text-m3-primary transition-colors hover:border-m3-primary/45 hover:bg-m3-primary/15 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-m3-primary focus-visible:ring-offset-1"
              >
                <Search className="h-3.5 w-3.5" /> {searchButtonLabel}
              </button>
            </div>
          </form>
        </M3Card>

        {/* Control row: filter + sort + view mode */}
        <div className="flex flex-col sm:flex-row justify-between items-stretch sm:items-center gap-2 bg-m3-surface-container/30 border border-m3-outline/20 rounded-xl p-2 flex-shrink-0">
          <div className="flex flex-wrap items-center gap-2">
            {filterOptions && onFilterChange && activeFilter !== undefined && (
              <div className="flex items-center gap-2">
                <div className="p-1 bg-m3-primary/10 rounded-md text-m3-primary border border-m3-primary/10">
                  <Filter className="w-3.5 h-3.5" />
                </div>
                <select
                  value={activeFilter}
                  onChange={(e) => onFilterChange(e.target.value)}
                  aria-label="Filter"
                  className="h-8 px-2.5 rounded-lg border border-m3-outline bg-m3-surface text-xs font-medium text-m3-secondary focus:outline-none focus:border-m3-primary transition-colors cursor-pointer"
                >
                  {filterOptions.map((f) => (
                    <option key={f.value} value={f.value}>{f.label}</option>
                  ))}
                </select>
              </div>
            )}

            {sortOptions && onSortByChange && sortBy !== undefined && (
              <div className="flex items-center gap-2">
                <div className="p-1 bg-m3-primary/10 rounded-md text-m3-primary border border-m3-primary/10">
                  <ArrowUpDown className="w-3.5 h-3.5" />
                </div>
                <select
                  value={sortBy}
                  onChange={(e) => onSortByChange(e.target.value)}
                  aria-label="Sort by"
                  className="h-8 px-2.5 rounded-lg border border-m3-outline bg-m3-surface text-xs font-medium text-m3-secondary focus:outline-none focus:border-m3-primary transition-colors cursor-pointer"
                >
                  {sortOptions.map((s) => (
                    <option key={s.value} value={s.value}>{s.label}</option>
                  ))}
                </select>

                {onSortOrderToggle && sortOrder && (
                  <button
                    type="button"
                    onClick={onSortOrderToggle}
                    className="h-8 w-8 flex items-center justify-center rounded-lg border border-m3-outline bg-m3-surface hover:bg-m3-primary/10 hover:border-m3-primary/30 transition-all text-m3-secondary"
                    title={sortOrder === 'asc' ? 'Ascending' : 'Descending'}
                  >
                    <ArrowUpDown className={`w-3.5 h-3.5 transition-transform duration-200 ${sortOrder === 'desc' ? 'rotate-180 text-m3-primary' : ''}`} />
                  </button>
                )}
              </div>
            )}
          </div>

          {/* View mode segmented control */}
          <M3SegmentedButton
            options={VIEW_MODE_OPTIONS}
            value={viewMode}
            onChange={onViewModeChange}
            size="sm"
            className="self-end sm:self-auto"
          />
        </div>
      </div>{/* end search zone */}

      {/*
        ══════════════════════════════════════════════════════════════
        SPLITTER — drag handle between search zone and list
        ══════════════════════════════════════════════════════════════
      */}
      <div
        onMouseDown={handleHSplitterMouseDown}
        onKeyDown={handleHSplitterKeyDown}
        role="separator"
        tabIndex={0}
        aria-orientation="horizontal"
        aria-label="Resize tenant search panel"
        aria-valuemin={0}
        aria-valuemax={100}
        aria-valuenow={topPx ?? undefined}
        className={[
          'relative flex-shrink-0 h-1.5 w-full flex items-center justify-center group cursor-row-resize z-10 mt-1 focus:outline-none focus-visible:ring-2 focus-visible:ring-m3-primary',
          isDraggingH ? 'bg-m3-primary/20' : 'hover:bg-m3-primary/10 transition-colors duration-150',
        ].join(' ')}
      >
        <div
          className={[
            'h-px w-full transition-colors duration-150',
            isDraggingH ? 'bg-m3-primary/60' : 'bg-m3-outline/30 group-hover:bg-m3-primary/40',
          ].join(' ')}
        />
        {/* Pill — always visible on the handle */}
        <button
          type="button"
          title={isHeaderCollapsed ? 'Expand search panel' : 'Collapse search panel'}
          onClick={(e) => { e.stopPropagation(); toggleHeader(); }}
          onMouseDown={(e) => e.stopPropagation()}
          className={[
            'absolute left-1/2 -translate-x-1/2 -translate-y-px',
            'h-4 w-12 rounded-full flex items-center justify-center gap-0.5',
            'border shadow-sm transition-all duration-150 text-[10px] font-medium',
            isDraggingH
              ? 'bg-m3-primary text-white border-m3-primary'
              : isHeaderCollapsed
                ? 'bg-m3-primary/15 border-m3-primary/50 text-m3-primary hover:bg-m3-primary/25'
                : 'bg-m3-surface-container border-m3-outline/50 text-m3-secondary hover:bg-m3-primary/10 hover:border-m3-primary/40 hover:text-m3-primary',
          ].join(' ')}
        >
          {isHeaderCollapsed
            ? <ChevronsDown className="w-3 h-3" />
            : <ChevronsUp   className="w-3 h-3" />
          }
        </button>
      </div>

      {/*
        ══════════════════════════════════════════════════════════════
        ZONE C — Content + Footer   (flex-1, takes remaining height)
        ══════════════════════════════════════════════════════════════
      */}
      <div className="flex-1 flex flex-col gap-3 min-h-0 overflow-hidden pt-1">

        {/* List content */}
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
            <EmptyState variant="card" title={emptyTitle} message={emptyLabel} tooltip={emptyTooltip} />
          ) : (
            <div className="animate-fadeIn">
              {viewMode === 'list' ? renderList() : renderThumbnail()}
            </div>
          )}
        </div>

        {/* Footer: permanent slim strip + collapsible pagination */}
        {(pagination || telemetryInfo) && (
          <div className="flex-shrink-0 bg-m3-surface-container/20 border border-m3-outline/20 rounded-xl overflow-hidden">

            {/* Always-visible strip: record count + footer toggle */}
            <div className="flex items-center justify-between px-3 h-9 gap-3">
              <div className="flex items-center gap-2 min-w-0 flex-1 text-xs font-medium text-m3-secondary">
                {telemetryInfo ?? (
                  pagination && (
                    <div className="flex items-center gap-1.5">
                      <span className="h-2 w-2 rounded-full bg-emerald-500 animate-pulse flex-shrink-0" />
                      <span className="text-xs text-m3-secondary/70 truncate">
                        {pagination.totalItems} records · {pagination.pageSize} per page
                      </span>
                    </div>
                  )
                )}
              </div>

              {/* Footer collapse toggle — only shown when pagination exists */}
              {pagination && pagination.totalPages > 1 && (
                <button
                  type="button"
                  title={isFooterCollapsed ? 'Show pagination' : 'Hide pagination'}
                  onClick={toggleFooter}
                  className={[
                    'flex-shrink-0 p-1.5 rounded-lg border transition-all duration-150',
                    isFooterCollapsed
                      ? 'bg-m3-primary/10 border-m3-primary/40 text-m3-primary'
                      : 'bg-m3-surface-container/60 border-m3-outline/40 text-m3-secondary hover:bg-m3-primary/10 hover:border-m3-primary/40 hover:text-m3-primary',
                  ].join(' ')}
                >
                  {isFooterCollapsed
                    ? <ChevronsUp   className="w-3.5 h-3.5" />
                    : <ChevronsDown className="w-3.5 h-3.5" />
                  }
                </button>
              )}
            </div>

            {/* Collapsible pagination row */}
            {pagination && pagination.totalPages > 1 && (
              <div
                style={{ maxHeight: isFooterCollapsed ? 0 : '3.5rem' }}
                className="overflow-hidden transition-[max-height] duration-200 ease-in-out"
              >
                <div className="flex justify-end items-center gap-2 px-3 pb-2.5">
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
              </div>
            )}

          </div>
        )}

      </div>{/* end bottom zone */}
    </div>
  );
};
