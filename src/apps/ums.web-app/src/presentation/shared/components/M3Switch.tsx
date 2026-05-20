import React from 'react';

interface M3SwitchProps {
  checked: boolean;
  onChange: (checked: boolean) => void;
  label?: string;
  disabled?: boolean;
  id?: string;
}

export const M3Switch: React.FC<M3SwitchProps> = ({
  checked,
  onChange,
  label,
  disabled = false,
  id,
}) => {
  const switchId = id || `m3-switch-${Math.random().toString(36).substring(2, 9)}`;

  const handleToggle = () => {
    if (!disabled) {
      onChange(!checked);
    }
  };

  return (
    <div className="flex items-center gap-3 select-none">
      <button
        id={switchId}
        type="button"
        role="switch"
        aria-checked={checked}
        disabled={disabled}
        onClick={handleToggle}
        className={`w-12 h-7 rounded-full flex items-center p-1 cursor-pointer transition-colors duration-300 disabled:opacity-50 disabled:pointer-events-none ${
          checked ? 'bg-m3-primary' : 'bg-m3-outline/40 border-2 border-m3-outline'
        }`}
      >
        <span
          className={`w-5 h-5 rounded-full shadow transition-all duration-300 transform ${
            checked
              ? 'translate-x-5 bg-m3-on-primary scale-110'
              : 'translate-x-0 bg-m3-secondary hover:bg-m3-primary/80'
          }`}
        />
      </button>
      {label && (
        <label htmlFor={switchId} className="text-xs font-bold uppercase tracking-wider text-m3-secondary cursor-pointer">
          {label}
        </label>
      )}
    </div>
  );
};
