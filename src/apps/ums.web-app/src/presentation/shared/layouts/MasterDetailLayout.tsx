import React, { useRef, useState, useCallback } from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';

// ─── Types ──────────────────────────────────────────────────────────────────

export interface MasterDetailLayoutProps {
  /** Left (master) panel content. */
  master: React.ReactNode;
  /** Right (detail) panel content. */
  detail: React.ReactNode;
  /**
   * Initial width of the detail panel as a % of the container.
   * @default 40
   */
  initialDetailPct?: number;
  /** Minimum detail width as % of the container. @default 18 */
  minDetailPct?: number;
  /** Maximum detail width as % of the container. @default 72 */
  maxDetailPct?: number;
  /** Accessible label for the splitter handle. */
  splitterLabel?: string;
  /** Extra content rendered outside the panels (e.g. a floating dialog / drawer). */
  overlay?: React.ReactNode;
}

// ─── Component ──────────────────────────────────────────────────────────────

export const MasterDetailLayout: React.FC<MasterDetailLayoutProps> = ({
  master,
  detail,
  initialDetailPct = 40,
  minDetailPct = 18,
  maxDetailPct = 72,
  splitterLabel = 'Resize detail panel',
  overlay,
}) => {
  // ── Splitter state ──────────────────────────────────────────────────────────
  const [rightPct, setRightPct]                   = useState(initialDetailPct);
  const [isRightCollapsed, setIsRightCollapsed]   = useState(false);
  const [isDragging, setIsDragging]               = useState(false);
  const containerRef  = useRef<HTMLDivElement>(null);
  const isDraggingRef = useRef(false);
  const prevRightPct  = useRef(initialDetailPct);

  // ── Drag ────────────────────────────────────────────────────────────────────

  const handleSplitterMouseDown = useCallback((e: React.MouseEvent) => {
    e.preventDefault();
    isDraggingRef.current = true;
    setIsDragging(true);

    const onMouseMove = (ev: MouseEvent) => {
      if (!isDraggingRef.current || !containerRef.current) return;
      const rect = containerRef.current.getBoundingClientRect();
      const fromRight = rect.right - ev.clientX;
      const pct = Math.min(maxDetailPct, Math.max(minDetailPct, (fromRight / rect.width) * 100));
      setRightPct(pct);
      prevRightPct.current = pct;
      setIsRightCollapsed(false);
    };

    const onMouseUp = () => {
      isDraggingRef.current = false;
      setIsDragging(false);
      window.removeEventListener('mousemove', onMouseMove);
      window.removeEventListener('mouseup', onMouseUp);
    };

    window.addEventListener('mousemove', onMouseMove);
    window.addEventListener('mouseup', onMouseUp);
  }, [minDetailPct, maxDetailPct]);

  // ── Toggle ──────────────────────────────────────────────────────────────────

  const toggleRightPanel = useCallback(() => {
    if (isRightCollapsed) {
      setRightPct(prevRightPct.current);
      setIsRightCollapsed(false);
    } else {
      prevRightPct.current = rightPct;
      setIsRightCollapsed(true);
    }
  }, [isRightCollapsed, rightPct]);

  // ── Keyboard ────────────────────────────────────────────────────────────────

  const handleSplitterKeyDown = useCallback(
    (e: React.KeyboardEvent<HTMLDivElement>) => {
      if (e.key === 'Enter' || e.key === ' ') {
        e.preventDefault();
        toggleRightPanel();
        return;
      }
      if (e.key !== 'ArrowLeft' && e.key !== 'ArrowRight') return;
      e.preventDefault();
      const delta = e.key === 'ArrowLeft' ? 4 : -4;
      setRightPct((current) => {
        const next = Math.min(maxDetailPct, Math.max(minDetailPct, current + delta));
        prevRightPct.current = next;
        return next;
      });
      setIsRightCollapsed(false);
    },
    [toggleRightPanel, minDetailPct, maxDetailPct],
  );

  // ── Render ──────────────────────────────────────────────────────────────────

  return (
    <div
      ref={containerRef}
      className={`flex flex-1 min-h-0 h-full${isDragging ? ' select-none cursor-col-resize' : ''}`}
    >
      {/* Overlay slot (dialogs, drawers, etc.) */}
      {overlay}

      {/* ── Master (left) ── */}
      <div
        style={{ width: isRightCollapsed ? '100%' : `calc(100% - ${rightPct}% - 6px)` }}
        className={`flex flex-col min-w-0 overflow-hidden${!isDragging ? ' transition-[width] duration-200 ease-in-out' : ''}`}
      >
        {master}
      </div>

      {/* ── Splitter handle ── */}
      <div
        onMouseDown={handleSplitterMouseDown}
        onKeyDown={handleSplitterKeyDown}
        role="separator"
        tabIndex={0}
        aria-orientation="vertical"
        aria-label={splitterLabel}
        aria-valuemin={minDetailPct}
        aria-valuemax={maxDetailPct}
        aria-valuenow={Math.round(rightPct)}
        className={[
          'relative flex-shrink-0 w-1.5 flex flex-col items-center justify-center z-10',
          'cursor-col-resize group focus:outline-none focus-visible:ring-2 focus-visible:ring-m3-primary',
          isDragging ? 'bg-m3-primary/15' : 'hover:bg-m3-primary/10 transition-colors duration-150',
        ].join(' ')}
      >
        {/* Track line */}
        <div
          className={[
            'w-px flex-1 transition-colors duration-150',
            isDragging ? 'bg-m3-primary/70' : 'bg-m3-outline/25 group-hover:bg-m3-primary/35',
          ].join(' ')}
        />
        {/* Collapse / expand pill */}
        <button
          type="button"
          title={isRightCollapsed ? 'Expand detail panel' : 'Collapse detail panel'}
          onClick={(e) => { e.stopPropagation(); toggleRightPanel(); }}
          onMouseDown={(e) => e.stopPropagation()}
          className={[
            'absolute top-1/2 -translate-y-1/2 -translate-x-px',
            'w-4 h-10 rounded-full flex items-center justify-center',
            'border shadow-sm transition-all duration-150',
            isDragging
              ? 'bg-m3-primary text-white border-m3-primary'
              : 'bg-m3-surface-container border-m3-outline/40 text-m3-secondary',
            !isDragging && 'hover:bg-m3-primary/10 hover:border-m3-primary/50 hover:text-m3-primary',
          ].join(' ')}
        >
          {isRightCollapsed
            ? <ChevronLeft  className="w-2.5 h-2.5" />
            : <ChevronRight className="w-2.5 h-2.5" />
          }
        </button>
      </div>

      {/* ── Detail (right) ── */}
      <div
        style={{ width: isRightCollapsed ? '0' : `${rightPct}%` }}
        className={[
          'flex flex-col min-w-0 overflow-y-auto space-y-4 pr-0.5',
          !isDragging ? 'transition-[width] duration-200 ease-in-out' : '',
          isRightCollapsed ? 'overflow-hidden opacity-0 pointer-events-none' : '',
        ].join(' ')}
      >
        {detail}
      </div>
    </div>
  );
};
