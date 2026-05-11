import { NestFactory } from '@nestjs/core';
import { ValidationPipe } from '@nestjs/common';
import helmet from 'helmet';
import { AppModule } from './app.module';
import { LoggingInterceptor } from 'libs-aop';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  // 1. Secure HTTP Headers using Helmet (OWASP A05: Security Misconfiguration)
  app.use(helmet());

  // 2. Enable CORS with defensive policy (OWASP A01: Broken Access Control)
  app.enableCors({
    origin: process.env.ALLOWED_ORIGINS
      ? process.env.ALLOWED_ORIGINS.split(',')
      : 'http://localhost:5173',
    methods: 'GET,HEAD,PUT,PATCH,POST,DELETE',
    credentials: true,
  });

  // 3. Global prefix for API endpoints
  app.setGlobalPrefix('api');

  // 4. Global Validation Pipes with strict filtering (OWASP A03: Injection)
  app.useGlobalPipes(
    new ValidationPipe({
      whitelist: true, // Strips any properties not explicitly defined in DTOs
      forbidNonWhitelisted: true, // Throws an error if any non-whitelisted property is sent
      transform: true, // Automatically converts request payloads to typed DTO classes
    }),
  );

  // 5. Global Observability Interceptor (AOP)
  app.useGlobalInterceptors(new LoggingInterceptor());

  const port = process.env.PORT || 3000;
  await app.listen(port);
  console.log(
    `[UMS API] Service successfully listening on: http://localhost:${port}/api`,
  );
}
bootstrap();
