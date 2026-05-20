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
  // Extract placeholder (hint) and required before spreading to the <input>.
  // The input always receives placeholder=" " (single space) so that CSS
  // :placeholder-shown fires correctly for the floating label animation.
  // The caller's placeholder is rendered as a separate hint element that
  // appears only when the field is focused and still empty.
  const { placeholder: hint, required: req, ...inputProps } = props;

  const inputId = id || `m3-tf-${label.toLowerCase().replace(/\W+/g, '-')}`;
  // Don't add default bottom margin when the caller already supplies one.
  const hasMbClass = /\bmb-/.test(className);

  return (
    <div className={`relative w-full ${hasMbClass ? '' : 'mb-4'} ${className}`}>
      <div className="relative">
        {icon && iconPosition === 'start' && (
          <div className="absolute left-3 top-1/2 -translate-y-1/2 text-m3-secondary pointer-events-none z-10">
            {icon}
          </div>
        )}

        {/* Input — placeholder=" " (space) keeps :placeholder-shown working */}
        <input
          id={inputId}
          required={req}
          placeholder=" "
          className={`
            peer w-full h-14 px-4 pt-5 pb-2 text-sm
            bg-m3-surface-container/30 dark:bg-m3-surface-container/20
            text-m3-on-surface
            rounded-[4px] border-[1.5px]
            focus:outline-none transition-colors duration-200
            disabled:opacity-50 disabled:cursor-not-allowed
            ${error
              ? 'border-m3-error focus:border-m3-error'
              : 'border-m3-outline hover:border-m3-on-surface focus:border-m3-primary'
            }
            ${icon && iconPosition === 'start' ? 'pl-10' : ''}
            ${icon && iconPosition === 'end' ? 'pr-10' : ''}
          `}
          {...inputProps}
        />

        {/* Floating label */}
        <label
          htmlFor={inputId}
          className={`
            absolute left-4 px-1 -mx-1
            origin-[0_50%] pointer-events-none
            text-sm font-normal
            transition-all duration-200
            bg-m3-surface
            top-1/2 -translate-y-1/2 scale-100
            peer-focus:top-0 peer-focus:-translate-y-1/2 peer-focus:scale-75
            peer-[&:not(:placeholder-shown)]:top-0
            peer-[&:not(:placeholder-shown)]:-translate-y-1/2
            peer-[&:not(:placeholder-shown)]:scale-75
            ${error
              ? 'text-m3-error peer-focus:text-m3-error'
              : 'text-m3-secondary peer-focus:text-m3-primary'
            }
          `}
        >
          {label}{req && <span className="ml-0.5 text-m3-error">*</span>}
        </label>

        {/* Hint — visible only when focused and empty */}
        {hint && (
          <span
            className="
              absolute left-4 right-4 text-sm text-m3-secondary/50
              pointer-events-none select-none
              transition-opacity duration-200
              opacity-0 peer-focus:opacity-100
              peer-[&:not(:placeholder-shown)]:hidden
            "
            style={{ top: '60%', transform: 'translateY(-50%)' }}
          >
            {hint}
          </span>
        )}

        {icon && iconPosition === 'end' && (
          <div className="absolute right-3 top-1/2 -translate-y-1/2 text-m3-secondary pointer-events-none z-10">
            {icon}
          </div>
        )}
      </div>

      {(error || helperText) && (
        <span className={`block text-xs mt-1 ml-1 ${error ? 'text-m3-error' : 'text-m3-secondary/75'}`}>
          {error || helperText}
        </span>
      )}
    </div>
  );
};
