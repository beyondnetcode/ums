namespace Ums.Presentation.GraphQL;

using HotChocolate.Execution.Configuration;
using Ums.Presentation.GraphQL.Approvals;
using Ums.Presentation.GraphQL.Audit;
using Ums.Presentation.GraphQL.Authorization;
using Ums.Presentation.GraphQL.IGA;
using Ums.Presentation.GraphQL.Identity;

public static class GraphQlServiceCollectionExtensions
{
    public static IRequestExecutorBuilder AddUmsGraphQl(this IServiceCollection services)
    {
        return services
            .AddGraphQLServer()
            .AddQueryType(d => d.Name("Query"))
            .AddTypeExtension<TenantQueries>()
            .AddTypeExtension<UserAccountQueries>()
            .AddTypeExtension<DelegationQueries>()
            .AddTypeExtension<ProfileQueries>()
            .AddTypeExtension<SystemSuiteQueries>()
            .AddTypeExtension<PermissionTemplateQueries>()
            .AddTypeExtension<AuditRecordQueries>()
            .AddTypeExtension<ApprovalWorkflowQueries>()
            .AddTypeExtension<ApprovalRequestQueries>()
            .AddTypeExtension<DocumentTypeQueries>()
            .AddTypeExtension<UserDocumentQueries>()
            .AddTypeExtension<AccessEnforcementPolicyQueries>()
            .AddTypeExtension<NotificationRuleQueries>()
            .AddTypeExtension<PromotionRequestQueries>()
            .AddTypeExtension<RoleMaturityStatusQueries>()
            .AddMaxExecutionDepthRule(12)
            .ModifyRequestOptions(options =>
            {
                options.ExecutionTimeout = TimeSpan.FromSeconds(10);
                options.IncludeExceptionDetails = false;
            });
    }
}
