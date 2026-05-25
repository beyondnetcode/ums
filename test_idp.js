process.env.NODE_TLS_REJECT_UNAUTHORIZED='0';

const query = `
query IdentityProviders($tenantId: UUID!) {
  getTenantIdentityProviders: tenantIdentityProviders(tenantId: $tenantId) {
    identityProviderId
    code
    name
    description
    strategy
    isActive
  }
}`;

fetch('https://localhost:7114/graphql', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'X-Tenant-Id': '3fa85f64-5717-4562-b3fc-2c963f66afa6'
  },
  body: JSON.stringify({
    query: query,
    variables: { tenantId: "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
  })
})
.then(r => r.json())
.then(d => console.log(JSON.stringify(d, null, 2)))
.catch(e => console.error(e));
