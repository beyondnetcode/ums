/**
 * branding.schema.ts
 *
 * Zod schemas for Tenant Branding runtime validation.
 */
import { z } from 'zod';

export const BrandingSchema = z.object({
  logo: z.string(),
  logoFormat: z.string(),
  primaryColor: z.string(),
  backgroundStyle: z.string(),
  headlineText: z.string(),
  secondaryText: z.string(),
  primaryButtonLabel: z.string(),
  footerText: z.string(),
  customDomain: z.string().nullable(),
  magicLinkFallbackEnabled: z.boolean(),
  dnsVerificationStatus: z.string().nullable(),
});

export type Branding = z.infer<typeof BrandingSchema>;

export const SetBrandingPayloadSchema = z.object({
  logo: z.string().min(1),
  logoFormat: z.string().min(1),
  primaryColor: z.string().min(1),
  backgroundStyle: z.string().min(1),
  headlineText: z.string().min(1),
  secondaryText: z.string().min(1),
  primaryButtonLabel: z.string().min(1),
  footerText: z.string().min(1),
  customDomain: z.string().optional(),
  magicLinkFallbackEnabled: z.boolean(),
});

export type SetBrandingPayload = z.infer<typeof SetBrandingPayloadSchema>;

export const UpdateBrandingPayloadSchema = SetBrandingPayloadSchema;
export type UpdateBrandingPayload = z.infer<typeof UpdateBrandingPayloadSchema>;
