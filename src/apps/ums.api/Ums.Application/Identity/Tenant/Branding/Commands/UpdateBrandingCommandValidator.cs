namespace Ums.Application.Identity.Tenant.Branding.Commands;

using FluentValidation;

public sealed class UpdateBrandingCommandValidator : AbstractValidator<UpdateBrandingCommand>
{
    public UpdateBrandingCommandValidator()
    {
        RuleFor(command => command.TenantId)
            .NotEmpty();

        RuleFor(command => command.Logo)
            .NotEmpty()
            .MaximumLength(2048);

        RuleFor(command => command.LogoFormat)
            .NotEmpty();

        RuleFor(command => command.PrimaryColor)
            .NotEmpty()
            .MaximumLength(7);

        RuleFor(command => command.BackgroundStyle)
            .NotEmpty();

        RuleFor(command => command.HeadlineText)
            .MaximumLength(200);

        RuleFor(command => command.SecondaryText)
            .MaximumLength(500);

        RuleFor(command => command.PrimaryButtonLabel)
            .MaximumLength(100);

        RuleFor(command => command.FooterText)
            .MaximumLength(500);

        RuleFor(command => command.CustomDomain)
            .MaximumLength(253)
            .When(command => !string.IsNullOrWhiteSpace(command.CustomDomain));
    }
}
