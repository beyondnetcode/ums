export const mockDelegations = [
  {
    delegationId: '12345678-1234-1234-1234-123456789012', 
    tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa6', 
    delegatingAdminId: '01a85f64-5717-4562-b3fc-2c963f66afa6', 
    delegatedAdminId: '02b736b4-6a84-48f8-b34d-176bc5a6d542', 
    scopeType: 'Tenant',
    scopeId: null,
    allowedActions: [
      'CreateUser',
      'BlockUser'
    ],
    validFrom: new Date(Date.now() - 86400000).toISOString(),
    validUntil: new Date(Date.now() + 30 * 86400000).toISOString(),
    maxDurationDays: null,
    requiresApproval: false,
    approvalRequestId: null,
    status: 'Active',
    revokedAt: null,
    revokedBy: null,
    revocationReason: null
  }
];
