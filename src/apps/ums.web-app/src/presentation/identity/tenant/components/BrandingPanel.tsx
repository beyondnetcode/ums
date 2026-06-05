import React, { useEffect, useCallback } from 'react';
import { useI18n } from '@app/i18n/use-i18n';
import { useFormFields } from '@app/hooks/use-form-fields';
import { useResetOnChange } from '@app/hooks/use-reset-on-change';
import { useNotificationStore } from '@app/stores/notification.store';
import { brandingService } from '@infra/identity/services/branding.service';
import type { Branding } from '@domain/identity/schemas/branding.schema';
import { M3Button } from '@shared/components/M3Button';
import { M3TextField } from '@shared/components/M3TextField';
import { M3Select } from '@shared/components/M3Select';
import { M3Switch } from '@shared/components/M3Switch';
import { M3FieldsetWrapper } from '@shared/components/M3FieldsetWrapper';
import { StatusBadge, StatusBadgeColorSet } from '@shared/components/StatusBadge';
import { SectionHeader } from '@shared/components/SectionHeader';
import { Tooltip } from '@shared/components/Tooltip';

const DNS_COLOR_MAP: Record<string, StatusBadgeColorSet> = {
  Verified: { bg: 'bg-emerald-500/10', border: 'border-emerald-500/20', text: 'text-emerald-500' },
  Pending: { bg: 'bg-amber-500/10', border: 'border-amber-500/20', text: 'text-amber-500' },
  Failed: { bg: 'bg-rose-500/10', border: 'border-rose-500/20', text: 'text-rose-500' },
};

interface BrandingPanelProps {
  tenantId: string;
  isRootTenant: boolean;
}

