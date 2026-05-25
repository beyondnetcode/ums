import { http, graphql, HttpResponse } from 'msw';

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
          items: [],
          totalItems: 0,
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
          items: [],
          totalItems: 0,
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
        getDelegationsByDelegatedAdmin: [],
      },
    });
  }),

  graphql.query('DelegationsByDelegatingAdmin', () => {
    return HttpResponse.json({
      data: {
        getDelegationsByDelegatingAdmin: [],
      },
    });
  }),
  
  // Catch-all for unmocked GraphQL queries to prevent 500 errors in Dev mode
  http.post('*/graphql', () => {
    return HttpResponse.json({
      data: {}
    });
  }),
];
