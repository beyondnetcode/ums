using System;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using BeyondNetCode.Shell.Ddd.Interfaces;
using Ums.Domain.Identity;

namespace Ums.Infrastructure.Persistence;

public class UmsPlatformDbContextFactory : IDesignTimeDbContextFactory<UmsPlatformDbContext>
{
    public UmsPlatformDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UmsPlatformDbContext>();
        
        // This is strictly used for EF Core tool generation at design time.
        optionsBuilder.UseNpgsql("Host=localhost;Database=ums_test;Username=postgres;Password=postgres");

        return new UmsPlatformDbContext(optionsBuilder.Options, new DesignTimeTenantContext(), new DesignTimePublishEndpoint());
    }

    private class DesignTimeTenantContext : ITenantContext
    {
        public Guid? OrganizationId => null;
        public Guid? UserId => null;
        public string? SessionId => null;
        public string? Email => null;
        public string? Roles => null;
        public string? Permissions => null;
        public bool IsAuthenticated => false;
        public string Token => string.Empty;
        public Guid? OriginalTenantId => null;
        public bool IsInternalAdmin => false;

        public void SetOrganizationId(Guid tenantId) { }
        public void EnableCrossTenantAccess() { }
        public void DisableCrossTenantAccess() { }
        public void Initialize(Guid tenantId, bool isInternalAdmin) { }
    }

    private class DesignTimePublishEndpoint : IPublishEndpoint
    {
        public Task Publish<T>(T message, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
        public Task Publish<T>(T message, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
        public Task Publish<T>(T message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
        public Task Publish(object message, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish(object message, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish(object message, Type messageType, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish<T>(object values, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
        public Task Publish<T>(object values, IPipe<PublishContext<T>> publishPipe, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
        public Task Publish<T>(object values, IPipe<PublishContext> publishPipe, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;

        public ConnectHandle ConnectPublishObserver(IPublishObserver observer) => throw new NotImplementedException();
    }
}
