import { useEffect, useRef, useCallback } from 'react';

const FOCUSABLE_SELECTORS = [
  'a[href]',
  'button:not([disabled])',
  'input:not([disabled])',
  'select:not([disabled])',
  'textarea:not([disabled])',
  '[tabindex]:not([tabindex="-1"])',
  'area[href]',
  'iframe',
  'object',
  'embed',
  '[contenteditable]',
  'audio[controls]',
  'video[controls]',
  'summary',
].join(', ');

/**
 * useFocusTrap — traps focus inside a container and restores it on unmount.
 *
 * Usage:
 *   const { containerRef } = useFocusTrap({ active: isOpen, onEscape: onClose });
 *   <div ref={containerRef}>...</div>
 */
export function useFocusTrap({
  active = true,
  onEscape,
}: {
  active?: boolean;
  onEscape?: () => void;
} = {}) {
  const containerRef = useRef<HTMLDivElement | null>(null);
  const previousActiveElementRef = useRef<Element | null>(null);

  const trapFocus = useCallback((e: KeyboardEvent) => {
    if (!containerRef.current || !active) return;

    if (e.key === 'Escape') {
      onEscape?.();
      return;
    }

    if (e.key !== 'Tab') return;

    const focusable = Array.from(
      containerRef.current.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTORS)
    ).filter((el) => {
      return !el.hasAttribute('disabled') && el.offsetParent !== null;
    });

    if (focusable.length === 0) return;

    const firstFocusable = focusable[0];
    const lastFocusable = focusable[focusable.length - 1];

    if (e.shiftKey) {
      if (document.activeElement === firstFocusable) {
        e.preventDefault();
        lastFocusable.focus();
      }
    } else {
      if (document.activeElement === lastFocusable) {
        e.preventDefault();
        firstFocusable.focus();
      }
    }
  }, [active, onEscape]);

  useEffect(() => {
    if (!active) return;

    previousActiveElementRef.current = document.activeElement;

    const container = containerRef.current;
    if (!container) return;

    const focusable = container.querySelector<HTMLElement>(FOCUSABLE_SELECTORS);
    if (focusable) {
      focusable.focus();
    } else {
      container.setAttribute('tabindex', '-1');
      container.focus();
    }

    document.addEventListener('keydown', trapFocus, true);

    return () => {
      document.removeEventListener('keydown', trapFocus, true);
      if (
        previousActiveElementRef.current &&
        previousActiveElementRef.current instanceof HTMLElement
      ) {
        previousActiveElementRef.current.focus();
      }
    };
  }, [active, trapFocus]);

  return { containerRef };
}
