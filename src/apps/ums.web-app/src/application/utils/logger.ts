/* eslint-disable no-console, @typescript-eslint/no-unused-vars */
/**
 * logger.ts — Structured logger with environment-aware levels.
 *
 * In production, only error/warn are enabled.
 * In development, all levels (debug, info) are available.
 */

type LogLevel = 'debug' | 'info' | 'warn' | 'error';

const LEVEL_PRIORITY: Record<LogLevel, number> = {
  debug: 0,
  info: 1,
  warn: 2,
  error: 3,
};

export interface LoggerConfig {
  minLevel: LogLevel;
}

let config: LoggerConfig = {
  minLevel: import.meta.env.DEV ? 'debug' : 'warn',
};

export function setLoggerConfig(newConfig: Partial<LoggerConfig>): void {
  config = { ...config, ...newConfig };
}

function shouldLog(level: LogLevel): boolean {
  return LEVEL_PRIORITY[level] >= LEVEL_PRIORITY[config.minLevel];
}

function formatMessage(level: LogLevel, message: string, ...args: unknown[]): string {
  const timestamp = new Date().toISOString();
  const prefix = `[UMS:${level.toUpperCase()}]`;
  return `${timestamp} ${prefix} ${message}`;
}

export const logger = {
  debug: (message: string, ...args: unknown[]) => {
    if (shouldLog('debug')) console.debug(formatMessage('debug', message), ...args);
  },
  info: (message: string, ...args: unknown[]) => {
    if (shouldLog('info')) console.info(formatMessage('info', message), ...args);
  },
  warn: (message: string, ...args: unknown[]) => {
    if (shouldLog('warn')) console.warn(formatMessage('warn', message), ...args);
  },
  error: (message: string, ...args: unknown[]) => {
    if (shouldLog('error')) console.error(formatMessage('error', message), ...args);
  },
};
