# CP-04: Multi-Tenant Repository with Row-Level Security

| Field | Value |
|-------|-------|
| **Pattern ID** | CP-04 |
| **Type** | Data Access / Security |
| **ADR Reference** | [ADR-0010: Multi-Tenancy RLS Strategy](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/core/0010-multi-tenancy-architecture-strategy.md) |
| **Language** | TypeScript / NestJS / TypeORM + PostgreSQL |
| **Last Review** | 2026-05-15 | ---

## Intent

Enforce tenant data isolation at the**database level**using Row-Level Security (RLS), preventing any query — regardless of ORM or direct SQL — from returning rows belonging to a different tenant. The application sets the tenant context once per request; the DB enforces isolation automatically.

---

## PostgreSQL RLS Setup

```sql
-- Enable RLS on tenant-scoped tables
ALTER TABLE ums.users ENABLE ROW LEVEL SECURITY;
ALTER TABLE ums.roles ENABLE ROW LEVEL SECURITY;
ALTER TABLE ums.permissions ENABLE ROW LEVEL SECURITY;

-- Policy: users can only see their own tenant's rows
CREATE POLICY tenant_isolation ON ums.users
 USING (tenant_id = current_setting('app.current_tenant_id', true)::uuid);

CREATE POLICY tenant_isolation ON ums.roles
 USING (tenant_id = current_setting('app.current_tenant_id', true)::uuid);

-- Service account must NOT be superuser (superusers bypass RLS)
-- GRANT SELECT, INSERT, UPDATE, DELETE ON ums.users TO ums_app;
```

---

## TypeORM Subscriber — Set Tenant Context

```typescript
// infrastructure/database/subscribers/tenant-context.subscriber.ts
@Injectable()
export class TenantContextSubscriber implements EntitySubscriberInterface {
 constructor(@InjectDataSource() private readonly ds: DataSource,
 private readonly tenantContext: TenantContext,) {
 this.ds.subscribers.push(this);
 }

 async beforeQuery(event: BeforeQueryEvent<unknown>): Promise<void> {
 const tenantId = this.tenantContext.getTenantId();
 if (tenantId) {
 await event.connection.query(`SELECT set_config('app.current_tenant_id', $1, true)`,
 [tenantId],);
 }
 }
}
```

---

## Tenant Context (Request-Scoped)

```typescript
// infrastructure/context/tenant-context.ts
@Injectable({ scope: Scope.REQUEST })
export class TenantContext {
 private _tenantId: string | null = null;

 setTenantId(tenantId: string): void {
 this._tenantId = tenantId;
 }

 getTenantId(): string | null {
 return this._tenantId;
 }
}
```

---

## JWT Guard — Extract and Set Tenant

```typescript
// infrastructure/guards/tenant.guard.ts
@Injectable()
export class TenantGuard implements CanActivate {
 constructor(private readonly jwtService: JwtService,
 private readonly tenantContext: TenantContext,) {}

 canActivate(context: ExecutionContext): boolean {
 const request = context.switchToHttp().getRequest();
 const token = request.headers.authorization?.replace('Bearer ', '');

 if (!token) throw new UnauthorizedException();

 const payload = this.jwtService.verify(token);
 if (!payload.tenantId) throw new ForbiddenException('No tenant claim in token.');

 this.tenantContext.setTenantId(payload.tenantId);
 return true;
 }
}
```

---

## Repository Usage

```typescript
// infrastructure/database/repositories/typeorm-user.repository.ts
@Injectable()
export class TypeOrmUserRepository implements IUserRepository {
 constructor(@InjectRepository(UserEntity)
 private readonly repo: Repository<UserEntity>,
 // TenantContext is NOT injected here — RLS enforces isolation at DB level) {}

 async findById(id: string): Promise<User | null> {
 // No WHERE tenant_id = ? needed — RLS filters automatically
 const entity = await this.repo.findOneBy({ id });
 return entity ? UserMapper.toDomain(entity) : null;
 }

 async findAll(): Promise<User[]> {
 // Returns only rows for the current tenant — RLS enforces this
 const entities = await this.repo.find();
 return entities.map(UserMapper.toDomain);
 }
}
```

---

## Testing RLS in Integration Tests

```typescript
// test/integration/rls.spec.ts
describe('RLS enforcement', () => {
 it('does not leak cross-tenant data', async () => {
 // Create users for two tenants
 await createUser(ds, { tenantId: 'tenant-A', email: 'a@a.com' });
 await createUser(ds, { tenantId: 'tenant-B', email: 'b@b.com' });

 // Set context to tenant-A
 await ds.query(`SELECT set_config('app.current_tenant_id', 'tenant-A', true)`);

 const users = await ds.getRepository(UserEntity).find();

 expect(users).toHaveLength(1);
 expect(users[0].email).toBe('a@a.com'); // tenant-B row must not appear
 });
});
```

---

## Security Rules

1. The DB service account (`ums_app`) must**not**be a superuser — superusers bypass RLS
2. The `app.current_tenant_id` setting is transaction-local (`true` as third argument to `set_config`) — it resets after each transaction
3. Admin / migration tasks use a separate migration role (`ums_migrator`) with `BYPASSRLS` privilege
4. Every integration test must assert that cross-tenant queries return zero rows

---

## What NOT to Do

```typescript
// Manual WHERE tenant_id filter — fragile, easy to forget
const users = await this.repo.find({ where: { tenantId: ctx.getTenantId() } });

// Superuser connection for application queries — bypasses RLS
// DATABASE_URL=postgres://postgres:pass@db/ums ← postgres is superuser

// Setting tenant context after queries have started
// TenantContext must be set in guard, before any repository call
```

---

**[Back to Canonical Patterns](./index.md)** | **[Back to Architecture Portal](../../index.md)**
