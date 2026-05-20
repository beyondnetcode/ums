import React from 'react';

interface M3TextFieldProps extends React.InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
  helperText?: string;
  icon?: React.ReactNode;
  iconPosition?: 'start' | 'end';
}

export const M3TextField: React.FC<M3TextFieldProps> = ({
  label,
  error,
  helperText,
  icon,
  iconPosition = 'start',
  className = '',
  id,
  ...props
}) => {
  const inputId = id || `m3-input-${Math.random().toString(36).substring(2, 9)}`;

  return (
    <div className={`flex flex-col w-full relative mb-4 ${className}`}>
      <label
        htmlFor={inputId}
        className="block text-[11px] font-bold text-m3-primary dark:text-m3-primary/80 uppercase tracking-wider mb-2 ml-1"
      >
        {label}
      </label>
      
      <div className="relative flex items-center">
        {icon && iconPosition === 'start' && (
          <div className="absolute left-4 text-m3-secondary pointer-events-none flex items-center">
            {icon}
          </div>
        )}

        <input
          id={inputId}
          className={`w-full px-4 py-3.5 text-sm rounded-2xl border bg-m3-surface-container/40 dark:bg-m3-surface-container/20 text-m3-on-surface focus:outline-none focus:ring-2 transition-all duration-300 ${
            error
              ? 'border-m3-error focus:border-m3-error focus:ring-m3-error/20'
              : 'border-m3-outline focus:border-m3-primary focus:ring-m3-primary/20'
          } ${icon && iconPosition === 'start' ? 'pl-11' : ''} ${
            icon && iconPosition === 'end' ? 'pr-11' : ''
          }`}
          {...props}
        />

        {icon && iconPosition === 'end' && (
          <div className="absolute right-4 text-m3-secondary pointer-events-none flex items-center">
            {icon}
          </div>
        )}
      </div>

      {(error || helperText) && (
        <span className={`text-[10px] mt-1.5 ml-2 font-medium tracking-wide ${error ? 'text-m3-error' : 'text-m3-secondary/75'}`}>
          {error || helperText}
        </span>
      )}
    </div>
  );
};
