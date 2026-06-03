import { z } from 'zod';

export const AuditRecordSchema = z.object({
  auditRecordId: z.string().uuid(),
  whoActed: z.string().uuid(),
  subjectType: z.string().min(1),
  whenOccurred: z.string().min(1),
  whatChanged: z.string().min(1),
  eventType: z.string().min(1),
  auditResult: z.string().min(1),
  affectedEntityId: z.string().uuid(),
  affectedEntityType: z.string().min(1),
  rootTenantId: z.string().uuid(),
  metadata: z.string().nullable().optional(),
});

export type AuditRecord = z.infer<typeof AuditRecordSchema>;

export const AuditRecordPageSchema = z.object({
  items: z.array(AuditRecordSchema),
  page: z.number().int().positive(),
  pageSize: z.number().int().positive(),
  totalItems: z.number().int().nonnegative(),
  totalPages: z.number().int().nonnegative(),
});

export type AuditRecordPage = z.infer<typeof AuditRecordPageSchema>;
