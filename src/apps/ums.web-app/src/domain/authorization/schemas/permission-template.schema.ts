/**
 * permission-template.schema.ts — Zod runtime validation schemas.
 *
 * PermissionTemplate lifecycle:  Draft → Published → Deprecated
 * PermissionTemplateItem effect: isAllowed=true ✓ | isDenied=true ✗ | both false ~
 * ExclusiveArcTarget:            SystemSuite | Module | Submodule | Option
 */
import { z } from 'zod';

// Zod v4 enforces RFC 4122 variant bits in .uuid() — too strict for SQL Server
// GUIDs which may not satisfy those bit constraints.  Use a format-only check.
const guidSchema = z
  .string()
  .regex(
    /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/,
    'Invalid GUID format'
  );

export const TemplateStatusSchema = z.enum(['Draft', 'Published', 'Deprecated']);
export const ExclusiveArcTargetSchema = z.enum([
  'SystemSuite',
  'Module',
  'Submodule',
  'Option',
  'Aggregate',
  'Entity',
]);
export const PermissionEffectSchema = z.enum(['Allow', 'Deny', 'Neutral']);

export const PermissionTemplateItemSchema = z.object({
  itemId: guidSchema,
  targetType: ExclusiveArcTargetSchema,
  targetId: guidSchema,
  targetName: z.string(),
  actionId: guidSchema,
  actionName: z.string(),
  isAllowed: z.boolean(),
  isDenied: z.boolean(),
  isActive: z.boolean(),
});

export const PermissionTemplateSchema = z.object({
  templateId: guidSchema,
  tenantId: guidSchema,
  roleId: guidSchema,
  roleName: z.string(),
  systemSuiteId: guidSchema,
  systemSuiteName: z.string(),
  version: z.string(),
  status: TemplateStatusSchema,
});

export const PermissionTemplateDetailSchema = PermissionTemplateSchema.extend({
  items: z.array(PermissionTemplateItemSchema),
});

export const PermissionTemplatePageSchema = z.object({
  items: z.array(PermissionTemplateSchema),
  page: z.number().int().min(1),
  pageSize: z.number().int().min(1),
  totalItems: z.number().int().min(0),
  totalPages: z.number().int().min(0),
});

export const CreatePermissionTemplateResponseSchema = z.object({
  templateId: guidSchema,
});

export type TemplateStatus = z.infer<typeof TemplateStatusSchema>;
export type ExclusiveArcTarget = z.infer<typeof ExclusiveArcTargetSchema>;
export type PermissionEffect = z.infer<typeof PermissionEffectSchema>;
export type PermissionTemplateItem = z.infer<typeof PermissionTemplateItemSchema>;
export type PermissionTemplate = z.infer<typeof PermissionTemplateSchema>;
export type PermissionTemplateDetail = z.infer<typeof PermissionTemplateDetailSchema>;
export type PermissionTemplatePage = z.infer<typeof PermissionTemplatePageSchema>;
export type CreatePermissionTemplateResponse = z.infer<
  typeof CreatePermissionTemplateResponseSchema
>;

export interface CreatePermissionTemplatePayload {
  tenantId: string;
  roleId: string;
  systemSuiteId: string;
}

export interface AddTemplateItemPayload {
  targetType: ExclusiveArcTarget;
  targetId: string;
  actionId: string;
  isAllowed: boolean;
  isDenied: boolean;
}

/** Derives the human-readable effect string from an item's booleans. */
export function itemEffect(
  item: Pick<PermissionTemplateItem, 'isAllowed' | 'isDenied'>
): PermissionEffect {
  if (item.isAllowed) return 'Allow';
  if (item.isDenied) return 'Deny';
  return 'Neutral';
}
