import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { User } from '../../../core/entities/user.entity';
import { IUserRepository } from '../../../core/interfaces/user-repository.interface';
import { UserDbEntity } from '../entities/user-db.entity';

@Injectable()
export class TypeOrmUserRepository implements IUserRepository {
  constructor(
    @InjectRepository(UserDbEntity)
    private readonly repository: Repository<UserDbEntity>,
  ) {}

  private toDomain(dbUser: UserDbEntity): User {
    return new User(
      dbUser.id,
      dbUser.username,
      dbUser.email,
      dbUser.passwordHash,
      dbUser.role,
      dbUser.createdAt,
      dbUser.updatedAt,
    );
  }

  private toPersistence(domainUser: User): UserDbEntity {
    const dbUser = new UserDbEntity();
    dbUser.id = domainUser.id;
    dbUser.username = domainUser.username;
    dbUser.email = domainUser.email;
    dbUser.passwordHash = domainUser.passwordHash;
    dbUser.role = domainUser.role;
    dbUser.createdAt = domainUser.createdAt;
    dbUser.updatedAt = domainUser.updatedAt;
    return dbUser;
  }

  public async save(user: User): Promise<User> {
    const persistenceModel = this.toPersistence(user);
    const saved = await this.repository.save(persistenceModel);
    return this.toDomain(saved);
  }

  public async findByEmail(email: string): Promise<User | null> {
    const found = await this.repository.findOne({ where: { email } });
    return found ? this.toDomain(found) : null;
  }

  public async findByUsername(username: string): Promise<User | null> {
    const found = await this.repository.findOne({ where: { username } });
    return found ? this.toDomain(found) : null;
  }

  public async findById(id: string): Promise<User | null> {
    const found = await this.repository.findOne({ where: { id } });
    return found ? this.toDomain(found) : null;
  }

  public async findAll(): Promise<User[]> {
    const users = await this.repository.find();
    return users.map((user) => this.toDomain(user));
  }
}
