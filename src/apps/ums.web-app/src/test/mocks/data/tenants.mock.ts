export const mockTenants = [
  {
    tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    code: 'RANSA_PERU',
    name: 'Ransa Perú',
    type: 'Internal',
    status: 'Active',
    companyReference: '20100035392',
    idpStrategy: 'AzureAd',
    branches: [
      { id: '10000000-0000-0000-0000-000000000001', code: 'LIM_N', name: 'Lima Norte' },
      { id: '10000000-0000-0000-0000-000000000002', code: 'LIM_S', name: 'Lima Sur' },
      { id: '10000000-0000-0000-0000-000000000003', code: 'PIU_1', name: 'Piura Almacenes' }
    ],
    identityProviders: [
      { id: '20000000-0000-0000-0000-000000000001', code: 'ENTRA_ID', name: 'Azure AD Corporativo', description: 'Directorio principal Ransa', strategy: 'AzureAd', isActive: true }
    ],
    branding: null,
    parentTenantId: null
  },
  {
    tenantId: 'c9b736b4-6a84-48f8-b34d-176bc5a6d542',
    code: 'NEPTUNIA',
    name: 'Neptunia S.A.',
    type: 'Client',
    status: 'Active',
    companyReference: '20300055555',
    idpStrategy: 'Okta',
    branches: [
      { id: '10000000-0000-0000-0000-000000000004', code: 'CAL_1', name: 'Puerto Callao Muelle 1' }
    ],
    identityProviders: [
      { id: '20000000-0000-0000-0000-000000000002', code: 'OKTA_CORP', name: 'Okta Neptunia', description: 'Directorio subsidiarias', strategy: 'Okta', isActive: true }
    ],
    branding: null,
    parentTenantId: null
  },
  {
    tenantId: 'a3f5b8c2-416d-495e-9f3a-7a5d8f6154b1',
    code: 'APM_CALLAO',
    name: 'APM Terminals Callao',
    type: 'Client',
    status: 'Active',
    companyReference: '20400011111',
    idpStrategy: 'InternalBcrypt',
    branches: [
      { id: '10000000-0000-0000-0000-000000000005', code: 'APM_C1', name: 'Terminal Norte' }
    ],
    identityProviders: [],
    branding: null,
    parentTenantId: null
  },
  {
    tenantId: 'f8a7e3c9-0218-4b7c-86e4-3d9a5b210a24',
    code: 'PAITA_PORT',
    name: 'Terminales Portuarios Euroandinos (Paita)',
    type: 'Client',
    status: 'Active',
    companyReference: '20500077777',
    idpStrategy: 'InternalBcrypt',
    branches: [],
    identityProviders: [],
    branding: null,
    parentTenantId: null
  },
  {
    tenantId: 'd4c2b9a7-8e6f-4513-9c8a-1f3e7d5b2c68',
    code: 'UNIMAR',
    name: 'Unimar Logística',
    type: 'Supplier',
    status: 'Active',
    companyReference: '20600088888',
    idpStrategy: 'InternalBcrypt',
    branches: [],
    identityProviders: [],
    branding: null,
    parentTenantId: null
  },
  {
    tenantId: 'e1d5a8f4-3b7c-4a9e-8d2f-6c4b0e9a1d37',
    code: 'INTRADEVCO',
    name: 'Intradevco Industrial S.A.',
    type: 'Client',
    status: 'Active',
    companyReference: '20100099999',
    idpStrategy: 'InternalBcrypt',
    branches: [],
    identityProviders: [],
    branding: null,
    parentTenantId: null
  }
];
