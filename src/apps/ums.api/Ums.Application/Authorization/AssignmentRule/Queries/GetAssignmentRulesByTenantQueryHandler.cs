namespace Ums.Application.Authorization.AssignmentRule.Queries;

using Ums.Application.Authorization.AssignmentRule.DTOs;
using Ums.Domain.Authorization;

public sealed class GetAssignmentRulesByTenantQueryHandler : IQueryHandler<GetAssignmentRulesByTenantQuery, IReadOnlyList<AssignmentRuleDto>>
{
    private readonly ITemplateAssignmentRuleRepository _ruleRepository;

    public GetAssignmentRulesByTenantQueryHandler(ITemplateAssignmentRuleRepository ruleRepository)
    {
        _ruleRepository = ruleRepository;
    }

    public async Task<Result<IReadOnlyList<AssignmentRuleDto>>> Handle(
        GetAssignmentRulesByTenantQuery request,
        CancellationToken cancellationToken)
    {
        var rules = await _ruleRepository.GetByTenantIdAsync(request.TenantId, cancellationToken);

        var dtos = rules
            .OrderByDescending(r => r.Priority)
            .Select(r => new AssignmentRuleDto(
                r.Props.Id.GetValue(),
                r.TenantId.GetValue(),
                r.TemplateId.GetValue(),
                r.RoleId.GetValue(),
                r.Priority,
                r.Status.ToString()))
            .ToList();

        return Result<IReadOnlyList<AssignmentRuleDto>>.Success(dtos);
    }
}
