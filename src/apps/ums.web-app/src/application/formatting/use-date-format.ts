/**
 * useDateFormat — React hook that wraps date formatting with the active
 * locale (i18n store) and session timezone (auth store).
 *
 * ADR-0076: Use this hook in all components instead of calling formatDate/
 * formatDateTime directly, so that locale and timezone are always consistent
 * with the authenticated session without passing them as props.
 *
 * Usage:
 *   const { formatDate, formatDateTime, formatRelativeTime, timezone } = useDateFormat();
 *   <span>{formatDate(record.createdAt)}</span>
 */
import { useCallback } from 'react';
import { useI18nStore } from '@app/stores/i18n.store';
import { useAuthStore } from '@app/stores/auth.store';
import {
  formatDate as _formatDate,
  formatDateTime as _formatDateTime,
  formatRelativeTime as _formatRelativeTime,
} from './date';
import type { Intl as IntlType } from 'typescript';

export function useDateFormat() {
  const locale = useI18nStore(s => s.language);
  const timezone = useAuthStore(s => s.user?.sessionParameters?.defaultTimezone);

  const formatDate = useCallback(
    (date: Date | string | null | undefined, options?: Intl.DateTimeFormatOptions) =>
      _formatDate(date, locale, timezone, options),
    [locale, timezone]
  );

  const formatDateTime = useCallback(
    (date: Date | string | null | undefined, options?: Intl.DateTimeFormatOptions) =>
      _formatDateTime(date, locale, timezone, options),
    [locale, timezone]
  );

  const formatRelativeTime = useCallback(
    (date: Date | string | null | undefined) => _formatRelativeTime(date, locale),
    [locale]
  );

  return { formatDate, formatDateTime, formatRelativeTime, locale, timezone };
}
