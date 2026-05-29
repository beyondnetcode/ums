import React from 'react';

interface DataTableProps {
  columns: number;
  viewMode: 'list' | 'thumbnail';
}

export const DataTable: React.FC<DataTableProps> = ({ columns, viewMode }) => {
  return (
    <div className="flex flex-col space-y-4 px-2 py-4">
      {Array.from({ length: 5 }).map((_, i) => (
        <div
          key={i}
          className={`grid gap-4 ${viewMode === 'list' ? `grid-cols-${columns}` : 'grid-cols-2'}`}
        >
          {Array.from({ length: viewMode === 'list' ? columns : 2 }).map((_, j) => (
            <div
              key={j}
              className="h-12 rounded-lg bg-m3-surface-container/40 animate-pulse"
            />
          ))}
        </div>
      ))}
    </div>
  );
};