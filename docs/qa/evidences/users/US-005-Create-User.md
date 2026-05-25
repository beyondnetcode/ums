# US-005: Create User

## Request
- **Endpoint**: POST /api/v1/user-accounts
- **Payload**:
```json
{
  "tenantId": "5328bab8-2e31-4522-91fa-e4c44d7a8b56",
  "branchId": null,
  "email": "qa.test.1@example.com",
  "category": "Internal",
  "identityReference": "EMP-001",
  "identityReferenceType": "HrId"
}
```

## Response
- **Status Code**: 201
- **Body**:
```json
{
  "userAccountId": "3f01b620-261b-47df-b046-488a9188f2bf"
}
```

## Verification
- Expected Status: 201 Created
- Result: PASSED