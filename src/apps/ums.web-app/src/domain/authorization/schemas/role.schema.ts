import { z } from 'zod';

export const RoleSchema = z.object({
  roleId: z.string().uuid(),
  tenantId: z.string().uuid(),
  systemSuiteId: z.string().uuid(),
  parentRoleId: z.string().uuid().nullable(),
  code: z.string().min(1),
  value: z.string().min(1),
  description: z.string(),
  hierarchyLevel: z.number().int().min(0),
  promotionOrder: z.number().int().min(0),
  isActive: z.boolean(),
});

export const RoleListSchema = z.array(RoleSchema);

export const CreateRoleResponseSchema = z.object({
  roleId: z.string().uuid(),
});

export type Role = z.infer<typeof RoleSchema>;
export type CreateRoleResponse = z.infer<typeof CreateRoleResponseSchema>;

export interface CreateRolePayload {
  code: string;
  value: string;
  description: string;
  parentRoleId: string | null;
  hierarchyLevel: number;
  promotionOrder: number;
}

export interface UpdateRolePayload {
  value: string;
  description: string;
  parentRoleId: string | null;
  hierarchyLevel: number;
  promotionOrder: number;
}
