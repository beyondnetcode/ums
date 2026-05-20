import React, { useState, useRef, useEffect } from 'react';

interface TooltipProps {
  content: string;
  children: React.ReactElement;
  placement?: 'top' | 'bottom' | 'left' | 'right';
  delay?: number;
}

export const Tooltip: React.FC<TooltipProps> = ({
  content,
  children,
  placement = 'top',
  delay = 400,
}) => {
  const [visible, setVisible] = useState(false);
  const [coords, setCoords] = useState({ top: 0, left: 0 });
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const triggerRef = useRef<HTMLElement | null>(null);
  const tooltipRef = useRef<HTMLDivElement | null>(null);

  const show = () => {
    timerRef.current = setTimeout(() => setVisible(true), delay);
  };

  const hide = () => {
    if (timerRef.current) clearTimeout(timerRef.current);
    setVisible(false);
  };

  useEffect(() => {
    if (!visible || !triggerRef.current || !tooltipRef.current) return;
    const rect = triggerRef.current.getBoundingClientRect();
    const tip = tooltipRef.current.getBoundingClientRect();
    const gap = 6;
    let top = 0;
    let left = 0;

    if (placement === 'top') {
      top = rect.top - tip.height - gap;
      left = rect.left + rect.width / 2 - tip.width / 2;
    } else if (placement === 'bottom') {
      top = rect.bottom + gap;
      left = rect.left + rect.width / 2 - tip.width / 2;
    } else if (placement === 'left') {
      top = rect.top + rect.height / 2 - tip.height / 2;
      left = rect.left - tip.width - gap;
    } else {
      top = rect.top + rect.height / 2 - tip.height / 2;
      left = rect.right + gap;
    }

    // Clamp to viewport
    left = Math.max(6, Math.min(left, window.innerWidth - tip.width - 6));
    top = Math.max(6, Math.min(top, window.innerHeight - tip.height - 6));

    setCoords({ top, left });
  }, [visible, placement]);

  useEffect(() => () => { if (timerRef.current) clearTimeout(timerRef.current); }, []);

  if (!content) return children;

  const child = React.cloneElement(children, {
    ref: (el: HTMLElement) => {
      triggerRef.current = el;
      const originalRef = (children as any).ref;
      if (typeof originalRef === 'function') originalRef(el);
      else if (originalRef) originalRef.current = el;
    },
    onMouseEnter: (e: React.MouseEvent) => { show(); children.props.onMouseEnter?.(e); },
    onMouseLeave: (e: React.MouseEvent) => { hide(); children.props.onMouseLeave?.(e); },
    onFocus: (e: React.FocusEvent) => { show(); children.props.onFocus?.(e); },
    onBlur: (e: React.FocusEvent) => { hide(); children.props.onBlur?.(e); },
  });

  return (
    <>
      {child}
      {visible && typeof document !== 'undefined' && (
        <div
          ref={tooltipRef}
          role="tooltip"
          style={{ position: 'fixed', top: coords.top, left: coords.left, zIndex: 9999 }}
          className="pointer-events-none px-2.5 py-1.5 rounded-lg bg-m3-on-surface/90 text-m3-surface text-[10px] font-semibold tracking-wide shadow-lg whitespace-nowrap backdrop-blur-sm animate-tooltipIn"
        >
          {content}
        </div>
      )}
    </>
  );
};

interface IconButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  tooltip: string;
  placement?: 'top' | 'bottom' | 'left' | 'right';
  children: React.ReactNode;
}

export const IconButton: React.FC<IconButtonProps> = ({
  tooltip,
  placement = 'top',
  children,
  className = '',
  ...props
}) => {
  return (
    <Tooltip content={tooltip} placement={placement}>
      <button
        className={`p-1.5 rounded-full transition-all text-m3-secondary hover:text-m3-primary hover:bg-m3-primary/10 ${className}`}
        {...props}
      >
        {children}
      </button>
    </Tooltip>
  );
};
