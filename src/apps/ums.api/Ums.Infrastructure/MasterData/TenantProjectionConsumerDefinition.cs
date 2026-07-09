using MassTransit;

namespace Ums.Infrastructure.MasterData;

/// <summary>
/// Binds <see cref="TenantProjectionConsumer"/> to the canonical queue
/// <c>ums.tenant-projection</c> (ADR-0107 topology) with exponential retry before dead-lettering.
/// </summary>
public sealed class TenantProjectionConsumerDefinition : ConsumerDefinition<TenantProjectionConsumer>
{
    public TenantProjectionConsumerDefinition()
    {
        EndpointName = "ums.tenant-projection";
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<TenantProjectionConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r =>
            r.Exponential(5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2)));
    }
}
