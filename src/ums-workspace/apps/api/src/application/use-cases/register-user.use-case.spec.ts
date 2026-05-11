import { Test, TestingModule } from '@nestjs/testing';
import { ConflictException } from '@nestjs/common';
import { RegisterUserUseCase } from './register-user.use-case';
import { User } from '../../core/entities/user.entity';
import {
  IUserRepository,
  IPasswordHasher,
  USER_REPOSITORY_TOKEN,
  USER_PASSWORD_HASHER_TOKEN,
} from '../../core/interfaces/user-repository.interface';
import { RegisterUserDto } from '../dtos/register-user.dto';

describe('RegisterUserUseCase (Clean Architecture Unit Test)', () => {
  let useCase: RegisterUserUseCase;
  let mockUserRepository: jest.Mocked<IUserRepository>;
  let mockPasswordHasher: jest.Mocked<IPasswordHasher>;

  beforeEach(async () => {
    // 1. Create robust mocks for interfaces
    mockUserRepository = {
      findByEmail: jest.fn(),
      findByUsername: jest.fn(),
      save: jest.fn(),
    } as any;

    mockPasswordHasher = {
      hash: jest.fn(),
      compare: jest.fn(),
    } as any;

    // 2. Build NestJS native TestingModule to mirror production Dependency Injection
    const module: TestingModule = await Test.createTestingModule({
      providers: [
        RegisterUserUseCase,
        {
          provide: USER_REPOSITORY_TOKEN,
          useValue: mockUserRepository,
        },
        {
          provide: USER_PASSWORD_HASHER_TOKEN,
          useValue: mockPasswordHasher,
        },
      ],
    }).compile();

    useCase = module.get<RegisterUserUseCase>(RegisterUserUseCase);
  });

  it('should be defined', () => {
    expect(useCase).toBeDefined();
  });

  it('should successfully register a user when email and username are unique', async () => {
    const dto: RegisterUserDto = {
      username: 'test_user',
      email: 'test@ums.com',
      password: 'SecurePassword123!',
    };

    mockUserRepository.findByEmail.mockResolvedValue(null);
    mockUserRepository.findByUsername.mockResolvedValue(null);
    mockPasswordHasher.hash.mockResolvedValue('hashed_password_hash');
    mockUserRepository.save.mockImplementation(async (user: User) => user);

    const result = await useCase.execute(dto);

    expect(result).toBeDefined();
    expect(result.username).toBe(dto.username);
    expect(result.email).toBe(dto.email);
    expect(result.passwordHash).toBe('hashed_password_hash');
    expect(mockUserRepository.findByEmail).toHaveBeenCalledWith(dto.email);
    expect(mockUserRepository.findByUsername).toHaveBeenCalledWith(
      dto.username,
    );
    expect(mockPasswordHasher.hash).toHaveBeenCalledWith(dto.password);
    expect(mockUserRepository.save).toHaveBeenCalled();
  });

  it('should throw ConflictException if email already exists', async () => {
    const dto: RegisterUserDto = {
      username: 'test_user',
      email: 'existing@ums.com',
      password: 'SecurePassword123!',
    };

    const existingUser = User.create(
      'uuid-123',
      'another_username',
      'existing@ums.com',
      'hash',
      'user',
    );
    mockUserRepository.findByEmail.mockResolvedValue(existingUser);

    await expect(useCase.execute(dto)).rejects.toThrow(ConflictException);
    expect(mockUserRepository.findByEmail).toHaveBeenCalledWith(dto.email);
    expect(mockUserRepository.save).not.toHaveBeenCalled();
  });

  it('should throw ConflictException if username already exists', async () => {
    const dto: RegisterUserDto = {
      username: 'existing_username',
      email: 'test@ums.com',
      password: 'SecurePassword123!',
    };

    mockUserRepository.findByEmail.mockResolvedValue(null);
    const existingUser = User.create(
      'uuid-123',
      'existing_username',
      'another@ums.com',
      'hash',
      'user',
    );
    mockUserRepository.findByUsername.mockResolvedValue(existingUser);

    await expect(useCase.execute(dto)).rejects.toThrow(ConflictException);
    expect(mockUserRepository.findByUsername).toHaveBeenCalledWith(
      dto.username,
    );
    expect(mockUserRepository.save).not.toHaveBeenCalled();
  });
});
