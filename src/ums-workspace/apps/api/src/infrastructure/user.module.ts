import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { UserDbEntity } from './database/entities/user-db.entity';
import { TypeOrmUserRepository } from './database/repositories/typeorm-user.repository';
import { BcryptPasswordHasher } from './security/bcrypt-password-hasher';
import { RegisterUserUseCase } from '../application/use-cases/register-user.use-case';
import { UserController } from './controllers/user.controller';
import {
  USER_REPOSITORY_TOKEN,
  USER_PASSWORD_HASHER_TOKEN,
} from '../core/interfaces/user-repository.interface';

@Module({
  imports: [TypeOrmModule.forFeature([UserDbEntity])],
  controllers: [UserController],
  providers: [
    RegisterUserUseCase,
    {
      provide: USER_REPOSITORY_TOKEN,
      useClass: TypeOrmUserRepository,
    },
    {
      provide: USER_PASSWORD_HASHER_TOKEN,
      useClass: BcryptPasswordHasher,
    },
  ],
  exports: [RegisterUserUseCase],
})
export class UserModule {}
