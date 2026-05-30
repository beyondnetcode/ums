import { z } from 'zod';

export const CreateAppConfigurationPayloadSchema = z.object({
  tenantId: z.string().uuid().nullable().optional(),
  systemSuiteId: z.string().uuid().nullable().optional(),
  moduleId: z.string().uuid().nullable().optional(),
  code: z.string().min(1).max(100),
  value: z.string(),
  description: z.string().max(500).optional().default(''),
  isInheritable: z.boolean().optional().default(false),
  isEncrypted: z.boolean().optional().default(false),
});

export type CreateAppConfigurationPayload = z.infer<typeof CreateAppConfigurationPayloadSchema>;

export const UpdateAppConfigurationPayloadSchema = z.object({
  value: z.string(),
  description: z.string().max(500).optional(),
});

export type UpdateAppConfigurationPayload = z.infer<typeof UpdateAppConfigurationPayloadSchema>;