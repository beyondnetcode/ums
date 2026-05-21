namespace Ums.Application.Audit.AuditRecord.Commands;

using FluentValidation;

public sealed class RecordAuditCommandValidator : AbstractValidator<RecordAuditCommand>
{
    public RecordAuditCommandValidator()
    {
        RuleFor(command => command.WhoActed).NotEmpty();
        RuleFor(command => command.WhatChanged).NotEmpty().MaximumLength(2000);
        RuleFor(command => command.EventType).NotEmpty().MaximumLength(100);
        RuleFor(command => command.AffectedEntityId).NotEmpty();
        RuleFor(command => command.AffectedEntityType).NotEmpty().MaximumLength(100);
        RuleFor(command => command.RootTenantId).NotEmpty();
    }
}
