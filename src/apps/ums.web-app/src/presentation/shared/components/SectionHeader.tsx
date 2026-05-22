import React from 'react';

/**
 * SectionHeader — consistent section title bar with optional actions.
 *
 * Used inside detail panels (BranchManager, IdpPanel, BrandingPanel)
 * to provide a uniform header with bottom border, title, subtitle,
 * and an action slot.
 */

export interface SectionHeaderProps {
  /** Section title. */
  title: string;
  /** Optional subtitle shown below the title. */
  subtitle?: string;
  /** Action buttons / badges rendered on the right. */
  actions?: React.ReactNode;
  /** Extra Tailwind classes. */
  className?: string;
}

export const SectionHeader: React.FC<SectionHeaderProps> = React.memo(({
  title,
  subtitle,
  actions,
  className = '',
}) => (
  <div
    className={`flex justify-between items-center border-b border-m3-outline/10 pb-3 ${className}`}
  >
    <div>
      <h3 className="text-xs font-semibold text-m3-on-surface">{title}</h3>
      {subtitle && (
        <p className="text-[9px] text-m3-secondary font-bold">{subtitle}</p>
      )}
    </div>
    {actions && <div className="flex items-center gap-2">{actions}</div>}
  </div>
));
