import { describe, expect, it } from 'vitest';
import {
  BranchSchema,
  BranchListSchema,
  AddBranchPayloadSchema,
  AddBranchResponseSchema,
} from './branch.schema';

describe('BranchSchema', () => {
  it('accepts a valid branch payload', () => {
    const branch = BranchSchema.parse({
      branchId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      code: 'BR001',
      name: 'Main Branch',
      isActive: true,
      geofencingMetadata: null,
    });

    expect(branch.code).toBe('BR001');
    expect(branch.name).toBe('Main Branch');
    expect(branch.isActive).toBe(true);
  });

  it('accepts optional geofencingMetadata', () => {
    const branch = BranchSchema.parse({
      branchId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      code: 'BR002',
      name: 'Warehouse',
      isActive: false,
      geofencingMetadata: '{"lat": -12.0464, "lng": -77.0428}',
    });

    expect(branch.geofencingMetadata).toBe('{"lat": -12.0464, "lng": -77.0428}');
  });

  it('transforms undefined geofencingMetadata to null', () => {
    const branch = BranchSchema.parse({
      branchId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      code: 'BR003',
      name: 'Office',
      isActive: true,
    });

    expect(branch.geofencingMetadata).toBeNull();
  });

  it('rejects invalid UUID for branchId', () => {
    expect(() =>
      BranchSchema.parse({
        branchId: 'not-a-uuid',
        code: 'BR001',
        name: 'Main Branch',
        isActive: true,
      })
    ).toThrow();
  });

  it('rejects empty code', () => {
    expect(() =>
      BranchSchema.parse({
        branchId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        code: '',
        name: 'Main Branch',
        isActive: true,
      })
    ).toThrow();
  });

  it('rejects empty name', () => {
    expect(() =>
      BranchSchema.parse({
        branchId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        code: 'BR001',
        name: '',
        isActive: true,
      })
    ).toThrow();
  });
});

describe('BranchListSchema', () => {
  it('accepts an array of valid branches', () => {
    const branches = BranchListSchema.parse([
      {
        branchId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
        code: 'BR001',
        name: 'Main Branch',
        isActive: true,
        geofencingMetadata: null,
      },
      {
        branchId: '4fa85f64-5717-4562-b3fc-2c963f66afa7',
        code: 'BR002',
        name: 'Secondary Branch',
        isActive: false,
      },
    ]);

    expect(branches).toHaveLength(2);
    expect(branches[0].code).toBe('BR001');
  });
});

describe('AddBranchPayloadSchema', () => {
  it('accepts a valid add branch payload', () => {
    const payload = AddBranchPayloadSchema.parse({
      code: 'NEW001',
      name: 'New Branch',
    });

    expect(payload.code).toBe('NEW001');
    expect(payload.name).toBe('New Branch');
  });

  it('accepts optional geofencingMetadata', () => {
    const payload = AddBranchPayloadSchema.parse({
      code: 'NEW002',
      name: 'Geo Branch',
      geofencingMetadata: '{"radius": 500}',
    });

    expect(payload.geofencingMetadata).toBe('{"radius": 500}');
  });

  it('rejects code exceeding max length', () => {
    expect(() =>
      AddBranchPayloadSchema.parse({
        code: 'A'.repeat(21),
        name: 'Too Long Code',
      })
    ).toThrow();
  });

  it('rejects name exceeding max length', () => {
    expect(() =>
      AddBranchPayloadSchema.parse({
        code: 'X01',
        name: 'A'.repeat(121),
      })
    ).toThrow();
  });
});

describe('AddBranchResponseSchema', () => {
  it('accepts a valid response', () => {
    const response = AddBranchResponseSchema.parse({
      branchId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
      tenantId: 'c9b736b4-6a84-48f8-b34d-176bc5a6d542',
      code: 'NEW001',
    });

    expect(response.branchId).toBe('3fa85f64-5717-4562-b3fc-2c963f66afa6');
  });

  it('rejects non-UUID branchId', () => {
    expect(() =>
      AddBranchResponseSchema.parse({
        branchId: 'invalid',
        tenantId: 'c9b736b4-6a84-48f8-b34d-176bc5a6d542',
        code: 'NEW001',
      })
    ).toThrow();
  });
});
