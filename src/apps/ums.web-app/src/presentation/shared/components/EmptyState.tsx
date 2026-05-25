import React from 'react';
import { Info } from 'lucide-react';
import { M3Card } from './M3Card';
import { Tooltip } from './Tooltip';

/**
 * EmptyState — standardised "no data" placeholder.
 *
 * Two visual variants:
 *   - "dashed": border-dashed container (for inline sections like branch lists)
 *   - "card":   elevated M3Card with icon circle (for full-panel empty states)
 */

export interface EmptyStateProps {
  /** Icon rendered above the message. Defaults to <Info />. Pass `null` to hide. */
  icon?: React.ReactNode | null;
  /** Optional title (shown only in "card" variant). */
  title?: string;
  /** Main descriptive message. */
  message: string;
  /** Visual style. @default "dashed" */
  variant?: 'dashed' | 'card';
  /** Optional tooltip text to show an Info icon next to the title. */
  tooltip?: string;
  /** Extra Tailwind classes. */
  className?: string;
}

export const EmptyState: React.FC<EmptyStateProps> = React.memo(({
  icon,
  title,
  message,
  variant = 'dashed',
  tooltip,
  className = '',
}) => {
  const iconNode =
    icon === null
      ? null
      : icon ?? <Info className={variant === 'card' ? 'w-6 h-6' : 'w-6 h-6 text-m3-outline'} />;

  if (variant === 'card') {
    return (
      <M3Card
        variant="elevated"
        className={`p-8 text-center text-sm text-m3-secondary border border-m3-outline/20 bg-m3-surface-container/10 ${className}`}
      >
        {iconNode && (
          <div className="w-12 h-12 bg-m3-primary/10 border border-m3-primary/20 text-m3-primary rounded-full flex items-center justify-center mx-auto mb-3.5">
            {iconNode}
          </div>
        )}
        {title && (
          <div className="flex items-center justify-center gap-1.5">
            <h4 className="text-sm font-semibold text-m3-on-surface">{title}</h4>
            {tooltip && (
              <Tooltip content={tooltip}>
                <Info className="w-4 h-4 text-m3-secondary hover:text-m3-primary cursor-help transition-colors" />
              </Tooltip>
            )}
          </div>
        )}
        <p className="mt-1 text-xs font-normal leading-relaxed max-w-sm mx-auto text-m3-secondary">
          {message}
        </p>
      </M3Card>
    );
  }

  // "dashed" variant — lightweight inline empty state
  return (
    <div
      className={`py-8 text-center text-[10px] text-m3-secondary/75 flex flex-col items-center justify-center gap-1.5 border border-dashed border-m3-outline/25 rounded-2xl p-4 ${className}`}
    >
      {iconNode}
      {message}
    </div>
  );
});
