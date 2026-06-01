import type { AuthGraphAccessor, AuthorizationValidator, AuthorizationLogger } from '@ums/sdk-authorization';
import type { ModuleMetadata, Type } from '@nestjs/common';

export type UmsAuthorizationMode = 'enforce' | 'audit-only';

/**
 * Configuration passed to `UmsSdkModule.forRoot()`. All fields are optional — sensible defaults
 * keep small consumers concise: a fresh AuthorizationValidator, no accessor (you provide one),
 * enforce mode, no audit logger.
 */
export interface UmsSdkModuleOptions {
  /** Compatibility range advertised by the SDK to consumers. Defaults to `">=1.0.0 <2.0.0"`. */
  readonly schemaCompatibility?: string;

  /** Enforcement mode. Defaults to `'enforce'`. */
  readonly mode?: UmsAuthorizationMode;

  /** Authorization graph accessor. Defaults to AsyncLocalAuthGraphAccessor (Node). */
  readonly accessor?: AuthGraphAccessor;

  /** Validator instance. Defaults to a fresh AuthorizationValidator. */
  readonly validator?: AuthorizationValidator;

  /** Optional structured logger for denial events. */
  readonly logger?: AuthorizationLogger;

  /**
   * Optional UMS base URL — reserved for the Phase 2 HTTP client. Currently unused by the
   * guard/decorator surface but accepted so app code can declare it once.
   */
  readonly umsBaseUrl?: string;
}

/** Async factory form, for ConfigService-driven setups. */
export interface UmsSdkModuleAsyncOptions extends Pick<ModuleMetadata, 'imports'> {
  readonly inject?: Array<Type<unknown> | string | symbol>;
  readonly useFactory: (...args: unknown[]) => UmsSdkModuleOptions | Promise<UmsSdkModuleOptions>;
}
