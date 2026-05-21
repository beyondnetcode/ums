import React from 'react';

/**
 * EntityRow — reusable row for entity lists (branches, providers, etc.)
 *
 * Renders a rounded-border row with:
 *   - Left: primary content (name, badges, subtitle)
 *   - Right: action buttons (visible on hover)
 *   - Active/inactive visual distinction
 *   - Double-click to edit support
 */

export interface EntityRowProps {
  /** Unique key for the entity (used as React key externally). */
  id: string;
  /** Whether the entity is active — controls opacity/border style. */
  isActive?: boolean;
  /** Primary content slot (name, badges, metadata). */
  children: React.ReactNode;
  /** Action buttons shown on the right side. */
  actions?: React.ReactNode;
  /** Called on double-click (typically opens inline edit). */
  onDoubleClick?: () => void;
  /** Extra Tailwind classes. */
  className?: string;
}

export const EntityRow: React.FC<EntityRowProps> = ({
  isActive = true,
  children,
  actions,
  onDoubleClick,
  className = '',
}) => (
  <div
    onDoubleClick={onDoubleClick}
    className={[
      'group/row p-3 rounded-xl border flex items-center justify-between',
      'transition-colors bg-m3-surface-container/40 cursor-default',
      isActive
        ? 'border-m3-outline/35 hover:border-m3-primary/25'
        : 'border-m3-outline/15 opacity-65 bg-m3-outline/5',
      className,
    ].join(' ')}
  >
    <div className="space-y-0.5 max-w-[75%]">{children}</div>
    {actions && <div className="flex items-center gap-0.5">{actions}</div>}
  </div>
);
