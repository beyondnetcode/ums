using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ums.ReadModels;

public class ReadModelDbContextFactory : IDesignTimeDbContextFactory<ReadModelDbContext>
{
    public ReadModelDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReadModelDbContext>();
        // TODO: Replace with actual connection string or use configuration.
        optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=UmsReadModel;Username=postgres;Password=root");
        return new ReadModelDbContext(optionsBuilder.Options);
    }
}
