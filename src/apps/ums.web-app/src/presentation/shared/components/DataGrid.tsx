/**
 * DataGrid.tsx — Generic, configurable data table for any aggregate.
 *
 * Accepts column definitions and a data array, renders a styled table
 * with optional row selection, expand/collapse, and custom cell renderers.
 * Replaces hardcoded table markup in list panels.
 */
import React from 'react';
import { Check, ChevronRight } from 'lucide-react';

export interface ColumnDef<T> {
  header: string;
  accessor?: keyof T;
  cell?: (item: T) => React.ReactNode;
  className?: string;
  headerClassName?: string;
}

export interface DataGridProps<T> {
  columns: ColumnDef<T>[];
  data: T[];
  idKey: keyof T;
  selectedId?: string;
  onSelect?: (id: string) => void;
  expandedIds?: Set<string>;
  onToggleExpand?: (id: string) => void;
  parentIdKey?: keyof T;
  className?: string;
  emptyLabel?: string;
  isLoading?: boolean;
}

function DataGrid<T extends Record<string, unknown>>({
  columns,
  data,
  idKey,
  selectedId,
  onSelect,
  expandedIds = new Set(),
  onToggleExpand,
  parentIdKey,
  className = '',
  emptyLabel = 'No records found.',
  isLoading = false,
}: DataGridProps<T>) {
  if (isLoading) {
    return (
      <div className="py-12 text-center text-sm text-m3-secondary">
        <svg className="animate-spin h-6 w-6 text-m3-primary mx-auto mb-2" fill="none" viewBox="0 0 24 24">
          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
        </svg>
        Loading...
      </div>
    );
  }

  if (data.length === 0) {
    return <div className="py-8 text-center text-sm text-m3-secondary">{emptyLabel}</div>;
  }

  const hasHierarchy = !!parentIdKey && !!onToggleExpand;

  const renderRow = (item: T, isChild = false) => {
    const id = String(item[idKey]);
    const isSelected = selectedId === id;
    const hasChildren = hasHierarchy && parentIdKey && data.some((d) => String(d[parentIdKey]) === id);
    const isExpanded = expandedIds.has(id);

    return (
      <React.Fragment key={id}>
        <tr
          onClick={() => onSelect?.(id)}
          className={`group cursor-pointer transition-colors duration-150 ${
            isSelected
              ? 'bg-m3-primary-container/30 text-m3-on-primary-container'
              : 'hover:bg-m3-primary/5 text-m3-secondary hover:text-m3-on-surface'
          } ${isChild ? 'bg-m3-surface-container/10' : ''}`}
        >
          {columns.map((col, idx) => (
            <td
              key={idx}
              className={`py-3.5 ${isChild && idx === 0 ? 'pl-10' : col.className ?? (idx === 0 ? 'px-5' : 'px-4')}`}
            >
              {idx === 0 ? (
                <div className="flex items-center gap-3">
                  {hasHierarchy && (
                    hasChildren ? (
                      <button
                        onClick={(e) => { e.stopPropagation(); onToggleExpand?.(id); }}
                        className={`p-0.5 rounded transition-transform duration-200 ${isExpanded ? 'rotate-90' : ''}`}
                      >
                        <ChevronRight className="w-3.5 h-3.5 text-m3-secondary" />
                      </button>
                    ) : (
                      <span className="w-4" />
                    )
                  )}
                  {col.cell ? col.cell(item) : (
                    <span className="font-medium text-m3-on-surface">{col.accessor ? String(item[col.accessor] ?? '') : ''}</span>
                  )}
                </div>
              ) : col.cell ? col.cell(item) : (
                <span className={isChild ? 'opacity-70' : ''}>{col.accessor ? String(item[col.accessor] ?? '') : ''}</span>
              )}
            </td>
          ))}
          {onSelect && (
            <td className="py-3.5 px-5 text-right">
              <div className="flex items-center justify-end">
                {isSelected && (
                  <span className="h-5 w-5 bg-m3-primary text-m3-on-primary rounded-full flex items-center justify-center">
                    <Check className="w-3 h-3" />
                  </span>
                )}
              </div>
            </td>
          )}
        </tr>
        {isExpanded && hasHierarchy && parentIdKey && (
          data
            .filter((d) => String(d[parentIdKey]) === id)
            .map((child) => renderRow(child, true))
        )}
      </React.Fragment>
    );
  };

  return (
    <div className={`overflow-x-auto border border-m3-outline/25 rounded-xl bg-m3-surface-container/20 ${className}`}>
      <table className="w-full text-left border-collapse">
        <thead>
          <tr className="border-b border-m3-outline/20 text-xs font-medium text-m3-secondary bg-m3-surface-container/40">
            {columns.map((col, idx) => (
              <th
                key={idx}
                className={`py-3.5 ${col.headerClassName ?? (idx === 0 ? 'px-5' : 'px-4')}`}
              >
                {col.header}
              </th>
            ))}
            {onSelect && <th className="py-3.5 px-5 text-right" />}
          </tr>
        </thead>
        <tbody className="divide-y divide-m3-outline/10 text-sm">
          {data
            .filter((item) => !parentIdKey || !item[parentIdKey])
            .map((item) => renderRow(item))}
        </tbody>
      </table>
    </div>
  );
}

export default DataGrid;
