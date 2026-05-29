# CP-03: Result Pattern (Either Monad)

| Field | Value |
|-------|-------|
| **Pattern ID** | CP-03 |
| **Type** | Error Handling / Functional |
| **ADR Reference** | [ADR-0038: Result Pattern TS Implementation](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/nodejs/0038-error-handling-result-pattern-strategy.md) |
| **Language** | TypeScript / NestJS |
| **Last Review** | 2026-05-15 |

---

## Intent

Replace exception-based control flow for **expected business failures** with an explicit `Result<T>` type that forces callers to handle both success and failure paths. Exceptions are reserved for **unexpected infrastructure failures** only.

---

## Core Type

```typescript
// core/primitives/result.ts
export class Result<T> {
  private constructor(
    private readonly _isSuccess: boolean,
    private readonly _error: string | undefined,
    private readonly _value: T | undefined,
  ) {}

  get isSuccess(): boolean { return this._isSuccess; }
  get isFailure(): boolean { return !this._isSuccess; }

  get value(): T {
    if (!this._isSuccess) throw new Error('Cannot access value of a failed Result.');
    return this._value as T;
  }

  get error(): string {
    if (this._isSuccess) throw new Error('Cannot access error of a successful Result.');
    return this._error as string;
  }

  static ok<T>(value: T): Result<T> {
    return new Result<T>(true, undefined, value);
  }

  static fail<T>(error: string): Result<T> {
    return new Result<T>(false, error, undefined);
  }

  // Monadic chain — applies fn only on success
  map<U>(fn: (value: T) => U): Result<U> {
    if (this.isFailure) return Result.fail<U>(this.error);
    return Result.ok(fn(this.value));
  }

  // Async chain
  async flatMapAsync<U>(fn: (value: T) => Promise<Result<U>>): Promise<Result<U>> {
    if (this.isFailure) return Result.fail<U>(this.error);
    return fn(this.value);
  }
}
```

---

## Domain Usage

```typescript
// core/entities/user.aggregate.ts
deactivate(): Result<void> {
  if (this._status === 'INACTIVE') {
    return Result.fail('User is already inactive.');  // expected business rule
  }
  this._status = 'INACTIVE';
  this.addDomainEvent(new UserDeactivatedEvent(this.id));
  return Result.ok(undefined);
}
```

---

## Use Case Usage

```typescript
// application/use-cases/deactivate-user.use-case.ts
@Injectable()
export class DeactivateUserUseCase {
  constructor(private readonly userRepository: IUserRepository) {}

  async execute(command: DeactivateUserCommand): Promise<Result<void>> {
    const user = await this.userRepository.findById(command.userId);
    if (!user) return Result.fail('User not found.');  // not found = expected failure

    const result = user.deactivate();
    if (result.isFailure) return result;

    await this.userRepository.save(user);
    return Result.ok(undefined);
  }
}
```

---

## HTTP Controller Mapping

```typescript
// infrastructure/controllers/user.controller.ts
@Patch(':id/deactivate')
async deactivate(@Param('id') id: string): Promise<void> {
  const result = await this.deactivateUserUseCase.execute({ userId: id });

  if (result.isFailure) {
    const msg = result.error;
    if (msg === 'User not found.') throw new NotFoundException(msg);
    throw new UnprocessableEntityException(msg);  // business rule violation → 422
  }
  // 200 OK implicit — no body for deactivation
}
```

---

## Error Classification

| Category | Handling | HTTP Status |
|----------|----------|------------|
| Not found (expected) | `Result.fail('... not found.')` | 404 |
| Business rule violation | `Result.fail('...')` | 422 |
| Validation (input) | NestJS `ValidationPipe` + class-validator | 400 |
| Infrastructure failure | `throw new Error(...)` (unexpected) | 500 |
| Auth / permission | `throw new ForbiddenException(...)` | 403 |

---

## Chaining Example

```typescript
// Chaining multiple Result-returning operations
const result = await userResult
  .flatMapAsync((user) => this.validateTenantAccess(user, tenantId))
  .then((r) => r.flatMapAsync((user) => this.assignRole(user, roleId)));

if (result.isFailure) {
  return Result.fail(result.error);
}
```

---

## What NOT to Do

```typescript
// ❌ Throwing for business rules
throw new Error('User already exists');       // Use Result.fail(...)

// ❌ Returning null for not-found
return null;                                  // Use Result.fail('... not found.')

// ❌ Ignoring Result
const r = user.deactivate();
await this.repo.save(user);                   // Must check r.isFailure first

// ❌ Using Result for infrastructure errors
try {
  return Result.fail('DB connection lost');   // This should throw — it's unexpected
} catch {}
```

---

**[Back to Canonical Patterns](./index.md)** | **[Back to Architecture Portal](../../index.md)**
