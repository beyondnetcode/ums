namespace Ums.Infrastructure.Persistence.Seeders;

using Microsoft.Extensions.DependencyInjection;
using Ums.Domain.IGA;
using Ums.Domain.IGA.PromotionRequest;
using Ums.Domain.IGA.RoleMaturityStatus;
using Ums.Domain.Kernel.ValueObjects;
using PromotionRequestAggregate = Ums.Domain.IGA.PromotionRequest.PromotionRequest;
using RoleMaturityStatusAggregate = Ums.Domain.IGA.RoleMaturityStatus.RoleMaturityStatus;

public static class IgaDevDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var promotionRepository = serviceProvider.GetService<IPromotionRequestRepository>();
        var inMemoryPromotionRepository = serviceProvider.GetService<InMemoryPromotionRequestRepository>();

        var maturityRepository = serviceProvider.GetService<IRoleMaturityStatusRepository>();
        var inMemoryMaturityRepository = serviceProvider.GetService<InMemoryRoleMaturityStatusRepository>();

        var actor = ActorId.Create(CoreDevDataSeeder.SystemActorId);
        var ransaTenantId = TenantId.Load(Guid.Parse(CoreDevDataSeeder.RansaTenantId));
        var adminUserId = UserId.Load(Guid.Parse(CoreDevDataSeeder.RansaAdminUserId));
        var analystUserId = UserId.Load(Guid.Parse(CoreDevDataSeeder.RansaAnalystUserId));

        // Promotion Requests
        var promotions = BuildSeedPromotions(ransaTenantId, adminUserId, analystUserId, actor);
        if (inMemoryPromotionRepository is not null)
            foreach (var promo in promotions) inMemoryPromotionRepository.Seed(promo);
        else if (promotionRepository is not null)
        {
            var existing = await promotionRepository.GetByTenantIdAsync(ransaTenantId.GetValue(), cancellationToken);
            if (existing.Count == 0)
            {
                foreach (var promo in promotions) await promotionRepository.AddAsync(promo, cancellationToken);
                await promotionRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }

        // Maturity Status
        var maturities = BuildSeedMaturities(ransaTenantId, adminUserId, actor);
        if (inMemoryMaturityRepository is not null)
            foreach (var mat in maturities) inMemoryMaturityRepository.Seed(mat);
        else if (maturityRepository is not null)
        {
            var existing = await maturityRepository.GetByTenantIdAsync(ransaTenantId.GetValue(), cancellationToken);
            if (existing.Count == 0)
            {
                foreach (var mat in maturities) await maturityRepository.AddAsync(mat, cancellationToken);
                await maturityRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }
    }

    // PromotionRequest.Create(TenantId, UserId, RoleId currentRoleId, RoleId targetRoleId, UserId managerId, TextValueObject? requestReason, ActorId)
    private static IReadOnlyList<PromotionRequestAggregate> BuildSeedPromotions(TenantId tenantId, UserId userId, UserId managerId, ActorId actor)
    {
        var req = PromotionRequestAggregate.Create(
            tenantId,
            userId,
            RoleId.Load(Guid.Parse(CoreDevDataSeeder.DemoAdminRoleId)),
            RoleId.Load(Guid.Parse(CoreDevDataSeeder.DemoAdminProfileId)),
            managerId,
            TextValueObject.Create("Requesting access to Warehouse Operator role for upcoming season"),
            actor);

        return req.IsSuccess ? new[] { req.Value } : Array.Empty<PromotionRequestAggregate>();
    }

    // RoleMaturityStatus.Create(TenantId, UserId, RoleId, RoleMaturityLevel, ActorId)
    private static IReadOnlyList<RoleMaturityStatusAggregate> BuildSeedMaturities(TenantId tenantId, UserId userId, ActorId actor)
    {
        var maturity = RoleMaturityStatusAggregate.Create(
            tenantId,
            userId,
            RoleId.Load(Guid.Parse(CoreDevDataSeeder.DemoAdminRoleId)),
            RoleMaturityLevel.Senior,
            actor);

        return maturity.IsSuccess ? new[] { maturity.Value } : Array.Empty<RoleMaturityStatusAggregate>();
    }
}
