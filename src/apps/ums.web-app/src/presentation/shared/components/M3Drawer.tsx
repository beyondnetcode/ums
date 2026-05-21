import React from 'react';
import { X } from 'lucide-react';

/**
 * M3Drawer — slide-out panel from the right edge.
 *
 * Extracted from NotificationCenter to provide a reusable container
 * for any right-side panel (audit log, settings, help, etc.).
 */

export interface M3DrawerProps {
  /** Controls visibility. */
  open: boolean;
  /** Called when the user dismisses the drawer (scrim click or close button). */
  onClose: () => void;
  /** Drawer title. */
  title: string;
  /** Optional subtitle shown below the title. */
  subtitle?: React.ReactNode;
  /** Optional action bar rendered between header and body. */
  actions?: React.ReactNode;
  /** Max width Tailwind class. @default "max-w-md" */
  maxWidth?: string;
  /** Scrollable body content. */
  children: React.ReactNode;
}

export const M3Drawer: React.FC<M3DrawerProps> = ({
  open,
  onClose,
  title,
  subtitle,
  actions,
  maxWidth = 'max-w-md',
  children,
}) => {
  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-hidden select-none">
      {/* Scrim */}
      <div
        className="absolute inset-0 bg-black/40 backdrop-blur-sm transition-opacity"
        onClick={onClose}
      />

      <div className="pointer-events-none fixed inset-y-0 right-0 flex max-w-full pl-10">
        <div className={`pointer-events-auto w-screen ${maxWidth}`}>
          <div className="flex h-full flex-col bg-m3-surface border-l border-m3-outline/25 shadow-2xl transition-all duration-300">
            {/* Header */}
            <div className="flex items-center justify-between px-6 py-5 border-b border-m3-outline/30">
              <div>
                <h2 className="text-base font-extrabold tracking-tight text-m3-on-surface">
                  {title}
                </h2>
                {subtitle && (
                  <div className="text-[10px] text-m3-primary font-bold tracking-wide uppercase mt-1">
                    {subtitle}
                  </div>
                )}
              </div>
              <button
                onClick={onClose}
                className="p-2 rounded-full hover:bg-m3-primary/10 text-m3-secondary transition-colors"
              >
                <X className="w-5 h-5" />
              </button>
            </div>

            {/* Actions bar */}
            {actions && (
              <div className="flex gap-2 px-6 py-3 bg-m3-surface-container/30 border-b border-m3-outline/10 text-xs">
                {actions}
              </div>
            )}

            {/* Scrollable body */}
            <div className="flex-1 overflow-y-auto p-6 space-y-4">
              {children}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
