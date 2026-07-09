using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ums.Infrastructure.MasterData;

/// <summary>Design-time factory for `dotnet ef migrations` on the Tenant projection context.</summary>
public sealed class TenantProjectionDbContextFactory : IDesignTimeDbContextFactory<TenantProjectionDbContext>
{
    public TenantProjectionDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TenantProjectionDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=ums;Username=postgres;Password=postgres")
            .Options;
        return new TenantProjectionDbContext(options);
    }
}
