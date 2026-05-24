namespace Ums.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// REC-04: Implementación SQL Server de <see cref="IUnitOfWorkScope"/>.
/// Envuelve operaciones de múltiples aggregates en una sola transacción EF Core.
/// </summary>
public sealed class UnitOfWorkScope(UmsPlatformDbContext dbContext) : IUnitOfWorkScope
{
    public async Task<ITransactionScope> BeginAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        return new EfTransactionScope(transaction);
    }

    private sealed class EfTransactionScope(IDbContextTransaction transaction) : ITransactionScope
    {
        private bool _completed;

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await transaction.CommitAsync(cancellationToken);
            _completed = true;
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            await transaction.RollbackAsync(cancellationToken);
            _completed = true;
        }

        public async ValueTask DisposeAsync()
        {
            // Auto-rollback if the scope was never committed
            if (!_completed)
            {
                try { await transaction.RollbackAsync(); }
                catch { /* best effort */ }
            }
            await transaction.DisposeAsync();
        }
    }
}

/// <summary>
/// REC-04: No-op scope para modos InMemory (sin transacciones reales).
/// </summary>
public sealed class NoOpUnitOfWorkScope : IUnitOfWorkScope
{
    public Task<ITransactionScope> BeginAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<ITransactionScope>(new NoOpTransactionScope());

    private sealed class NoOpTransactionScope : ITransactionScope
    {
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
