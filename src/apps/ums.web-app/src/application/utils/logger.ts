/* eslint-disable no-console */
/**
 * logger.ts — Minimal production logger.
 *
 * In production (NODE_ENV=production): debug/info suppressed, only warn/error enabled
 * In development: all levels active with formatted output
 */
type LogLevel = 'debug' | 'info' | 'warn' | 'error';

const PRODUCTION_LEVEL: LogLevel = 'warn';

function createLogger(level: LogLevel) {
  const shouldLog = import.meta.env.DEV || level === 'warn' || level === 'error';

  if (!shouldLog) {
    return () => {};
  }

  const prefix = `[UMS:${level.toUpperCase()}]`;
  const logFn = level === 'error' ? console.error : level === 'warn' ? console.warn : level === 'info' ? console.info : console.debug;

  return (message: string, ...args: unknown[]) => {
    logFn(`${new Date().toISOString()} ${prefix} ${message}`, ...args);
  };
}

export const logger = {
  debug: createLogger('debug'),
  info: createLogger('info'),
  warn: createLogger('warn'),
  error: createLogger('error'),
};