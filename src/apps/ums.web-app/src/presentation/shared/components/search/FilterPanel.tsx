import React from 'react';
import { Filter, ArrowUpDown } from 'lucide-react';
import { M3SegmentedButton, SegmentOption } from '../M3SegmentedButton';

export interface FilterOption {
  label: string;
  value: string;
}

export interface SortOption {
  label: string;
  value: string;
}

interface FilterPanelProps {
  filterOptions?: FilterOption[];
  activeFilter?: string;
  onFilterChange?: (val: string) => void;
  
  sortOptions?: SortOption[];
  sortBy?: string;
  onSortByChange?: (val: string) => void;
  
  sortOrder?: 'asc' | 'desc';
  onSortOrderToggle?: () => void;
  
  viewModeOptions?: SegmentOption<'list' | 'thumbnail'>[];
  viewMode?: 'list' | 'thumbnail';
  onViewModeChange?: (mode: 'list' | 'thumbnail') => void;
}

export const FilterPanel: React.FC<FilterPanelProps> = ({
  filterOptions,
  activeFilter,
  onFilterChange,
  sortOptions,
  sortBy,
  onSortByChange,
  sortOrder,
  onSortOrderToggle,
  viewModeOptions,
  viewMode,
  onViewModeChange,
}) => {
  return (
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

      {viewModeOptions && viewMode && onViewModeChange && (
        <M3SegmentedButton
          options={viewModeOptions}
          value={viewMode}
          onChange={(val) => onViewModeChange(val as 'list' | 'thumbnail')}
          size="sm"
          className="self-end sm:self-auto"
        />
      )}
    </div>
  );
};
