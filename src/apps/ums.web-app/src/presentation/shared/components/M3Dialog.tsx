import React from 'react';
import { ShieldAlert } from 'lucide-react';
import { M3Card } from './M3Card';
import { M3Button } from './M3Button';

// ─── Types ──────────────────────────────────────────────────────────────────

export interface M3DialogAction {
  label: string;
  variant?: 'filled' | 'tonal' | 'outlined' | 'text';
  /** Extra Tailwind classes — use to override colour for destructive actions. */
  className?: string;
  onClick: () => void;
}

export interface M3DialogProps {
  /** Controls visibility — the dialog renders nothing when `false`. */
  open: boolean;
  /** Dialog title (short). */
  title: string;
  /** Supporting description shown below the title. */
  message?: string;
  /** Optional custom icon; defaults to a warning icon. Pass `null` to hide. */
  icon?: React.ReactNode | null;
  /** Colour ring around the default icon — e.g. `"bg-amber-500/15 text-amber-500"`. */
  iconColor?: string;
  /** Action buttons rendered left-to-right in the footer. */
  actions: M3DialogAction[];
  /** Called when the scrim (backdrop) is clicked. Omit to disable scrim-dismiss. */
  onScrimClick?: () => void;
}

// ─── Component ──────────────────────────────────────────────────────────────

export const M3Dialog: React.FC<M3DialogProps> = ({
  open,
  title,
  message,
  icon,
  iconColor = 'bg-amber-500/15 text-amber-500',
  actions,
  onScrimClick,
}) => {
  if (!open) return null;

  const iconNode =
    icon === null
      ? null
      : icon ?? (
          <div className={`p-2 rounded-lg flex-shrink-0 ${iconColor}`}>
            <ShieldAlert className="w-5 h-5" />
          </div>
        );

  return (
    <div
      className="fixed inset-0 z-[60] flex items-center justify-center bg-black/50 backdrop-blur-sm"
      onClick={(e) => {
        if (e.target === e.currentTarget) onScrimClick?.();
      }}
    >
      <M3Card
        variant="elevated"
        className="p-6 max-w-sm w-full mx-4 border border-m3-outline/30 shadow-2xl space-y-4 animate-fadeIn"
      >
        <div className="flex items-start gap-3">
          {iconNode}
          <div>
            <h3 className="text-sm font-semibold text-m3-on-surface">{title}</h3>
            {message && (
              <p className="text-xs text-m3-secondary mt-1 leading-relaxed">{message}</p>
            )}
          </div>
        </div>

        {actions.length > 0 && (
          <div className="flex gap-2.5 pt-1">
            {actions.map((action) => (
              <M3Button
                key={action.label}
                variant={action.variant ?? 'outlined'}
                onClick={action.onClick}
                className={`flex-1 ${action.className ?? ''}`}
              >
                {action.label}
              </M3Button>
            ))}
          </div>
        )}
      </M3Card>
    </div>
  );
};
