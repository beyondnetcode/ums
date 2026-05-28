import { describe, it, expect } from 'vitest';
import { isValidPublicUrl, sanitizeInput, sanitizeCode, formatSystemCode } from './security';

describe('isValidPublicUrl', () => {
  it('accepts valid public HTTPS URLs', () => {
    expect(isValidPublicUrl('https://example.com')).toBe(true);
    expect(isValidPublicUrl('https://api.example.com/path')).toBe(true);
  });

  it('accepts valid public HTTP URLs', () => {
    expect(isValidPublicUrl('http://example.com')).toBe(true);
  });

  it('rejects localhost URLs', () => {
    expect(isValidPublicUrl('http://localhost:3000')).toBe(false);
    expect(isValidPublicUrl('http://localhost/api')).toBe(false);
  });

  it('rejects 172.16-31 private range URLs', () => {
    expect(isValidPublicUrl('http://172.16.0.1')).toBe(false);
    expect(isValidPublicUrl('http://172.31.255.255')).toBe(false);
  });

  it('rejects link-local addresses', () => {
    expect(isValidPublicUrl('http://169.254.169.254')).toBe(false);
  });

  it('rejects IPv6 localhost', () => {
    expect(isValidPublicUrl('http://::1')).toBe(false);
    expect(isValidPublicUrl('http://[::1]:8080')).toBe(false);
  });

  it('rejects 0.0.0.0', () => {
    expect(isValidPublicUrl('http://0.0.0.0')).toBe(false);
  });

  it('rejects invalid URLs', () => {
    expect(isValidPublicUrl('not-a-url')).toBe(false);
    expect(isValidPublicUrl('')).toBe(false);
  });

  it('rejects ftp protocol', () => {
    expect(isValidPublicUrl('ftp://example.com')).toBe(false);
  });
});

describe('sanitizeInput', () => {
  it('escapes HTML special characters', () => {
    expect(sanitizeInput('<script>')).toBe('&lt;script&gt;');
    expect(sanitizeInput('"quoted"')).toBe('&quot;quoted&quot;');
    expect(sanitizeInput("'single'")).toBe('&#x27;single&#x27;');
    expect(sanitizeInput('a&b')).toBe('a&amp;b');
  });

  it('trims whitespace', () => {
    expect(sanitizeInput('  hello  ')).toBe('hello');
  });

  it('leaves safe input unchanged', () => {
    expect(sanitizeInput('Hello World')).toBe('Hello World');
    expect(sanitizeInput('123-abc')).toBe('123-abc');
  });

  it('handles mixed dangerous characters', () => {
    expect(sanitizeInput('<div class="test">')).toBe('&lt;div class=&quot;test&quot;&gt;');
  });
});

describe('sanitizeCode', () => {
  it('removes non-alphanumeric characters except underscore', () => {
    expect(sanitizeCode('ABC-123')).toBe('ABC123');
  });

  it('preserves uppercase letters, digits, and underscores', () => {
    expect(sanitizeCode('ABC_123_DEF')).toBe('ABC_123_DEF');
  });

  it('removes lowercase letters', () => {
    expect(sanitizeCode('abcABC')).toBe('ABC');
  });

  it('trims whitespace', () => {
    expect(sanitizeCode('  ABC  ')).toBe('ABC');
  });

  it('returns empty string for all-invalid input', () => {
    expect(sanitizeCode('!@#$%')).toBe('');
  });
});

describe('formatSystemCode', () => {
  it('converts to uppercase', () => {
    expect(formatSystemCode('hello')).toBe('HELLO');
  });

  it('replaces spaces with underscores', () => {
    expect(formatSystemCode('hello world')).toBe('HELLO_WORLD');
  });

  it('trims whitespace', () => {
    expect(formatSystemCode('  test  ')).toBe('TEST');
  });

  it('handles multiple consecutive spaces', () => {
    expect(formatSystemCode('hello   world')).toBe('HELLO_WORLD');
  });

  it('handles mixed case with spaces', () => {
    expect(formatSystemCode('User Management System')).toBe('USER_MANAGEMENT_SYSTEM');
  });
});
