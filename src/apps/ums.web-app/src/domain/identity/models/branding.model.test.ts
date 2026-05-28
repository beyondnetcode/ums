import { describe, it, expect } from 'vitest';
import { DEFAULT_BRANDING, type BrandingConfig } from './branding.model';

describe('branding.model', () => {
  it('exports DEFAULT_BRANDING with correct shape', () => {
    expect(DEFAULT_BRANDING).toBeDefined();
    expect(DEFAULT_BRANDING.headlineText).toBe('');
    expect(DEFAULT_BRANDING.secondaryText).toBe('');
    expect(DEFAULT_BRANDING.primaryButtonLabel).toBe('');
    expect(DEFAULT_BRANDING.footerText).toBe('');
    expect(DEFAULT_BRANDING.primaryColor).toBe('#3b5bdb');
    expect(DEFAULT_BRANDING.backgroundStyle).toBe('solid');
    expect(DEFAULT_BRANDING.logo).toBe('');
    expect(DEFAULT_BRANDING.logoFormat).toBe('png');
    expect(DEFAULT_BRANDING.customDomain).toBe('');
    expect(DEFAULT_BRANDING.magicLinkFallbackEnabled).toBe(false);
    expect(DEFAULT_BRANDING.dnsVerificationStatus).toBe('Pending');
  });

  it('DEFAULT_BRANDING satisfies BrandingConfig interface', () => {
    const config: BrandingConfig = DEFAULT_BRANDING;
    expect(config).toHaveProperty('headlineText');
    expect(config).toHaveProperty('primaryColor');
    expect(config).toHaveProperty('dnsVerificationStatus');
  });
});
