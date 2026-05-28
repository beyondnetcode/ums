import React from 'react';
import { M3Card } from '../M3Card';
import { Database, Plus, ChevronsUp, ChevronsDown } from 'lucide-react';
import { useDragResize } from '@app/hooks/use-drag-resize';

interface DataViewShellProps {
  title: string;
  subtitle?: string;
  onRegisterNew?: () => void;
  registerLabel?: string;
  headerIcon?: React.ReactNode;
  controls: React.ReactNode;
  content: React.ReactNode;
}

export const DataViewShell: React.FC<DataViewShellProps> = ({
  title,
  subtitle,
  onRegisterNew,
  registerLabel = 'New',
  headerIcon = <Database className="w-5 h-5 text-m3-primary flex-shrink-0" />,
  controls,
  content,
}) => {
  const {
    size: topPx,
    isCollapsed: isHeaderCollapsed,
    isDragging: isDraggingH,
    containerRef: dvContainerRef,
    resizableRef: searchZoneRef,
    handleMouseDown: handleHSplitterMouseDown,
    handleKeyDown: handleHSplitterKeyDown,
    toggleCollapse: toggleHeader,
  } = useDragResize();

  return (
    <div
      ref={dvContainerRef}
      className={`flex flex-col h-full${isDraggingH ? ' select-none cursor-row-resize' : ''}`}
    >
      {/* ZONE A — Title bar */}
      <M3Card
        variant="elevated"
        className="flex-shrink-0 border border-m3-outline/25 bg-m3-surface-container/20 shadow-sm"
      >
        <div className="px-5 py-4 flex items-center justify-between gap-3">
          <div className="min-w-0 flex-1 space-y-0.5">
            <h2 className="text-base font-semibold text-m3-on-surface flex items-center gap-2">
              {headerIcon}
              <span className="truncate">{title}</span>
            </h2>
            {subtitle && (
              <p className="text-xs text-m3-secondary font-normal pl-7 truncate">
                {subtitle}
              </p>
            )}
          </div>

          <div className="flex items-center gap-2 flex-shrink-0">
            {onRegisterNew && (
              <button
                type="button"
                onClick={onRegisterNew}
                className="inline-flex h-8 items-center gap-1.5 rounded-lg border border-m3-primary/25 bg-m3-primary/10 px-3 text-xs font-medium text-m3-primary transition-colors hover:border-m3-primary/45 hover:bg-m3-primary/15"
              >
                <Plus className="h-3.5 w-3.5" /> {registerLabel}
              </button>
            )}

            <button
              type="button"
              title={isHeaderCollapsed ? 'Show search & filters' : 'Hide search & filters'}
              onClick={toggleHeader}
              className={[
                'p-1.5 rounded-lg border transition-all duration-150',
                isHeaderCollapsed
                  ? 'bg-m3-primary/10 border-m3-primary/40 text-m3-primary'
                  : 'bg-m3-surface-container/60 border-m3-outline/40 text-m3-secondary hover:bg-m3-primary/10 hover:border-m3-primary/40 hover:text-m3-primary',
              ].join(' ')}
            >
              {isHeaderCollapsed ? <ChevronsDown className="w-3.5 h-3.5" /> : <ChevronsUp className="w-3.5 h-3.5" />}
            </button>
          </div>
        </div>
      </M3Card>

      {/* ZONE B — Search form + control row */}
      <div
        ref={searchZoneRef}
        style={topPx !== null ? { height: topPx } : undefined}
        className={[
          'flex flex-col gap-3 flex-shrink-0',
          topPx !== null ? 'overflow-hidden' : 'overflow-visible',
          topPx !== null && !isDraggingH ? 'transition-[height] duration-200 ease-in-out' : '',
        ].join(' ')}
      >
        {controls}
      </div>

      {/* SPLITTER */}
      <div
        onMouseDown={handleHSplitterMouseDown}
        onKeyDown={handleHSplitterKeyDown}
        role="separator"
        tabIndex={0}
        aria-orientation="horizontal"
        className={[
          'relative flex-shrink-0 h-1.5 w-full flex items-center justify-center group cursor-row-resize z-10 mt-1',
          isDraggingH ? 'bg-m3-primary/20' : 'hover:bg-m3-primary/10 transition-colors duration-150',
        ].join(' ')}
      >
        <div className={['h-px w-full transition-colors duration-150', isDraggingH ? 'bg-m3-primary/60' : 'bg-m3-outline/30 group-hover:bg-m3-primary/40'].join(' ')} />
        <button
          type="button"
          onClick={(e) => { e.stopPropagation(); toggleHeader(); }}
          onMouseDown={(e) => e.stopPropagation()}
          className={[
            'absolute left-1/2 -translate-x-1/2 -translate-y-px',
            'h-4 w-12 rounded-full flex items-center justify-center gap-0.5',
            'border shadow-sm transition-all duration-150 text-[10px] font-medium',
            isDraggingH
              ? 'bg-m3-primary text-white border-m3-primary'
              : isHeaderCollapsed
                ? 'bg-m3-primary/15 border-m3-primary/50 text-m3-primary hover:bg-m3-primary/25'
                : 'bg-m3-surface-container border-m3-outline/50 text-m3-secondary hover:bg-m3-primary/10 hover:border-m3-primary/40 hover:text-m3-primary',
          ].join(' ')}
        >
          {isHeaderCollapsed ? <ChevronsDown className="w-3 h-3" /> : <ChevronsUp className="w-3 h-3" />}
        </button>
      </div>

      {/* ZONE C — Content */}
      <div className="flex-1 flex flex-col gap-3 min-h-0 overflow-hidden pt-1">
        {content}
      </div>
    </div>
  );
};
