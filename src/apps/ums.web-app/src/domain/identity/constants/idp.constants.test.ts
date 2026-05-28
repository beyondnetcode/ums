import { describe, it, expect } from 'vitest';
import { IDP_STRATEGIES } from '@domain/identity/constants/idp.constants';

describe('idp.constants', () => {
  it('exports correct strategies', () => {
    expect(IDP_STRATEGIES).toContain('OIDC');
    expect(IDP_STRATEGIES).toContain('SAML2');
    expect(IDP_STRATEGIES).toContain('OAuth2');
    expect(IDP_STRATEGIES).toHaveLength(3);
  });
});
