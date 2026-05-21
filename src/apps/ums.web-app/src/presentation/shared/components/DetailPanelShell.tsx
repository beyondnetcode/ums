import React from 'react';
import { RefreshCw } from 'lucide-react';
import { M3Card } from './M3Card';
import { M3SegmentedButton } from './M3SegmentedButton';
import type { SegmentOption } from './M3SegmentedButton';

// ─── Types ──────────────────────────────────────────────────────────────────

export interface DetailTab<T extends string = string> {
  key: T;
  label: string;
  icon?: React.ReactNode;
}

export interface DetailPanelShellProps<T extends string = string> {
  // ── State ──────────────────────────────────────────────────────────────────
  isLoading: boolean;
  /** True when no entity is currently selected. */
  isEmpty: boolean;

  // ── Customisable labels ────────────────────────────────────────────────────
  loadingLabel?: string;
  emptyLabel?: string;

  // ── Profile / header section ───────────────────────────────────────────────
  /** Slot rendered above the tab bar (e.g. a profile card, summary strip). */
  header?: React.ReactNode;
  /** Optional informational banner between header and tabs. */
  banner?: React.ReactNode;

  // ── Tabs ───────────────────────────────────────────────────────────────────
  tabs: DetailTab<T>[];
  activeTab: T;
  onTabChange: (tab: T) => void;

  // ── Tab content ────────────────────────────────────────────────────────────
  /** Renders the body for the currently-active tab. */
  children: React.ReactNode;

  /** Unique key to reset animation when the selected entity changes. */
  entityKey?: string;
}

// ─── Component ──────────────────────────────────────────────────────────────

export function DetailPanelShell<T extends string = string>({
  isLoading,
  isEmpty,
  loadingLabel = 'Loading...',
  emptyLabel = 'Select an item to view details.',
  header,
  banner,
  tabs,
  activeTab,
  onTabChange,
  children,
  entityKey,
}: DetailPanelShellProps<T>) {
  if (isLoading) {
    return (
      <M3Card
        variant="elevated"
        className="py-24 text-center text-sm text-m3-secondary border border-m3-outline/20"
      >
        <RefreshCw className="w-8 h-8 animate-spin text-m3-primary mx-auto mb-3" />
        {loadingLabel}
      </M3Card>
    );
  }

  if (isEmpty) {
    return (
      <M3Card
        variant="elevated"
        className="p-6 text-center text-sm text-m3-secondary border border-m3-outline/20 bg-m3-surface-container/10"
      >
        {emptyLabel}
      </M3Card>
    );
  }

  return (
    <div key={entityKey} className="space-y-4">
      {/* Profile / header slot */}
      {header}

      {/* Optional informational banner */}
      {banner}

      {/* Tab bar */}
      {tabs.length > 0 && (
        <M3SegmentedButton
          options={tabs.map((tab): SegmentOption<T> => ({
            value: tab.key,
            label: <span className="flex items-center gap-1.5">{tab.icon} {tab.label}</span>,
          }))}
          value={activeTab}
          onChange={onTabChange}
          size="md"
        />
      )}

      {/* Tab content */}
      <M3Card
        variant="elevated"
        className="p-5 border border-m3-outline/25 bg-m3-surface-container/20 shadow-sm"
      >
        {children}
      </M3Card>
    </div>
  );
}
