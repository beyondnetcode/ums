namespace Ums.Application.IGA.RoleMaturityStatus.Commands;

public sealed class UpdatePerformanceScoreCommandValidator : AbstractValidator<UpdatePerformanceScoreCommand>
{
    public UpdatePerformanceScoreCommandValidator()
    {
        RuleFor(x => x.RoleMaturityStatusId).NotEmpty();
        RuleFor(x => x.Score).InclusiveBetween(0m, 5m)
            .WithMessage("Performance score must be between 0 and 5.");
    }
}
