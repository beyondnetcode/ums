/**
 * system-suite.schema.ts — Runtime validation schemas (Zod)
 *
 * Single source of truth for SystemSuite types. Validates API responses
 * at the infrastructure boundary so TypeScript types match reality at runtime.
 *
 * Backend enums:
 *   SystemStatus: Active | Maintenance | Deprecated
 */
import { z } from 'zod';

export const SystemStatusSchema = z.enum(['Active', 'Maintenance', 'Deprecated']);

const GuidSchema = z.string().regex(/^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/);

export const SystemSuiteActionSchema = z.object({
  id: z.string().uuid(),
  code: z.string(),
  name: z.string(),
});

export const SystemSuiteOptionSchema = z.object({
  id: z.string().uuid(),
  code: z.string(),
  label: z.string(),
  description: z.string(),
  actionCode: z.string(),
  sortOrder: z.number(),
});

export const SystemSuiteSubMenuSchema = z.object({
  id: z.string().uuid(),
  code: z.string(),
  label: z.string(),
  description: z.string(),
  sortOrder: z.number(),
  options: z.array(SystemSuiteOptionSchema),
});

export const SystemSuiteMenuSchema = z.object({
  id: z.string().uuid(),
  code: z.string(),
  label: z.string(),
  description: z.string(),
  sortOrder: z.number(),
  subMenus: z.array(SystemSuiteSubMenuSchema),
});

export const SystemSuiteModuleSchema = z.object({
  id: z.string().uuid(),
  code: z.string(),
  name: z.string(),
  description: z.string(),
  status: z.string(),
  sortOrder: z.number(),
  menus: z.array(SystemSuiteMenuSchema),
});

export const SystemSuiteCrudOperationSchema = z.object({
  id: z.string().uuid(),
  code: z.string(),
  name: z.string(),
  description: z.string(),
  sortOrder: z.number(),
});

export const SystemSuiteCustomActionSchema = z.object({
  id: z.string().uuid(),
  code: z.string(),
  name: z.string(),
  description: z.string(),
  sortOrder: z.number(),
});

export const SystemSuiteDomainResourceSchema = z.object({
  id: z.string().uuid(),
  moduleId: z.string().uuid().nullable().optional(),
  parentResourceId: z.string().uuid().nullable().optional(),
  type: z.enum(['Aggregate', 'Entity', 'DomainMethod']),
  code: z.string(),
  name: z.string(),
  description: z.string(),
  crudOperations: z.array(SystemSuiteCrudOperationSchema).optional().default([]),
  customActions: z.array(SystemSuiteCustomActionSchema).optional().default([]),
});

export const SystemSuiteSchema = z.object({
  systemSuiteId: GuidSchema,
  tenantId:      GuidSchema,
  code:          z.string().min(1),
  name:          z.string().min(1),
  description:   z.string(),
  status:        SystemStatusSchema,
  modules:       z.array(SystemSuiteModuleSchema).optional().default([]),
  actions:       z.array(SystemSuiteActionSchema).optional().default([]),
  domainResources: z.array(SystemSuiteDomainResourceSchema).optional().default([]),
});

export const SystemSuiteListSchema = z.array(SystemSuiteSchema);

export const SystemSuitePageSchema = z.object({
  items:      SystemSuiteListSchema,
  page:       z.number().int().min(1),
  pageSize:   z.number().int().min(1),
  totalItems: z.number().int().min(0),
  totalPages: z.number().int().min(0),
});

export const CreateSystemSuitePayloadSchema = z.object({
  tenantId:    z.string().uuid(),
  code:        z.string().min(1).max(50),
  name:        z.string().min(1).max(120),
  description: z.string().max(500).optional(),
});

export const CreateSystemSuiteResponseSchema = z.object({
  systemSuiteId: z.string().uuid(),
});

export type SystemSuite               = z.infer<typeof SystemSuiteSchema>;
export type SystemSuitePage           = z.infer<typeof SystemSuitePageSchema>;
export type SystemStatus              = z.infer<typeof SystemStatusSchema>;
export type CreateSystemSuitePayload  = z.infer<typeof CreateSystemSuitePayloadSchema>;
export type CreateSystemSuiteResponse = z.infer<typeof CreateSystemSuiteResponseSchema>;
export type SystemSuiteDomainResource = z.infer<typeof SystemSuiteDomainResourceSchema>;
export type SystemSuiteCrudOperation  = z.infer<typeof SystemSuiteCrudOperationSchema>;
export type SystemSuiteCustomAction   = z.infer<typeof SystemSuiteCustomActionSchema>;
