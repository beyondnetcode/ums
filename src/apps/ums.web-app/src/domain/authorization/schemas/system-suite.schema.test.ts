import { describe, expect, it } from 'vitest';
import {
  SystemStatusSchema,
  SystemSuiteSchema,
  SystemSuitePageSchema,
  CreateSystemSuitePayloadSchema,
  CreateSystemSuiteResponseSchema,
  SystemSuiteActionSchema,
  SystemSuiteOptionSchema,
  SystemSuiteModuleSchema,
  SystemSuiteDomainResourceSchema,
} from './system-suite.schema';

describe('SystemStatusSchema', () => {
  it('accepts valid status values', () => {
    expect(SystemStatusSchema.parse('Active')).toBe('Active');
    expect(SystemStatusSchema.parse('Maintenance')).toBe('Maintenance');
    expect(SystemStatusSchema.parse('Deprecated')).toBe('Deprecated');
  });

  it('rejects invalid status', () => {
    expect(() => SystemStatusSchema.parse('Unknown')).toThrow();
  });
});

describe('SystemSuiteActionSchema', () => {
  it('accepts a valid action', () => {
    const action = SystemSuiteActionSchema.parse({
      id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      code: 'CREATE',
      name: 'Create',
    });
    expect(action.code).toBe('CREATE');
  });
});

describe('SystemSuiteOptionSchema', () => {
  it('accepts a valid option', () => {
    const option = SystemSuiteOptionSchema.parse({
      id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      code: 'OPT1',
      label: 'Option 1',
      description: 'Test option',
      actionCode: 'CREATE',
      sortOrder: 1,
    });
    expect(option.label).toBe('Option 1');
  });
});

describe('SystemSuiteModuleSchema', () => {
  it('accepts a valid module', () => {
    const module = SystemSuiteModuleSchema.parse({
      id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      code: 'MOD1',
      name: 'Module 1',
      description: 'Test module',
      status: 'Active',
      sortOrder: 1,
      menus: [],
    });
    expect(module.name).toBe('Module 1');
  });
});

describe('SystemSuiteDomainResourceSchema', () => {
  it('accepts a valid domain resource', () => {
    const resource = SystemSuiteDomainResourceSchema.parse({
      id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      type: 'Aggregate',
      code: 'USER',
      name: 'User',
      description: 'User aggregate',
    });
    expect(resource.type).toBe('Aggregate');
    expect(resource.crudOperations).toEqual([]);
  });

  it('defaults crudOperations to empty array', () => {
    const resource = SystemSuiteDomainResourceSchema.parse({
      id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      type: 'Entity',
      code: 'ROLE',
      name: 'Role',
      description: 'Role entity',
    });
    expect(resource.crudOperations).toEqual([]);
  });

  it('accepts Entity type', () => {
    const resource = SystemSuiteDomainResourceSchema.parse({
      id: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      type: 'Entity',
      code: 'PERM',
      name: 'Permission',
      description: 'Permission entity',
    });
    expect(resource.type).toBe('Entity');
  });
});

describe('SystemSuiteSchema', () => {
  const validSuite = {
    systemSuiteId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa7',
    code: 'UMS',
    name: 'User Management System',
    description: 'Main system suite',
    status: 'Active',
  };

  it('accepts a valid system suite', () => {
    const suite = SystemSuiteSchema.parse(validSuite);
    expect(suite.code).toBe('UMS');
    expect(suite.status).toBe('Active');
  });

  it('defaults modules to empty array', () => {
    const suite = SystemSuiteSchema.parse(validSuite);
    expect(suite.modules).toEqual([]);
  });

  it('defaults actions to empty array', () => {
    const suite = SystemSuiteSchema.parse(validSuite);
    expect(suite.actions).toEqual([]);
  });

  it('defaults domainResources to empty array', () => {
    const suite = SystemSuiteSchema.parse(validSuite);
    expect(suite.domainResources).toEqual([]);
  });

  it('rejects empty code', () => {
    expect(() => SystemSuiteSchema.parse({ ...validSuite, code: '' })).toThrow();
  });

  it('rejects empty name', () => {
    expect(() => SystemSuiteSchema.parse({ ...validSuite, name: '' })).toThrow();
  });
});

describe('SystemSuitePageSchema', () => {
  it('accepts a valid page', () => {
    const page = SystemSuitePageSchema.parse({
      items: [],
      page: 1,
      pageSize: 10,
      totalItems: 0,
      totalPages: 0,
    });
    expect(page.page).toBe(1);
  });

  it('rejects page less than 1', () => {
    expect(() =>
      SystemSuitePageSchema.parse({
        items: [],
        page: 0,
        pageSize: 10,
        totalItems: 0,
        totalPages: 0,
      })
    ).toThrow();
  });
});

describe('CreateSystemSuitePayloadSchema', () => {
  it('accepts a valid payload', () => {
    const payload = CreateSystemSuitePayloadSchema.parse({
      tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      code: 'NEW',
      name: 'New System',
    });
    expect(payload.code).toBe('NEW');
  });

  it('accepts optional description', () => {
    const payload = CreateSystemSuitePayloadSchema.parse({
      tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      code: 'NEW',
      name: 'New System',
      description: 'A new system suite',
    });
    expect(payload.description).toBe('A new system suite');
  });

  it('rejects code exceeding max length', () => {
    expect(() =>
      CreateSystemSuitePayloadSchema.parse({
        tenantId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        code: 'A'.repeat(51),
        name: 'New System',
      })
    ).toThrow();
  });
});

describe('CreateSystemSuiteResponseSchema', () => {
  it('accepts a valid response', () => {
    const response = CreateSystemSuiteResponseSchema.parse({
      systemSuiteId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    });
    expect(response.systemSuiteId).toBe('3fa85f64-5717-4562-b3fc-2c963f66afa6');
  });
});
