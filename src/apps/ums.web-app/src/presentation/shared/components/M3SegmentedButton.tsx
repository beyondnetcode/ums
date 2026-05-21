import React from 'react';

/**
 * M3SegmentedButton — single-select segmented control.
 *
 * Renders a horizontal row of mutually-exclusive options following
 * Material Design 3 segmented button patterns. Replaces inline
 * button-group implementations across data views and forms.
 */

export interface SegmentOption<T extends string = string> {
  /** The value used for selection tracking. */
  value: T;
  /** Display label (text or icon). */
  label: React.ReactNode;
}

export interface M3SegmentedButtonProps<T extends string = string> {
  /** Available options. */
  options: SegmentOption<T>[];
  /** Currently selected value. */
  value: T;
  /** Called when the user selects an option. */
  onChange: (value: T) => void;
  /** Size variant. @default "md" */
  size?: 'sm' | 'md';
  /** Extra Tailwind classes on the container. */
  className?: string;
}

const SIZE_CLASSES: Record<'sm' | 'md', string> = {
  sm: 'p-1.5 text-xs',
  md: 'p-2 text-sm',
} as const;

export function M3SegmentedButton<T extends string = string>({
  options,
  value,
  onChange,
  size = 'md',
  className = '',
}: M3SegmentedButtonProps<T>) {
  return (
    <div
      className={`flex items-center gap-1 bg-m3-surface-container rounded-lg p-1 border border-m3-outline/25 ${className}`}
    >
      {options.map((opt) => {
        const isActive = opt.value === value;
        return (
          <button
            key={opt.value}
            type="button"
            onClick={() => onChange(opt.value)}
            className={[
              `${SIZE_CLASSES[size]} rounded-md transition-all duration-200 flex items-center justify-center`,
              isActive
                ? 'bg-m3-primary text-m3-on-primary shadow-sm'
                : 'text-m3-secondary hover:bg-m3-primary/10',
            ].join(' ')}
          >
            {opt.label}
          </button>
        );
      })}
    </div>
  );
}
