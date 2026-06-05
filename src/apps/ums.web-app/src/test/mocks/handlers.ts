import { http, graphql, HttpResponse } from 'msw';
import { mockTenants } from './data/tenants.mock';
import { mockUserAccounts } from './data/user-accounts.mock';
import { mockDelegations } from './data/delegations.mock';
import { mockSystemSuites } from './data/system-suites.mock';

export const handlers = [
  // Example REST interception
  http.get('*/api/v1/profiles/*', () => {
    return HttpResponse.json({
      id: 'mock-id',
      userId: 'mock-user',
      templates: [],
      permissions: [],
      status: 'Active',
    });
  }),

  // Example GraphQL interception
  graphql.query('Tenants', () => {
    return HttpResponse.json({
      data: {
        getTenants: {
          items: mockTenants,
          totalItems: mockTenants.length,
          totalPages: 1,
          page: 1,
          pageSize: 20,
        },
      },
    });
  }),

  graphql.query('UserAccounts', () => {
    return HttpResponse.json({
      data: {
        getUserAccounts: {
          items: mockUserAccounts.items,
          totalItems: mockUserAccounts.totalCount,
          totalPages: 1,
          page: 1,
          pageSize: 20,
        },
      },
    });
  }),

  graphql.query('DelegationsByDelegatedAdmin', () => {
    return HttpResponse.json({
      data: {
        getDelegationsByDelegatedAdmin: mockDelegations,
      },
    });
  }),

  graphql.query('DelegationsByDelegatingAdmin', () => {
    return HttpResponse.json({
      data: {
        getDelegationsByDelegatingAdmin: mockDelegations,
      },
    });
  }),

  graphql.query('Tenant', ({ variables }) => {
    const { tenantId } = variables;
    const tenant = mockTenants.find(t => t.tenantId === tenantId) || mockTenants[0];
    return HttpResponse.json({
      data: {
        getTenantById: tenant,
      },
    });
  }),

  graphql.query('TenantBranches', ({ variables }) => {
    const { tenantId } = variables;
    const tenant = mockTenants.find(t => t.tenantId === tenantId) || mockTenants[0];
    return HttpResponse.json({
      data: {
        getTenantBranches: tenant.branches || [],
      },
    });
  }),

  graphql.query('UserAccount', ({ variables }) => {
    const { userAccountId } = variables;
    const account =
      mockUserAccounts.items.find(u => u.userAccountId === userAccountId) ||
      mockUserAccounts.items[0];
    return HttpResponse.json({
      data: {
        getUserAccountById: account,
      },
    });
  }),

  graphql.query('DelegationById', ({ variables }) => {
    const { delegationId } = variables;
    const delegation =
      mockDelegations.find(d => d.delegationId === delegationId) || mockDelegations[0];
    return HttpResponse.json({
      data: {
        getDelegationById: delegation,
      },
    });
  }),

  graphql.query('IdentityProviders', ({ variables }) => {
    const { tenantId } = variables;
    const tenant = mockTenants.find(t => t.tenantId === tenantId) || mockTenants[0];
    return HttpResponse.json({
      data: {
        getTenantIdentityProviders: tenant.identityProviders || [],
      },
    });
  }),

  graphql.query('Branding', ({ variables }) => {
    const { tenantId } = variables;
    const tenant = mockTenants.find(t => t.tenantId === tenantId) || mockTenants[0];
    return HttpResponse.json({
      data: {
        getTenantBranding: tenant.branding || null,
      },
    });
  }),

  // ── Authorization / SystemSuite ────────────────────────────────────────────
  graphql.query('SystemSuites', ({ variables }) => {
    const { page = 1, pageSize = 20, status = 'all' } = variables || {};
    let items = mockSystemSuites;
    if (status && status !== 'all') {
      items = items.filter(s => s.status === status);
    }
    const totalItems = items.length;
    const totalPages = Math.ceil(totalItems / pageSize);
    const pagedItems = items.slice((page - 1) * pageSize, page * pageSize);
    return HttpResponse.json({
      data: {
        getSystemSuites: {
          items: pagedItems,
          page,
          pageSize,
          totalItems,
          totalPages,
        },
      },
    });
  }),

  graphql.query('SystemSuite', ({ variables }) => {
    const { systemSuiteId } = variables;
    const suite =
      mockSystemSuites.find(s => s.systemSuiteId === systemSuiteId) || mockSystemSuites[0];
    return HttpResponse.json({
      data: {
        getSystemSuiteById: suite,
      },
    });
  }),

  // Catch-all for unmocked GraphQL queries to prevent 500 errors in Dev mode
  http.post('*/graphql', () => {
    return HttpResponse.json({
      data: {},
    });
  }),
];
