# CP-02: Aggregate Root & Domain Event Pattern

| Field | Value |
|-------|-------|
| **Pattern ID** | CP-02 |
| **Type** | Domain / Tactical DDD |
| **ADR Reference** | [ADR-0029: Tactical DDD Primitives](../../../../../reference/architecture/adrs/nodejs/0029-tactical-ddd-primitives-library.md) |
| **Language** | TypeScript / NestJS |
| **Last Review** | 2026-05-15 |

---

## Intent

Encapsulate business invariants in an **Aggregate Root** and communicate state changes via **Domain Events** — ensuring consistency within the aggregate boundary and decoupling downstream side effects.

---

## Rules

1. All mutations go through the Aggregate Root — never directly modify child entities
2. Aggregate Root validates invariants before mutating state
3. State changes produce Domain Events; events are not persisted separately — the Outbox pattern does that (see CP-TE-04)
4. Aggregates are always reconstructed from primitives — no ORM entities in the domain layer
5. Factory method (`static create()`) is the only way to create a valid aggregate

---

## Base Class

```typescript
// core/primitives/aggregate-root.ts
export abstract class AggregateRoot<TId = string> {
  private readonly _domainEvents: DomainEvent[] = [];

  protected constructor(public readonly id: TId) {}

  protected addDomainEvent(event: DomainEvent): void {
    this._domainEvents.push(event);
  }

  pullDomainEvents(): DomainEvent[] {
    const events = [...this._domainEvents];
    this._domainEvents.length = 0;
    return events;
  }
}

export abstract class DomainEvent {
  public readonly occurredAt: Date = new Date();
  constructor(public readonly aggregateId: string) {}
}
```

---

## Domain Event Definition

```typescript
// core/events/user-registered.event.ts
export class UserRegisteredEvent extends DomainEvent {
  constructor(
    aggregateId: string,
    public readonly tenantId: string,
    public readonly email: string,
    public readonly displayName: string,
  ) {
    super(aggregateId);
  }
}
```

---

## Aggregate Root Implementation

```typescript
// core/entities/user.aggregate.ts
export class User extends AggregateRoot {
  private constructor(
    id: string,
    private readonly _tenantId: string,
    private _email: string,
    private _displayName: string,
    private _status: UserStatus,
    private readonly _createdAt: Date,
  ) {
    super(id);
  }

  get tenantId() { return this._tenantId; }
  get email() { return this._email; }
  get displayName() { return this._displayName; }
  get status() { return this._status; }
  get createdAt() { return this._createdAt; }

  // Factory — only valid way to create a User
  static create(
    id: string,
    tenantId: string,
    email: string,
    displayName: string,
  ): Result<User> {
    if (!email.includes('@')) return Result.fail('Invalid email format.');
    if (!displayName.trim()) return Result.fail('Display name cannot be blank.');

    const user = new User(id, tenantId, email, displayName, 'ACTIVE', new Date());
    user.addDomainEvent(new UserRegisteredEvent(id, tenantId, email, displayName));
    return Result.ok(user);
  }

  // Reconstitute from persistence — no events emitted
  static reconstitute(props: UserProps): User {
    return new User(
      props.id,
      props.tenantId,
      props.email,
      props.displayName,
      props.status,
      props.createdAt,
    );
  }

  deactivate(): Result<void> {
    if (this._status === 'INACTIVE') return Result.fail('User is already inactive.');
    this._status = 'INACTIVE';
    this.addDomainEvent(new UserDeactivatedEvent(this.id, this._tenantId));
    return Result.ok(undefined);
  }

  changeEmail(newEmail: string): Result<void> {
    if (!newEmail.includes('@')) return Result.fail('Invalid email format.');
    const previous = this._email;
    this._email = newEmail;
    this.addDomainEvent(new UserEmailChangedEvent(this.id, this._tenantId, previous, newEmail));
    return Result.ok(undefined);
  }
}
```

---

## Repository Contract (Outbox Integration)

```typescript
// infrastructure/database/repositories/typeorm-user.repository.ts
@Injectable()
export class TypeOrmUserRepository implements IUserRepository {
  async save(user: User): Promise<void> {
    const events = user.pullDomainEvents();

    await this.ds.transaction(async (em) => {
      await em.save(UserEntity, UserMapper.toEntity(user));

      for (const event of events) {
        await em.save(OutboxEventEntity, {
          aggregateId: user.id,
          aggregateType: 'User',
          eventType: event.constructor.name,
          payload: JSON.stringify(event),
          status: 'PENDING',
        });
      }
    });
  }
}
```

---

## Reconstitution (from DB)

```typescript
// infrastructure/database/mappers/user.mapper.ts
export class UserMapper {
  static toDomain(entity: UserEntity): User {
    return User.reconstitute({
      id: entity.id,
      tenantId: entity.tenantId,
      email: entity.email,
      displayName: entity.displayName,
      status: entity.status as UserStatus,
      createdAt: entity.createdAt,
    });
  }

  static toEntity(user: User): UserEntity {
    const entity = new UserEntity();
    entity.id = user.id;
    entity.tenantId = user.tenantId;
    entity.email = user.email;
    entity.displayName = user.displayName;
    entity.status = user.status;
    entity.createdAt = user.createdAt;
    return entity;
  }
}
```

---

## What NOT to Do

```typescript
// ❌ Direct mutation bypassing aggregate
user['_status'] = 'INACTIVE'; // Never — bypasses invariant check and event emission

// ❌ ORM entity in domain layer
import { UserEntity } from '../../infrastructure/...'; // Never — violates dependency rule

// ❌ Throwing exceptions for business rules
throw new Error('User is inactive'); // Never — use Result pattern (CP-03)
```

---

**[Back to Canonical Patterns](./index.md)** | **[Back to Architecture Portal](../../index.md)**
