import React from 'react';
import { ChevronDown } from 'lucide-react';

interface M3SelectProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  label: string;
  error?: string;
  helperText?: string;
}

export const M3Select: React.FC<M3SelectProps> = ({
  label,
  error,
  helperText,
  className = '',
  id,
  children,
  ...props
}) => {
  const selectId = id || `m3-sel-${label.toLowerCase().replace(/\W+/g, '-')}`;
  const hasMbClass = /\bmb-/.test(className);

  return (
    <div className={`relative w-full ${hasMbClass ? '' : 'mb-4'} ${className}`}>
      <div className="relative">
        <select
          id={selectId}
          className={`
            peer w-full h-14 pl-4 pr-10 pt-5 pb-1 text-sm
            bg-m3-surface-container/30 dark:bg-m3-surface-container/20
            text-m3-on-surface
            rounded-[4px] border-[1.5px]
            focus:outline-none transition-colors duration-200
            appearance-none cursor-pointer
            disabled:opacity-50 disabled:cursor-not-allowed
            ${error
              ? 'border-m3-error focus:border-m3-error'
              : 'border-m3-outline hover:border-m3-on-surface focus:border-m3-primary'
            }
          `}
          {...props}
        >
          {children}
        </select>

        <label
          htmlFor={selectId}
          className={`
            absolute left-4 px-1 -mx-1
            origin-[0] pointer-events-none
            text-xs font-normal
            bg-m3-surface
            top-1 -translate-y-full scale-75
            transition-colors duration-200
            ${error ? 'text-m3-error' : 'text-m3-primary peer-focus:text-m3-primary'}
          `}
        >
          {label}
        </label>

        <div className="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none text-m3-secondary">
          <ChevronDown className="w-4 h-4" />
        </div>
      </div>

      {(error || helperText) && (
        <span className={`block text-xs mt-1 ml-1 ${error ? 'text-m3-error' : 'text-m3-secondary/75'}`}>
          {error || helperText}
        </span>
      )}
    </div>
  );
};
