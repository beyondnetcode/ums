import { describe, expect, it } from 'vitest';
import {
  BrandingSchema,
  SetBrandingPayloadSchema,
} from './branding.schema';

describe('BrandingSchema', () => {
  it('accepts a valid branding payload', () => {
    const branding = BrandingSchema.parse({
      logo: 'base64encodedlogo',
      logoFormat: 'png',
      primaryColor: '#0066CC',
      backgroundStyle: 'gradient',
      headlineText: 'Welcome to UMS',
      secondaryText: 'User Management System',
      primaryButtonLabel: 'Sign In',
      footerText: '© 2024 Company',
      customDomain: 'login.company.com',
      magicLinkFallbackEnabled: true,
      dnsVerificationStatus: 'verified',
    });

    expect(branding.primaryColor).toBe('#0066CC');
    expect(branding.magicLinkFallbackEnabled).toBe(true);
  });

  it('accepts null customDomain', () => {
    const branding = BrandingSchema.parse({
      logo: 'logo',
      logoFormat: 'svg',
      primaryColor: '#FF0000',
      backgroundStyle: 'solid',
      headlineText: 'Hello',
      secondaryText: 'World',
      primaryButtonLabel: 'Go',
      footerText: 'Footer',
      customDomain: null,
      magicLinkFallbackEnabled: false,
      dnsVerificationStatus: null,
    });

    expect(branding.customDomain).toBeNull();
    expect(branding.dnsVerificationStatus).toBeNull();
  });
});

describe('SetBrandingPayloadSchema', () => {
  it('accepts a valid set branding payload', () => {
    const payload = SetBrandingPayloadSchema.parse({
      logo: 'base64logo',
      logoFormat: 'png',
      primaryColor: '#0066CC',
      backgroundStyle: 'gradient',
      headlineText: 'Welcome',
      secondaryText: 'Subtitle',
      primaryButtonLabel: 'Login',
      footerText: 'Footer',
      magicLinkFallbackEnabled: true,
    });

    expect(payload.logoFormat).toBe('png');
  });

  it('accepts optional customDomain', () => {
    const payload = SetBrandingPayloadSchema.parse({
      logo: 'logo',
      logoFormat: 'svg',
      primaryColor: '#000000',
      backgroundStyle: 'solid',
      headlineText: 'Title',
      secondaryText: 'Sub',
      primaryButtonLabel: 'Btn',
      footerText: 'Foot',
      customDomain: 'auth.example.com',
      magicLinkFallbackEnabled: false,
    });

    expect(payload.customDomain).toBe('auth.example.com');
  });

  it('rejects empty logo', () => {
    expect(() =>
      SetBrandingPayloadSchema.parse({
        logo: '',
        logoFormat: 'png',
        primaryColor: '#000',
        backgroundStyle: 'solid',
        headlineText: 'Title',
        secondaryText: 'Sub',
        primaryButtonLabel: 'Btn',
        footerText: 'Foot',
        magicLinkFallbackEnabled: true,
      })
    ).toThrow();
  });

  it('rejects empty primaryColor', () => {
    expect(() =>
      SetBrandingPayloadSchema.parse({
        logo: 'logo',
        logoFormat: 'png',
        primaryColor: '',
        backgroundStyle: 'solid',
        headlineText: 'Title',
        secondaryText: 'Sub',
        primaryButtonLabel: 'Btn',
        footerText: 'Foot',
        magicLinkFallbackEnabled: true,
      })
    ).toThrow();
  });
});
