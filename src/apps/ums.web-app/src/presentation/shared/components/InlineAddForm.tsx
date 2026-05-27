import React from 'react';
import { Plus, X } from 'lucide-react';
import { M3Card } from './M3Card';
import { M3Button } from './M3Button';
import { IconButton } from './Tooltip';

/**
 * InlineAddForm — collapsible card for adding new entities inline.
 *
 * When closed, renders a tonal "Add" button (or a quiet custom pill button).
 * When open, renders an outlined card with header, children (form fields),
 * and a submit button aligned to the bottom right.
 */

export interface InlineAddFormProps {
  /** Whether the form is currently visible. */
  isOpen: boolean;
  /** Toggle visibility. */
  onToggle: (open: boolean) => void;
  /** Form submit handler (receives the form event). */
  onSubmit: (e: React.FormEvent) => void;
  /** Label shown on the closed-state "Add" button. */
  addLabel: string;
  /** Title shown in the open-state header. */
  title: string;
  /** Cancel tooltip label. */
  cancelLabel?: string;
  /** Submit button label. */
  submitLabel: string;
  /** Whether the submit is in progress. */
  isLoading?: boolean;
  /** Optional icon next to the title (defaults to Plus). */
  icon?: React.ReactNode;
  /** Optional error message displayed above the form fields. */
  error?: string;
  /** Form field children. */
  children: React.ReactNode;
  /** Visual emphasis for the closed-state trigger button. */
  triggerEmphasis?: 'normal' | 'quiet';
  /** Whether the closed-state trigger should span the full width. */
  fullWidth?: boolean;
}

export const InlineAddForm: React.FC<InlineAddFormProps> = ({
  isOpen,
  onToggle,
  onSubmit,
  addLabel,
  title,
  cancelLabel = 'Cancelar',
  submitLabel,
  isLoading = false,
  icon,
  error,
  children,
  triggerEmphasis = 'normal',
  fullWidth = false,
}) => {
  if (!isOpen) {
    if (triggerEmphasis === 'quiet') {
      return (
        <div className="flex justify-start my-1 animate-fadeIn">
          <button
            type="button"
            onClick={() => onToggle(true)}
            className="inline-flex items-center gap-1.5 px-3 py-1 h-7 rounded-full text-[10px] font-bold uppercase tracking-wider text-m3-primary border border-m3-primary/20 bg-m3-primary/5 hover:bg-m3-primary/10 hover:border-m3-primary/30 transition-all select-none hover:shadow-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-m3-primary active:scale-[0.97]"
          >
            <Plus className="w-3 h-3 text-m3-primary" />
            <span>{addLabel}</span>
          </button>
        </div>
      );
    }

    return (
      <div className={`flex ${fullWidth ? 'w-full' : 'justify-start'} my-1 animate-fadeIn`}>
        <M3Button
          variant="tonal"
          onClick={() => onToggle(true)}
          className={`${
            fullWidth ? 'w-full' : ''
          } flex items-center justify-center gap-1.5 py-1.5 px-4 h-8 text-[10px] font-semibold border border-m3-primary/10 hover:border-m3-primary/30`}
        >
          <Plus className="w-3.5 h-3.5 text-m3-primary" /> {addLabel}
        </M3Button>
      </div>
    );
  }

  return (
    <M3Card
      variant="outlined"
      className="p-[18px] rounded-2xl bg-m3-surface-container/20 border-m3-primary/25 animate-fadeIn"
    >
      <div className="flex justify-between items-center border-b border-m3-outline/15 pb-2 mb-3">
        <h4 className="text-[10px] font-semibold text-m3-primary flex items-center gap-1">
          {icon ?? <Plus className="w-3.5 h-3.5" />} {title}
        </h4>
        <IconButton
          tooltip={cancelLabel}
          onClick={() => onToggle(false)}
        >
          <X className="w-3.5 h-3.5" />
        </IconButton>
      </div>

      {error && (
        <div className="p-2.5 bg-m3-error/15 border border-m3-error/20 text-m3-error rounded-xl text-[9px] font-bold mb-3">
          {error}
        </div>
      )}

      <form onSubmit={onSubmit} className="space-y-3">
        {children}
        <div className="flex justify-end gap-2 pt-1">
          <M3Button
            variant="text"
            type="button"
            onClick={() => onToggle(false)}
            className="text-[11px] h-8 px-4 font-semibold"
          >
            {cancelLabel}
          </M3Button>
          <M3Button
            variant="filled"
            type="submit"
            className="text-[11px] h-8 px-4 font-semibold shadow-sm"
            loading={isLoading}
          >
            {submitLabel}
          </M3Button>
        </div>
      </form>
    </M3Card>
  );
};
