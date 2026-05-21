namespace Ums.Infrastructure;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Audit.AuditRecord;
using Ums.Domain.Approvals;
using Ums.Domain.Authorization;
using Ums.Domain.IGA;
using Ums.Domain.Identity;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<ITenantContext, TenantContext>();

        services.AddSingleton<InMemoryTenantRepository>();
        services.AddSingleton<ITenantRepository>(sp => sp.GetRequiredService<InMemoryTenantRepository>());

        services.AddSingleton<InMemoryUserAccountRepository>();
        services.AddSingleton<IUserAccountRepository>(sp => sp.GetRequiredService<InMemoryUserAccountRepository>());

        services.AddSingleton<InMemoryProfileRepository>();
        services.AddSingleton<IProfileRepository>(sp => sp.GetRequiredService<InMemoryProfileRepository>());

        services.AddSingleton<InMemorySystemSuiteRepository>();
        services.AddSingleton<ISystemSuiteRepository>(sp => sp.GetRequiredService<InMemorySystemSuiteRepository>());

        services.AddSingleton<InMemoryPermissionTemplateRepository>();
        services.AddSingleton<IPermissionTemplateRepository>(sp => sp.GetRequiredService<InMemoryPermissionTemplateRepository>());

        services.AddSingleton<InMemoryAuditRecordRepository>();
        services.AddSingleton<IAuditRecordRepository>(sp => sp.GetRequiredService<InMemoryAuditRecordRepository>());

        services.AddSingleton<InMemoryApprovalWorkflowRepository>();
        services.AddSingleton<IApprovalWorkflowRepository>(sp => sp.GetRequiredService<InMemoryApprovalWorkflowRepository>());

        services.AddSingleton<InMemoryApprovalRequestRepository>();
        services.AddSingleton<IApprovalRequestRepository>(sp => sp.GetRequiredService<InMemoryApprovalRequestRepository>());

        services.AddSingleton<InMemoryDocumentTypeRepository>();
        services.AddSingleton<IDocumentTypeRepository>(sp => sp.GetRequiredService<InMemoryDocumentTypeRepository>());

        services.AddSingleton<InMemoryUserDocumentRepository>();
        services.AddSingleton<IUserDocumentRepository>(sp => sp.GetRequiredService<InMemoryUserDocumentRepository>());

        services.AddSingleton<InMemoryAccessEnforcementPolicyRepository>();
        services.AddSingleton<IAccessEnforcementPolicyRepository>(sp => sp.GetRequiredService<InMemoryAccessEnforcementPolicyRepository>());

        services.AddSingleton<InMemoryNotificationRuleRepository>();
        services.AddSingleton<INotificationRuleRepository>(sp => sp.GetRequiredService<InMemoryNotificationRuleRepository>());

        services.AddSingleton<InMemoryPromotionRequestRepository>();
        services.AddSingleton<IPromotionRequestRepository>(sp => sp.GetRequiredService<InMemoryPromotionRequestRepository>());

        services.AddSingleton<InMemoryRoleMaturityStatusRepository>();
        services.AddSingleton<IRoleMaturityStatusRepository>(sp => sp.GetRequiredService<InMemoryRoleMaturityStatusRepository>());

        return services;
    }
}
