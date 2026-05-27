import React from 'react';

/**
 * EntityRow — reusable row for entity lists (branches, providers, etc.)
 *
 * Renders an M3-compliant list item row with:
 *   - leading: icon or avatar slot
 *   - children: primary content (headline, supporting text)
 *   - trailingColumns: array of fixed-width column slots for aligned badges/actions
 *   - trailing (legacy freeform): unstructured trailing content
 *   - Interactive states (hover, selected, disabled)
 *
 * Use `trailingColumns` for aligned grid layout across rows.
 * Use `trailing` for simple, single-element trailing content.
 */

export interface TrailingColumn {
  /** Content to render in the column. */
  content: React.ReactNode;
  /** Fixed width class for alignment (e.g. 'w-20', 'w-24'). */
  width?: string;
  /** Horizontal alignment within the column. Default: 'end'. */
  align?: 'start' | 'center' | 'end';
}

export interface EntityRowProps {
  /** Unique key for the entity (used as React key externally, optional here). */
  id?: string;
  /** Whether the entity is active in the business logic (fades out if false). */
  isActive?: boolean;
  /** Whether the row is currently selected in the UI. */
  selected?: boolean;
  /** Primary content slot (name, badges, metadata). */
  children: React.ReactNode;
  /** Leading slot for icons, checkboxes, or avatars. */
  leading?: React.ReactNode;
  /** Structured trailing columns with fixed widths for cross-row alignment. */
  trailingColumns?: TrailingColumn[];
  /** Trailing slot for action buttons or trailing badges (legacy freeform). */
  trailing?: React.ReactNode;
  /** Legacy alias for trailing. */
  actions?: React.ReactNode;
  /** Click handler for the entire row. */
  onClick?: () => void;
  /** Called on double-click (typically opens inline edit). */
  onDoubleClick?: () => void;
  /** Extra Tailwind classes. */
  className?: string;
}

const ALIGN_MAP = {
  start: 'justify-start',
  center: 'justify-center',
  end: 'justify-end',
} as const;

export const EntityRow: React.FC<EntityRowProps> = ({
  isActive = true,
  selected = false,
  children,
  leading,
  trailingColumns,
  trailing,
  actions,
  onClick,
  onDoubleClick,
  className = '',
}) => {
  const isInteractive = !!onClick || !!onDoubleClick;

  // Determine trailing content: prefer structured columns, fall back to freeform
  const hasStructuredTrailing = trailingColumns && trailingColumns.length > 0;
  const hasFreeformTrailing = !!(trailing || actions);

  return (
    <div
      onClick={onClick}
      onDoubleClick={onDoubleClick}
      className={[
        'group/row flex items-center px-3 py-2.5 min-h-[3.5rem] mx-2 my-0.5 rounded-xl',
        'transition-all duration-200 outline-none',
        isInteractive ? 'cursor-pointer' : 'cursor-default',
        isActive ? 'opacity-100' : 'opacity-65',
        selected
          ? 'bg-m3-primary/10 border border-m3-primary/10'
          : isInteractive
          ? 'bg-transparent hover:bg-m3-surface-container/30 border border-transparent hover:border-m3-outline/5'
          : 'bg-transparent border border-transparent',
        className,
      ].join(' ')}
    >
      {/* Leading + Content area — takes available space */}
      <div className="flex items-center gap-3 flex-1 min-w-0">
        {leading && <div className="shrink-0">{leading}</div>}
        <div className="flex-1 min-w-0 flex flex-col justify-center">{children}</div>
      </div>

      {/* Structured trailing columns — fixed-width grid for alignment */}
      {hasStructuredTrailing && (
        <div className="flex items-center gap-3 shrink-0 ml-4">
          {trailingColumns.map((col, i) => (
            <div
              key={i}
              className={`${col.width ?? 'w-auto'} flex items-center ${ALIGN_MAP[col.align ?? 'end']} shrink-0`}
            >
              {col.content}
            </div>
          ))}
        </div>
      )}

      {/* Legacy freeform trailing — only when no structured columns */}
      {!hasStructuredTrailing && hasFreeformTrailing && (
        <div className="flex items-center gap-2 shrink-0 ml-4">
          {trailing || actions}
        </div>
      )}
    </div>
  );
};
