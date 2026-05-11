import { Module } from '@nestjs/common';
import { APP_GUARD } from '@nestjs/core';
import { TypeOrmModule } from '@nestjs/typeorm';
import { ThrottlerModule, ThrottlerGuard } from '@nestjs/throttler';
import { UserModule } from './infrastructure/user.module';
import { UserDbEntity } from './infrastructure/database/entities/user-db.entity';

@Module({
  imports: [
    // Database connection using environment or defaults
    TypeOrmModule.forRoot({
      type: 'postgres',
      host: process.env.DB_HOST || 'localhost',
      port: parseInt(process.env.DB_PORT || '5432', 10),
      username: process.env.DB_USERNAME || 'ums_user',
      password: process.env.DB_PASSWORD || 'ums_password',
      database: process.env.DB_NAME || 'ums_db',
      entities: [UserDbEntity],
      synchronize: process.env.NODE_ENV !== 'production', // Dynamic sync protection!
      logging: false,
    }),
    // Rate limiter module to prevent brute force / DDoS attacks
    ThrottlerModule.forRoot([
      {
        ttl: 60000, // 1 minute
        limit: 100, // max 100 requests per minute
      },
    ]),
    UserModule,
  ],
  providers: [
    {
      provide: APP_GUARD,
      useClass: ThrottlerGuard, // Global DDoS protection active
    },
  ],
})
export class AppModule {}
