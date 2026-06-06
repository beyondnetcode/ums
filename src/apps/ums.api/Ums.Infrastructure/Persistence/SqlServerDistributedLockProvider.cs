using Microsoft.EntityFrameworkCore;

namespace Ums.Infrastructure.Persistence;

public sealed class SqlServerDistributedLockProvider : IDistributedLockProvider
{
    public async Task<IAsyncDisposable> AcquireLockAsync(
        DbContext dbContext, 
        string resourceName, 
        TimeSpan timeout, 
        CancellationToken cancellationToken = default)
    {
        var timeoutMs = (int)timeout.TotalMilliseconds;

        // Note: The caller must ensure that the DbContext has an open connection,
        // otherwise EF Core will close the connection immediately after ExecuteSqlRawAsync,
        // which drops the Session-level sp_getapplock.
        await dbContext.Database.ExecuteSqlRawAsync(
            "EXEC sp_getapplock @Resource = {0}, @LockMode = N'Exclusive', @LockOwner = N'Session', @LockTimeout = {1};",
            new object[] { resourceName, timeoutMs },
            cancellationToken);

        return new SqlServerLockScope(dbContext, resourceName);
    }

    private sealed class SqlServerLockScope : IAsyncDisposable
    {
        private readonly DbContext _dbContext;
        private readonly string _resourceName;

        public SqlServerLockScope(DbContext dbContext, string resourceName)
        {
            _dbContext = dbContext;
            _resourceName = resourceName;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                // Release the lock when the scope is disposed
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "EXEC sp_releaseapplock @Resource = {0}, @LockOwner = N'Session';",
                    new object[] { _resourceName });
            }
            catch
            {
                // Suppress release errors so we don't mask original exceptions during disposal.
                // The lock will naturally release when the connection is closed.
            }
        }
    }
}