export const BrandingPanel: React.FC<BrandingPanelProps> = ({ tenantId, isRootTenant }) => {
  const t = useI18n();
  const addNotification = useNotificationStore(s => s.addNotification);

  const [branding, setBranding] = React.useState<Branding | null>(null);
  const [isLoading, setIsLoading] = React.useState(false);

  const { fields, setField, setFields } = useFormFields({
    headlineText: '',
    secondaryText: '',
    primaryButtonLabel: '',
    footerText: '',
    primaryColor: '#3b5bdb',
    backgroundStyle: 'SolidColor',
    logo: '',
    logoFormat: 'Png',
    customDomain: '',
    magicLinkFallbackEnabled: false,
  });

  const loadBranding = useCallback(async () => {
    if (!tenantId) return;
    setIsLoading(true);
    try {
      const data = await brandingService.getBranding(tenantId);
      if (data) {
        setBranding(data);
        setFields({
          headlineText: data.headlineText,
          secondaryText: data.secondaryText,
          primaryButtonLabel: data.primaryButtonLabel,
          footerText: data.footerText,
          primaryColor: data.primaryColor,
          backgroundStyle: data.backgroundStyle,
          logo: data.logo,
          logoFormat: data.logoFormat,
          customDomain: data.customDomain ?? '',
          magicLinkFallbackEnabled: data.magicLinkFallbackEnabled,
        });
      }
    } catch {
      addNotification({ title: t.error, message: t.notifBrandingLoadFailed, type: 'error' });
    } finally {
      setIsLoading(false);
    }
  }, [tenantId, addNotification, t, setFields]);

  useEffect(() => {
    loadBranding();
  }, [loadBranding]);

  useResetOnChange(tenantId, () => {
    setBranding(null);
  });

  const handleUpdateBranding = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const payload = {
        logo: fields.logo.trim(),
        logoFormat: fields.logoFormat,
        primaryColor: fields.primaryColor,
        backgroundStyle: fields.backgroundStyle,
        headlineText: fields.headlineText.trim(),
        secondaryText: fields.secondaryText.trim(),
        primaryButtonLabel: fields.primaryButtonLabel.trim(),
        footerText: fields.footerText.trim(),
        customDomain: fields.customDomain.trim() || undefined,
        magicLinkFallbackEnabled: fields.magicLinkFallbackEnabled,
      };

      if (branding) {
        await brandingService.updateBranding(tenantId, payload);
      } else {
        await brandingService.setBranding(tenantId, payload);
      }

      addNotification({
        title: t.notifBrandingApplied,
        message: t.notifBrandingMsg(fields.primaryColor),
        type: 'success',
      });
      await loadBranding();
    } catch {
      addNotification({ title: t.error, message: t.notifBrandingSaveFailed, type: 'error' });
    }
  };

  if (!isRootTenant) return null;

  const dns = branding?.dnsVerificationStatus ?? 'Pending';
  const dnsLabel =
    dns === 'Verified'
      ? t.brandDnsVerified
      : dns === 'Failed'
        ? t.brandDnsFailed
        : t.brandDnsPending;

  if (isLoading) {
    return <div className="py-8 text-center text-sm text-m3-secondary">{t.loading}</div>;
  }

  return (
    <div className="space-y-4">
      <SectionHeader
        title={t.customBranding}
        subtitle={t.brandingSubtitle}
        actions={
          <Tooltip content={t.brandDnsStatus}>
            <span>
              <StatusBadge
                status={dns}
                label={dnsLabel}
                colorMap={DNS_COLOR_MAP}
                className="cursor-default"
              />
            </span>
          </Tooltip>
        }
      />

      <form onSubmit={handleUpdateBranding}>
        <p className="text-[11px] font-semibold uppercase tracking-wide text-m3-secondary/70 mb-2">
          {t.brandingContent}
        </p>

        <M3TextField
          compact
          label={t.brandHeadline}
          value={fields.headlineText}
          onChange={e => setField('headlineText', e.target.value)}
          placeholder="e.g. Welcome to Ransa Portal"
        />
        <M3TextField
          compact
          label={t.brandSecondary}
          value={fields.secondaryText}
          onChange={e => setField('secondaryText', e.target.value)}
          placeholder="e.g. Sign in to continue"
        />
        <M3TextField
          compact
          label={t.brandButtonLabel}
          value={fields.primaryButtonLabel}
          onChange={e => setField('primaryButtonLabel', e.target.value)}
          placeholder="e.g. Sign in"
        />
        <M3TextField
          compact
          label={t.brandFooter}
          value={fields.footerText}
          onChange={e => setField('footerText', e.target.value)}
          placeholder="e.g. © 2025 Ransa Perú S.A."
        />

        <p className="text-[11px] font-semibold uppercase tracking-wide text-m3-secondary/70 mb-2 mt-4">
          {t.brandingVisual}
        </p>

        <M3FieldsetWrapper label={t.brandPrimaryColor} compact className="mb-3">
          <div className="flex items-center gap-3">
            <input
              type="color"
              value={fields.primaryColor}
              onChange={e => setField('primaryColor', e.target.value)}
              className="h-7 w-9 rounded border-0 bg-transparent cursor-pointer p-0 flex-shrink-0"
            />
            <span className="font-mono text-sm text-m3-on-surface">{fields.primaryColor}</span>
          </div>
        </M3FieldsetWrapper>

        <M3Select
          compact
          label={t.brandBackground}
          value={fields.backgroundStyle}
          onChange={e => setField('backgroundStyle', e.target.value)}
        >
          <option value="SolidColor">{t.brandBgSolid}</option>
          <option value="Gradient">{t.brandBgGradientSubtle}</option>
          <option value="Image">{t.brandBgImage}</option>
        </M3Select>

        <div className="flex gap-2 items-start mb-3">
          <div className="flex-1 min-w-0">
            <M3TextField
              compact
              label={t.brandLogoUrl}
              value={fields.logo}
              onChange={e => setField('logo', e.target.value)}
              placeholder="https://cdn.example.com/logo.png"
              className="mb-0"
            />
          </div>
          <div className="w-24 flex-shrink-0">
            <M3Select
              compact
              label={t.brandLogoFormat}
              value={fields.logoFormat}
              onChange={e => setField('logoFormat', e.target.value)}
              className="mb-0"
            >
              <option value="Png">PNG</option>
              <option value="Svg">SVG</option>
            </M3Select>
          </div>
        </div>

        {fields.logo && (
          <div className="mb-3 px-3 py-2 bg-m3-surface-container/60 rounded-lg border border-m3-outline/25 flex items-center justify-between gap-4">
            <span className="text-xs font-medium text-m3-secondary">{t.brandLogoPreview}</span>
            <img
              src={fields.logo}
              alt="Logo preview"
              className="h-6 w-auto object-contain rounded border border-m3-outline/30 bg-white p-0.5"
              onError={e => {
                (e.target as HTMLImageElement).style.display = 'none';
              }}
            />
          </div>
        )}

        <p className="text-[11px] font-semibold uppercase tracking-wide text-m3-secondary/70 mb-2 mt-4">
          {t.brandingDomain}
        </p>

        <M3TextField
          compact
          label={t.brandCustomDomain}
          value={fields.customDomain}
          onChange={e => setField('customDomain', e.target.value)}
          placeholder="e.g. auth.ransa.pe"
        />

        {fields.customDomain && branding && (
          <div className="mb-4 p-3 bg-m3-surface-container/40 rounded-xl border border-m3-outline/25 flex flex-col gap-2.5 animate-fadeIn">
            <div className="flex justify-between items-center text-[10px]">
              <span className="font-semibold text-m3-secondary uppercase tracking-wider">
                {t.brandDnsStatus || 'Estado DNS'}
              </span>
              <StatusBadge status={dns} label={dnsLabel} colorMap={DNS_COLOR_MAP} />
            </div>
            {dns !== 'Verified' && (
              <div className="flex gap-2">
                <M3Button
                  variant="filled"
                  type="button"
                  onClick={async (e: React.MouseEvent) => {
                    e.preventDefault();
                    try {
                      await brandingService.verifyDns(tenantId);
                      addNotification({
                        title: t.brandDnsVerified,
                        message: 'DNS verificado correctamente',
                        type: 'success',
                      });
                      await loadBranding();
                    } catch {
                      addNotification({
                        title: t.errorBackendUnavailableTitle,
                        message: 'Error al verificar DNS',
                        type: 'error',
                      });
                    }
                  }}
                  className="flex-1 text-[9px] py-1 h-7 font-semibold bg-emerald-600 hover:bg-emerald-700 text-white border-0 flex items-center justify-center"
                >
                  Verificar DNS
                </M3Button>
                <M3Button
                  variant="outlined"
                  type="button"
                  onClick={async (e: React.MouseEvent) => {
                    e.preventDefault();
                    try {
                      await brandingService.failDns(tenantId);
                      addNotification({
                        title: t.brandDnsFailed,
                        message: 'Simulación de error DNS',
                        type: 'warning',
                      });
                      await loadBranding();
                    } catch {
                      addNotification({
                        title: t.errorBackendUnavailableTitle,
                        message: 'Error',
                        type: 'error',
                      });
                    }
                  }}
                  className="flex-1 text-[9px] py-1 h-7 font-semibold border-rose-500/40 text-rose-500 hover:bg-rose-500/10 flex items-center justify-center"
                >
                  Simular Fallo
                </M3Button>
              </div>
            )}
          </div>
        )}

        <M3FieldsetWrapper label={t.brandMagicLink} compact className="mb-4">
          <div className="flex items-center justify-between w-full">
            <span className="text-sm text-m3-on-surface/70">
              {fields.magicLinkFallbackEnabled ? t.active : t.suspended}
            </span>
            <M3Switch
              checked={fields.magicLinkFallbackEnabled}
              onChange={v => setField('magicLinkFallbackEnabled', v)}
            />
          </div>
        </M3FieldsetWrapper>

        <M3Button variant="filled" type="submit" className="w-full">
          {t.applyBranding}
        </M3Button>
      </form>
    </div>
  );
};
