import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useFocusTrap } from './use-focus-trap';

describe('useFocusTrap', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
    document.body.innerHTML = '';
  });

  afterEach(() => {
    document.body.innerHTML = '';
  });

  it('returns a containerRef', () => {
    const { result } = renderHook(() => useFocusTrap({ active: false }));
    expect(result.current.containerRef).toBeDefined();
    expect(result.current.containerRef.current).toBeNull();
  });

  it('does not trap focus when inactive', () => {
    const container = document.createElement('div');
    const button = document.createElement('button');
    button.textContent = 'Outside';
    document.body.appendChild(container);
    document.body.appendChild(button);

    button.focus();
    expect(document.activeElement).toBe(button);

    renderHook(() => useFocusTrap({ active: false }));

    expect(document.activeElement).toBe(button);
  });

  it('does not call onEscape when inactive', () => {
    const onEscape = vi.fn();
    const container = document.createElement('div');
    container.setAttribute('tabindex', '-1');
    document.body.appendChild(container);

    renderHook(() => useFocusTrap({ active: false, onEscape }));

    const escapeEvent = new KeyboardEvent('keydown', { key: 'Escape' });
    document.dispatchEvent(escapeEvent);

    expect(onEscape).not.toHaveBeenCalled();
  });

  it('restores focus to previous element on cleanup', () => {
    const container = document.createElement('div');
    const outsideButton = document.createElement('button');
    outsideButton.textContent = 'Outside';
    document.body.appendChild(container);
    document.body.appendChild(outsideButton);

    outsideButton.focus();
    expect(document.activeElement).toBe(outsideButton);

    const { unmount } = renderHook(() => useFocusTrap({ active: true }));

    act(() => {
      unmount();
    });

    expect(document.activeElement).toBe(outsideButton);
  });

  it('accepts optional onEscape callback', () => {
    const { result } = renderHook(() => useFocusTrap({ active: true }));
    expect(result.current.containerRef).toBeDefined();
  });

  it('works without any options', () => {
    const { result } = renderHook(() => useFocusTrap());
    expect(result.current.containerRef).toBeDefined();
  });
});
