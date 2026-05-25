# US-004: Get Tenants (GraphQL)

## Request
- **Payload**: 
```json
{"query":"query { getTenants: tenants(page: 1, pageSize: 10, search: \"QA Tenant S.A.\") { items { tenantId name status } totalItems } }"}
```

## Response
- **Status Code**: 200
- **Body**:
```json
{
  "data": {
    "getTenants": {
      "items": [
        {
          "tenantId": "5328bab8-2e31-4522-91fa-e4c44d7a8b56",
          "name": "QA Tenant S.A.",
          "status": "Active"
        }
      ],
      "totalItems": 1
    }
  }
}
```

## Verification
- Expected: Item found
- Result: PASSED