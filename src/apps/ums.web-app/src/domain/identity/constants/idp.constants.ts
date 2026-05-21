/**
 * idp.constants.ts — Domain constants for Identity Providers.
 *
 * Centralises valid IdP strategy values referenced by forms and
 * panels so changes propagate from one place.
 */

/** Valid identity provider strategy values. */
export const IDP_STRATEGIES = ['OIDC', 'SAML2', 'OAuth2'] as const;
export type IdpStrategy = (typeof IDP_STRATEGIES)[number];
