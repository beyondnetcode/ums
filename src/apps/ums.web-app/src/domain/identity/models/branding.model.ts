export interface BrandingConfig {
  headlineText: string;
  secondaryText: string;
  primaryButtonLabel: string;
  footerText: string;
  primaryColor: string;
  backgroundStyle: string;
  logo: string;
  logoFormat: string;
  customDomain: string;
  magicLinkFallbackEnabled: boolean;
  dnsVerificationStatus: string;
}

export const DEFAULT_BRANDING: BrandingConfig = {
  headlineText: '',
  secondaryText: '',
  primaryButtonLabel: '',
  footerText: '',
  primaryColor: '#3b5bdb',
  backgroundStyle: 'solid',
  logo: '',
  logoFormat: 'png',
  customDomain: '',
  magicLinkFallbackEnabled: false,
  dnsVerificationStatus: 'Pending',
};

export interface UpdateBrandingPayload {
  tenantId: string;
  config: Omit<BrandingConfig, 'dnsVerificationStatus'>;
}
