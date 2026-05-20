import React from 'react';

interface M3ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'filled' | 'tonal' | 'outlined' | 'text' | 'fab';
  icon?: React.ReactNode;
  iconPosition?: 'left' | 'right';
  loading?: boolean;
}

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
  const baseStyle = 'inline-flex items-center justify-center font-semibold text-xs tracking-wider uppercase transition-all duration-300 active:scale-95 disabled:pointer-events-none disabled:opacity-50 select-none';

  const variants = {
    filled: 'bg-m3-primary text-m3-on-primary hover:bg-m3-primary/90 hover:shadow-lg hover:shadow-m3-primary/20 rounded-full px-6 py-3.5',
    tonal: 'bg-m3-primary-container text-m3-on-primary-container hover:bg-m3-primary-container/80 rounded-full px-6 py-3.5',
    outlined: 'border border-m3-outline text-m3-primary hover:bg-m3-primary/5 dark:hover:bg-m3-primary/10 rounded-full px-6 py-3.5',
    text: 'text-m3-primary hover:bg-m3-primary/5 rounded-full px-4 py-3',
    fab: 'bg-m3-primary-container text-m3-on-primary-container hover:shadow-xl hover:shadow-m3-primary/25 rounded-2xl p-4 elevation-3 hover:-translate-y-0.5',
  };

  const loadingSpinner = (
    <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-current" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
      <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
      <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
    </svg>
  );

  return (
    <button
      disabled={disabled || loading}
      className={`${baseStyle} ${variants[variant]} ${className}`}
      {...props}
    >
      {loading && loadingSpinner}
      {!loading && icon && iconPosition === 'left' && <span className="mr-2 flex items-center">{icon}</span>}
      {children}
      {!loading && icon && iconPosition === 'right' && <span className="ml-2 flex items-center">{icon}</span>}
    </button>
  );
};
