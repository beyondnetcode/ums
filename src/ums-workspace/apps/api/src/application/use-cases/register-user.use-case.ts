import { ConflictException, Inject, Injectable } from '@nestjs/common';
import { User } from '../../core/entities/user.entity';
import {
  IUserRepository,
  IPasswordHasher,
  USER_REPOSITORY_TOKEN,
  USER_PASSWORD_HASHER_TOKEN,
} from '../../core/interfaces/user-repository.interface';
import { RegisterUserDto } from '../dtos/register-user.dto';

@Injectable()
export class RegisterUserUseCase {
  constructor(
    @Inject(USER_REPOSITORY_TOKEN)
    private readonly userRepository: IUserRepository,
    @Inject(USER_PASSWORD_HASHER_TOKEN)
    private readonly passwordHasher: IPasswordHasher,
  ) {}

  public async execute(dto: RegisterUserDto): Promise<User> {
    // 1. Check if email already exists
    const existingEmail = await this.userRepository.findByEmail(dto.email);
    if (existingEmail) {
      throw new ConflictException('A user with this email already exists.');
    }

    // 2. Check if username already exists
    const existingUsername = await this.userRepository.findByUsername(
      dto.username,
    );
    if (existingUsername) {
      throw new ConflictException('A user with this username already exists.');
    }

    // 3. Hash password securely
    const passwordHash = await this.passwordHasher.hash(dto.password);

    // 4. Create and save entity
    // Generate simple UUID or random string for demonstration, or we can use crypto.randomUUID()
    const id =
      typeof crypto !== 'undefined' && crypto.randomUUID
        ? crypto.randomUUID()
        : Math.random().toString(36).substring(2, 15);
    const user = User.create(id, dto.username, dto.email, passwordHash, 'user');

    return this.userRepository.save(user);
  }
}
