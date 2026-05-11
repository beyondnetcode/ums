import { User } from '../entities/user.entity';

export interface IUserRepository {
  save(user: User): Promise<User>;
  findByEmail(email: string): Promise<User | null>;
  findByUsername(username: string): Promise<User | null>;
  findById(id: string): Promise<User | null>;
  findAll(): Promise<User[]>;
}

export const USER_REPOSITORY_TOKEN = 'IUserRepository';
export const USER_PASSWORD_HASHER_TOKEN = 'IPasswordHasher';
export interface IPasswordHasher {
  hash(password: string): Promise<string>;
  compare(password: string, hash: string): Promise<boolean>;
}
