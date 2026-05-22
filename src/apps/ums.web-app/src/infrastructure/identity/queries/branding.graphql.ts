/**
 * branding.graphql.ts — GraphQL query definitions for Tenant Branding bounded context
 *
 * All read operations use these GraphQL queries.
 * Commands/transactions use REST endpoints via branding.service.ts.
 */
import { graphqlClient, GraphQlValidationError } from '@infra/http/graphqlClient';

const GET_BRANDING = `
  query Branding($tenantId: ID!) {
    getTenantBranding: tenantBranding(tenantId: $tenantId) {
      logo
      logoFormat
      primaryColor
      backgroundStyle
      headlineText
      secondaryText
      primaryButtonLabel
      footerText
      customDomain
      magicLinkFallbackEnabled
      dnsVerificationStatus
    }
  }
`;

export interface GraphqlBrandingDto {
  logo: string;
  logoFormat: string;
  primaryColor: string;
  backgroundStyle: string;
  headlineText: string;
  secondaryText: string;
  primaryButtonLabel: string;
  footerText: string;
  customDomain: string | null;
  magicLinkFallbackEnabled: boolean;
  dnsVerificationStatus: string | null;
}

export interface GetBrandingResponse {
  getTenantBranding: GraphqlBrandingDto | null;
}

export const graphqlBrandingQueries = {
  getBranding: async (tenantId: string): Promise<GetBrandingResponse> => {
    if (!tenantId || tenantId.trim() === '') {
      throw new GraphQlValidationError('Invalid tenantId parameter', ['tenantId must be a non-empty string']);
    }
    return graphqlClient.request<GetBrandingResponse>(GET_BRANDING, { tenantId });
  },
};
