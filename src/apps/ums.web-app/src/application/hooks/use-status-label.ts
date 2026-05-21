/**
 * useStatusLabel — shared i18n-aware status label resolver.
 *
 * H-4: Uses TenantStatusSchema enum values instead of hardcoded strings.
 */
import { useI18n } from '@app/i18n/use-i18n';
import { TenantStatusSchema } from '@domain/identity/schemas/tenant.schema';

const STATUS_KEY_MAP: Record<string, keyof ReturnType<typeof useI18n>> = {
  [TenantStatusSchema.enum.Active]: 'active',
  [TenantStatusSchema.enum.Suspended]: 'suspended',
  [TenantStatusSchema.enum.Pending]: 'pending',
};

export const useStatusLabel = () => {
  const t = useI18n();
  return (status: string): string => {
    const key = STATUS_KEY_MAP[status];
    return key ? (t[key] as string) : t.pending;
  };
};
