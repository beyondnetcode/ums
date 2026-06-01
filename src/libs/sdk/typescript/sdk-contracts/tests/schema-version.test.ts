import { describe, it, expect } from 'vitest';
import {
  SchemaVersion,
  isSchemaVersionSupported,
  isMinorAhead,
  isMinorBehind,
  isMajorMatch
} from '../src/index.js';

describe('SchemaVersion compatibility', () => {
  it.each([
    ['1.0.0', true],
    ['1.0.5', true],
    ['1.99.0', true],
    ['2.0.0', false],
    ['2.5.3', false],
    ['0.9.0', false],
    ['', false],
    [null, false],
    [undefined, false],
    ['not-a-version', false]
  ])('isSchemaVersionSupported(%s) === %s', (input, expected) => {
    expect(isSchemaVersionSupported(input)).toBe(expected);
  });

  it('Current and bounds match expected compatibility window', () => {
    expect(isSchemaVersionSupported(SchemaVersion.Current)).toBe(true);
    expect(isSchemaVersionSupported(SchemaVersion.CompatibilityMinInclusive)).toBe(true);
    expect(isSchemaVersionSupported(SchemaVersion.CompatibilityMaxExclusive)).toBe(false);
  });

  it('isMinorAhead detects server newer than SDK', () => {
    expect(isMinorAhead('1.1.0')).toBe(true);
    expect(isMinorAhead('1.0.0')).toBe(false);
    expect(isMinorAhead('0.9.0')).toBe(false);
  });

  it('isMinorBehind detects SDK newer than server', () => {
    expect(isMinorBehind('1.0.0')).toBe(false);
    // After bumping SchemaVersion.Current the older 0.x is MAJOR-different, so behind returns false.
    expect(isMinorBehind('0.9.0')).toBe(false);
  });

  it('isMajorMatch checks major equality', () => {
    expect(isMajorMatch('1.999.999')).toBe(true);
    expect(isMajorMatch('2.0.0')).toBe(false);
  });
});
