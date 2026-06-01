import {
  type DynamicModule,
  Global,
  Module,
  type Provider
} from '@nestjs/common';
import { AsyncLocalAuthGraphAccessor, AuthorizationValidator } from '@ums/sdk-authorization';
import {
  UMS_AUTH_GRAPH_ACCESSOR,
  UMS_AUTHORIZATION_LOGGER,
  UMS_AUTHORIZATION_VALIDATOR,
  UMS_SDK_OPTIONS
} from './tokens.js';
import type { UmsSdkModuleAsyncOptions, UmsSdkModuleOptions } from './options.js';
import { UmsAuthGuard } from './guard.js';
import { AuthorizationDeniedFilter } from './filter.js';
import { AuthGraphMiddleware } from './middleware.js';

const FROZEN_OPTIONS_TOKEN = UMS_SDK_OPTIONS;

function defaultOptions(input?: UmsSdkModuleOptions): UmsSdkModuleOptions {
  return {
    schemaCompatibility: input?.schemaCompatibility ?? '>=1.0.0 <2.0.0',
    mode: input?.mode ?? 'enforce',
    accessor: input?.accessor,
    validator: input?.validator,
    logger: input?.logger,
    umsBaseUrl: input?.umsBaseUrl
  };
}

function buildProviders(options: UmsSdkModuleOptions): Provider[] {
  return [
    { provide: FROZEN_OPTIONS_TOKEN, useValue: options },
    {
      provide: UMS_AUTH_GRAPH_ACCESSOR,
      useValue: options.accessor ?? new AsyncLocalAuthGraphAccessor()
    },
    {
      provide: UMS_AUTHORIZATION_VALIDATOR,
      useValue: options.validator ?? new AuthorizationValidator()
    },
    {
      provide: UMS_AUTHORIZATION_LOGGER,
      useValue: options.logger ?? null
    },
    UmsAuthGuard,
    AuthorizationDeniedFilter,
    AuthGraphMiddleware
  ];
}

const EXPORTS = [
  FROZEN_OPTIONS_TOKEN,
  UMS_AUTH_GRAPH_ACCESSOR,
  UMS_AUTHORIZATION_VALIDATOR,
  UMS_AUTHORIZATION_LOGGER,
  UmsAuthGuard,
  AuthorizationDeniedFilter,
  AuthGraphMiddleware
] as const;

/**
 * Single configuration entry point for the UMS SDK NestJS distribution.
 *
 * Example:
 * ```ts
 * @Module({
 *   imports: [UmsSdkModule.forRoot({ mode: 'enforce' })],
 *   providers: [
 *     { provide: APP_GUARD, useClass: UmsAuthGuard },
 *     { provide: APP_FILTER, useClass: AuthorizationDeniedFilter }
 *   ]
 * })
 * export class AppModule implements NestModule {
 *   configure(consumer: MiddlewareConsumer) {
 *     consumer.apply(AuthGraphMiddleware).forRoutes('*');
 *   }
 * }
 * ```
 */
@Global()
@Module({})
export class UmsSdkModule {
  static forRoot(options?: UmsSdkModuleOptions): DynamicModule {
    const resolved = defaultOptions(options);
    return {
      module: UmsSdkModule,
      providers: buildProviders(resolved),
      exports: [...EXPORTS]
    };
  }

  static forRootAsync(options: UmsSdkModuleAsyncOptions): DynamicModule {
    const optionsProvider: Provider = {
      provide: FROZEN_OPTIONS_TOKEN,
      useFactory: async (...args: unknown[]) => defaultOptions(await options.useFactory(...args)),
      inject: options.inject ?? []
    };

    return {
      module: UmsSdkModule,
      imports: options.imports ?? [],
      providers: [
        optionsProvider,
        {
          provide: UMS_AUTH_GRAPH_ACCESSOR,
          useFactory: (resolved: UmsSdkModuleOptions) => resolved.accessor ?? new AsyncLocalAuthGraphAccessor(),
          inject: [FROZEN_OPTIONS_TOKEN]
        },
        {
          provide: UMS_AUTHORIZATION_VALIDATOR,
          useFactory: (resolved: UmsSdkModuleOptions) => resolved.validator ?? new AuthorizationValidator(),
          inject: [FROZEN_OPTIONS_TOKEN]
        },
        {
          provide: UMS_AUTHORIZATION_LOGGER,
          useFactory: (resolved: UmsSdkModuleOptions) => resolved.logger ?? null,
          inject: [FROZEN_OPTIONS_TOKEN]
        },
        UmsAuthGuard,
        AuthorizationDeniedFilter,
        AuthGraphMiddleware
      ],
      exports: [...EXPORTS]
    };
  }
}
