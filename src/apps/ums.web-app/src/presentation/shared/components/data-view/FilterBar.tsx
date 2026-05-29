import React from 'react';
import { Filter } from 'lucide-react';
import type { FilterOption } from '../M3DataView';

interface FilterBarProps {
  filterOptions?: FilterOption[];
  activeFilter?: string;
  onFilterChange?: (val: string) => void;
}

export const FilterBar: React.FC<FilterBarProps> = ({
  filterOptions,
  activeFilter,
  onFilterChange,
}) => {
  if (!filterOptions || !onFilterChange || activeFilter === undefined) {
    return null;
  }

  return (
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
  );
};