# US-001: Create Tenant (Success)
  
## Request
- **Endpoint**: POST /api/v1/tenants
- **Payload**:
```json
{
  "code": "QA_TENANT_1",
  "name": "QA Tenant S.A.",
  "type": "Internal",
  "idpStrategy": null,
  "companyReference": "RUC-9999999"
}
```

## Response
- **Status Code**: 201
- **Body**:
```json
{
  "tenantId": "5328bab8-2e31-4522-91fa-e4c44d7a8b56"
}
```

## Verification
- Expected Status: 201 Created
- Result: PASSED
