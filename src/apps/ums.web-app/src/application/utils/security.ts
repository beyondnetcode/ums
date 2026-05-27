const ALLOWED_PROTOCOLS = ['http:', 'https:'];

const PRIVATE_IP_PATTERNS = [
  /^https?:\/\/(?:127\.|10\.|192\.168\.|172\.(?:1[6-9]|2\d|3[01]))\./,
  /^https?:\/\/localhost(?:[:/]|$)/i,
  /^https?:\/\/(?:0\.0\.0\.0|::1|\[::1\])(?:[:/]|$)/,
  /^https?:\/\/169\.254\./,
];

export function isValidPublicUrl(url: string): boolean {
  try {
    const parsed = new URL(url);
    if (!ALLOWED_PROTOCOLS.includes(parsed.protocol)) return false;
    for (const pattern of PRIVATE_IP_PATTERNS) {
      if (pattern.test(url)) return false;
    }
    return true;
  } catch {
    return false;
  }
}

const DANGEROUS_CHARS = /[<>"'&]/g;
const CHAR_MAP: Record<string, string> = {
  '<': '&lt;',
  '>': '&gt;',
  '"': '&quot;',
  "'": '&#x27;',
  '&': '&amp;',
};

export function sanitizeInput(input: string): string {
  return input.replace(DANGEROUS_CHARS, (char) => CHAR_MAP[char] ?? char).trim();
}

export function sanitizeCode(input: string): string {
  return input.replace(/[^A-Z0-9_]/g, '').trim();
}

/**
 * Format a raw string into a standard technical system code.
 * Standardizes to uppercase, replaces whitespaces with underscores, and trims.
 */
export function formatSystemCode(input: string): string {
  return input.trim().toUpperCase().replace(/\s+/g, '_');
}

