import {
  Injectable,
  NestInterceptor,
  ExecutionContext,
  CallHandler,
  Logger,
} from '@nestjs/common';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable()
export class LoggingInterceptor implements NestInterceptor {
  private readonly logger = new Logger('AOP-Observability-Lib');

  intercept(context: ExecutionContext, next: CallHandler): Observable<any> {
    const request = context.switchToHttp().getRequest();
    const method = request.method;
    const url = request.url;
    const startTime = Date.now();

    return next.handle().pipe(
      tap({
        next: () => {
          const duration = Date.now() - startTime;
          this.logger.log(
            JSON.stringify({
              event: 'HTTP_REQUEST_SUCCESS',
              method,
              url,
              durationMs: duration,
              timestamp: new Date().toISOString(),
            }),
          );
        },
        error: (err: Error) => {
          const duration = Date.now() - startTime;
          this.logger.error(
            JSON.stringify({
              event: 'HTTP_REQUEST_FAILED',
              method,
              url,
              durationMs: duration,
              error: err.message,
              stack: err.stack,
              timestamp: new Date().toISOString(),
            }),
          );
        },
      }),
    );
  }
}
