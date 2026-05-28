import { z } from 'zod';

const guidSchema = z.string().regex(
  /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/,
  'Invalid GUID format',
);

export const ProfilePermissionSchema = z.object({
  permissionId: guidSchema,
  profileId: guidSchema,
  templateId: guidSchema,
  targetType: z.string(),
  targetId: guidSchema,
  targetName: z.string(),
  actionId: guidSchema,
  actionName: z.string(),
  isAllowed: z.boolean(),
  isDenied: z.boolean(),
  isActive: z.boolean(),
  isOverride: z.boolean(),
});

export const ProfileSchema = z.object({
  profileId: guidSchema,
  tenantId: guidSchema,
  userId: guidSchema,
  roleId: guidSchema,
  branchId: guidSchema.nullable().optional(),
  scope: z.string(),
  isActive: z.boolean(),
  permissions: z.array(ProfilePermissionSchema),
});

export const ProfilePageSchema = z.object({
  items: z.array(ProfileSchema),
  page: z.number().int().min(1),
  pageSize: z.number().int().min(1),
  totalItems: z.number().int().min(0),
  totalPages: z.number().int().min(0),
});

export const CreateProfileResponseSchema = z.object({
  profileId: guidSchema,
});

export type ProfilePermission = z.infer<typeof ProfilePermissionSchema>;
export type Profile = z.infer<typeof ProfileSchema>;
export type ProfilePage = z.infer<typeof ProfilePageSchema>;
export type CreateProfileResponse = z.infer<typeof CreateProfileResponseSchema>;

export interface CreateProfilePayload {
  tenantId: string;
  userId: string;
  roleId: string;
  branchId?: string | null;
}
