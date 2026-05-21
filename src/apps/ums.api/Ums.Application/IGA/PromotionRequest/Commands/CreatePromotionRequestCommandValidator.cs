namespace Ums.Application.IGA.PromotionRequest.Commands;

using FluentValidation;

public sealed class CreatePromotionRequestCommandValidator : AbstractValidator<CreatePromotionRequestCommand>
{
    public CreatePromotionRequestCommandValidator()
    {
        RuleFor(c => c.TenantId).NotEmpty();
        RuleFor(c => c.UserId).NotEmpty();
        RuleFor(c => c.CurrentRoleId).NotEmpty();
        RuleFor(c => c.TargetRoleId).NotEmpty();
        RuleFor(c => c.ManagerId).NotEmpty();
    }
}
