# CP-01: Hexagonal Architecture — Port & Adapter Pattern

| Field | Value |
|-------|-------|
| **Pattern ID** | CP-01 |
| **Type** | Structural / Architectural |
| **ADR Reference** | [ADR-0002: Clean Architecture NestJS](../../../../../reference/architecture/adrs/nodejs/0002-clean-architecture-nestjs.md) |
| **Language** | TypeScript / NestJS |
| **Last Review** | 2026-05-15 |

---

## Intent

Isolate the **domain** from all external concerns (HTTP, database, message bus, third-party APIs) using explicit ports (interfaces) and adapters (implementations). The domain layer has zero external dependencies.

---

## Layer Map

```
src/
├── core/                       ← Domain Layer (zero external imports)
│   ├── entities/               ← Aggregates, Entities
│   ├── value-objects/          ← Immutable value types
│   ├── interfaces/             ← Ports (repository, event bus, etc.)
│   └── domain-services/        ← Pure domain logic with no I/O
│
├── application/                ← Application Layer (orchestration only)
│   ├── use-cases/              ← One class per use case
│   ├── commands/               ← Command DTOs
│   ├── queries/                ← Query DTOs + Read Models
│   └── dtos/                   ← Input/Output contracts
│
└── infrastructure/             ← Adapters (I/O implementations)
    ├── controllers/            ← HTTP adapter (NestJS)
    ├── database/
    │   └── repositories/       ← TypeORM / in-memory adapters
    ├── messaging/              ← Dapr / Redis pub/sub adapters
    └── [module].module.ts      ← DI wiring — selects which adapter to inject
```

---

## Port Definition (Domain)

```typescript
// core/interfaces/user-repository.interface.ts
export abstract class IUserRepository {
  abstract save(user: User): Promise<void>;
  abstract findById(id: string): Promise<User | null>;
  abstract findByEmail(email: string, tenantId: string): Promise<User | null>;
  abstract delete(id: string): Promise<void>;
}
```

> Ports are abstract classes (not interfaces) so NestJS DI can use them as injection tokens.

---

## Adapter Implementation (Infrastructure)

```typescript
// infrastructure/database/repositories/typeorm-user.repository.ts
@Injectable()
export class TypeOrmUserRepository implements IUserRepository {
  constructor(
    @InjectRepository(UserEntity)
    private readonly repo: Repository<UserEntity>,
  ) {}

  async save(user: User): Promise<void> {
    await this.repo.save(UserMapper.toEntity(user));
  }

  async findById(id: string): Promise<User | null> {
    const entity = await this.repo.findOneBy({ id });
    return entity ? UserMapper.toDomain(entity) : null;
  }

  async findByEmail(email: string, tenantId: string): Promise<User | null> {
    const entity = await this.repo.findOneBy({ email, tenantId });
    return entity ? UserMapper.toDomain(entity) : null;
  }

  async delete(id: string): Promise<void> {
    await this.repo.delete(id);
  }
}
```

---

## Use Case (Application Layer)

```typescript
// application/use-cases/register-user.use-case.ts
@Injectable()
export class RegisterUserUseCase {
  constructor(private readonly userRepository: IUserRepository) {}

  async execute(command: RegisterUserCommand): Promise<Result<User>> {
    const existing = await this.userRepository.findByEmail(command.email, command.tenantId);
    if (existing) return Result.fail('Email already registered in this tenant.');

    const userResult = User.create(command.id, command.tenantId, command.email, command.displayName);
    if (userResult.isFailure) return userResult;

    await this.userRepository.save(userResult.value);
    return userResult;
  }
}
```

---

## DI Wiring (Module)

```typescript
// infrastructure/user.module.ts
@Module({
  providers: [
    RegisterUserUseCase,
    // Swap adapter here without touching domain or application layers:
    { provide: IUserRepository, useClass: TypeOrmUserRepository },
    // For tests: { provide: IUserRepository, useClass: InMemoryUserRepository },
  ],
  exports: [RegisterUserUseCase],
})
export class UserModule {}
```

---

## Rules

1. **Domain imports nothing** from `infrastructure/` or `application/`
2. **Application imports** only from `core/` (entities, ports, domain services)
3. **Infrastructure imports** from `application/` and `core/`, never the reverse
4. **Adapters implement ports**, never bypass them
5. Use `eslint-plugin-boundaries` to enforce layer rules in CI

---

## Dependency Rule Diagram

```
HTTP Request
     │
     ▼
[Controller]  ──imports──►  [UseCase]  ──imports──►  [Port (IUserRepository)]
     │                                                        ▲
     │                                              [TypeOrmAdapter implements]
     │                                                        │
     └────────────────────────────────────────────────────────┘
                                    (NestJS DI resolves at runtime)
```

---

**[Back to Canonical Patterns](./index.md)** | **[Back to Architecture Portal](../../index.md)**
