/* eslint-disable no-console */
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { logger, setLoggerConfig } from './logger';

describe('logger', () => {
  beforeEach(() => {
    vi.spyOn(console, 'debug').mockImplementation(() => {});
    vi.spyOn(console, 'info').mockImplementation(() => {});
    vi.spyOn(console, 'warn').mockImplementation(() => {});
    vi.spyOn(console, 'error').mockImplementation(() => {});
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('in production mode', () => {
    beforeEach(() => {
      setLoggerConfig({ minLevel: 'warn' });
    });

    it('suppresses debug messages', () => {
      logger.debug('test');
      expect(console.debug).not.toHaveBeenCalled();
    });

    it('suppresses info messages', () => {
      logger.info('test');
      expect(console.info).not.toHaveBeenCalled();
    });

    it('allows warn messages', () => {
      logger.warn('test');
      expect(console.warn).toHaveBeenCalled();
    });

    it('allows error messages', () => {
      logger.error('test');
      expect(console.error).toHaveBeenCalled();
    });
  });

  describe('in development mode', () => {
    beforeEach(() => {
      setLoggerConfig({ minLevel: 'debug' });
    });

    it('allows debug messages', () => {
      logger.debug('test');
      expect(console.debug).toHaveBeenCalled();
    });

    it('allows info messages', () => {
      logger.info('test');
      expect(console.info).toHaveBeenCalled();
    });
  });
});
