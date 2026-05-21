import { z } from 'zod';

export const BranchSchema = z.object({
  branchId:            z.string().uuid(),
  code:                z.string().min(1),
  name:                z.string().min(1),
  isActive:            z.boolean(),
  geofencingMetadata:  z.string().nullable().optional().transform((v) => v ?? null),
});

export const BranchListSchema = z.array(BranchSchema);

export const AddBranchPayloadSchema = z.object({
  code:               z.string().min(1).max(20),
  name:               z.string().min(1).max(120),
  geofencingMetadata: z.string().optional(),
});

export const AddBranchResponseSchema = z.object({
  branchId: z.string().uuid(),
  tenantId: z.string().uuid(),
  code:     z.string(),
});

export type Branch            = z.infer<typeof BranchSchema>;
export type AddBranchPayload  = z.infer<typeof AddBranchPayloadSchema>;
export type AddBranchResponse = z.infer<typeof AddBranchResponseSchema>;
