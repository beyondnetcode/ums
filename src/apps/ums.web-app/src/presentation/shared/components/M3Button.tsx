import React from 'react';

interface M3ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'filled' | 'tonal' | 'outlined' | 'text' | 'fab';
  icon?: React.ReactNode;
  iconPosition?: 'left' | 'right';
  loading?: boolean;
}

/**
 * MD3 Button
 *
 * Spec references:
 *   • Height        40 dp  → h-10 (py-2.5 + text-sm line-height 20 px = 40 px)
 *   • Corner radius full pill for contained/outlined/text; 16dp for FAB
 *   • Horizontal pad 24 dp → px-6
 *   • Typography    Label Large — 14 sp / medium weight / +0.1 sp tracking
 *   • State layers  Hover 8 % on-color; Pressed 12 % on-color (approximated
 *                   via Tailwind opacity utilities on hover/active)
 */
export const M3Button: React.FC<M3ButtonProps> = ({
  children,
  variant = 'filled',
  icon,
  iconPosition = 'left',
  loading = false,
  className = '',
  disabled,
  ...props
}) => {
  const base =
    'inline-flex items-center justify-center font-medium text-sm ' +
    'tracking-[0.1px] transition-all duration-200 ' +
    'active:scale-[0.97] disabled:pointer-events-none disabled:opacity-38 ' +
    'select-none focus-visible:outline-none focus-visible:ring-2 ' +
    'focus-visible:ring-m3-primary focus-visible:ring-offset-1';

  const variants: Record<string, string> = {
    /**
     * Filled — highest emphasis.
     * Container: Primary; label: On-Primary
     * Hover: 8 % white over container → bg-primary/90 approximation
     */
    filled:
      'bg-m3-primary text-m3-on-primary ' +
      'hover:bg-m3-primary/90 hover:shadow-md hover:shadow-m3-primary/25 ' +
      'rounded-full px-6 py-2.5',

    /**
     * Tonal — medium emphasis.
     * Container: Secondary-Container; label: On-Secondary-Container
     */
    tonal:
      'bg-m3-primary-container text-m3-on-primary-container ' +
      'hover:bg-m3-primary-container/80 hover:shadow-sm ' +
      'rounded-full px-6 py-2.5',

    /**
     * Outlined — medium emphasis, no fill.
     * Border: Outline token; label: Primary
     */
    outlined:
      'border border-m3-outline text-m3-primary ' +
      'hover:bg-m3-primary/8 dark:hover:bg-m3-primary/12 ' +
      'rounded-full px-6 py-2.5',

    /**
     * Text — lowest emphasis.
     * No container, no border; label: Primary
     */
    text:
      'text-m3-primary ' +
      'hover:bg-m3-primary/8 dark:hover:bg-m3-primary/12 ' +
      'rounded-full px-3 py-2.5',

    /**
     * FAB — Floating Action Button.
     * Container: Primary-Container; icon only (no label padding)
     */
    fab:
      'bg-m3-primary-container text-m3-on-primary-container ' +
      'hover:shadow-xl hover:shadow-m3-primary/25 hover:-translate-y-0.5 ' +
      'rounded-2xl p-4 elevation-3',
  };

  const spinner = (
    <svg
      className="animate-spin -ml-1 mr-2 h-4 w-4 text-current"
      xmlns="http://www.w3.org/2000/svg"
      fill="none"
      viewBox="0 0 24 24"
    >
      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
      <path
        className="opacity-75"
        fill="currentColor"
        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
      />
    </svg>
  );

  return (
    <button
      disabled={disabled || loading}
      className={`${base} ${variants[variant]} ${className}`}
      {...props}
    >
      {loading && spinner}
      {!loading && icon && iconPosition === 'left'  && <span className="mr-2 flex items-center">{icon}</span>}
      {children}
      {!loading && icon && iconPosition === 'right' && <span className="ml-2 flex items-center">{icon}</span>}
    </button>
  );
};
