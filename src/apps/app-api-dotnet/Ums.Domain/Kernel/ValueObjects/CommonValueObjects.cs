namespace Ums.Domain.Kernel.ValueObjects;

using System.Text.RegularExpressions;
using Ums.Shell.Ddd.ValueObjects.Common;
using Ums.Shell.Ddd.Rules.Impl;
using Ums.Shell.Ddd.Rules;
using Ums.Shell.Ddd;

public class EmailAddressValidator : AbstractRuleValidator<ValueObject<string>>
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public EmailAddressValidator(ValueObject<string> subject) : base(subject) { }

    public override void AddRules(RuleContext? context)
    {
        var value = Subject.GetValue();
        if (string.IsNullOrWhiteSpace(value) || !EmailRegex.IsMatch(value))
        {
            AddBrokenRule("EmailAddress", "A valid email address is required.");
        }
    }
}

public class EmailAddress : StringValueObject
{
    private EmailAddress(string value) : base(value)
    {
    }

    public static Result<EmailAddress> Create(string value)
    {
        var email = new EmailAddress(value.Trim().ToLowerInvariant());
        if (!email.IsValid)
        {
            return Result<EmailAddress>.Failure(email.BrokenRules.GetBrokenRules().First().Message);
        }
        return Result<EmailAddress>.Success(email);
    }

    public static EmailAddress Default()
    {
        return new EmailAddress(string.Empty);
    }

    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new EmailAddressValidator(this));
    }
}

public class CodeValidator : AbstractRuleValidator<ValueObject<string>>
{
    public CodeValidator(ValueObject<string> subject) : base(subject) { }

    public override void AddRules(RuleContext? context)
    {
        var value = Subject.GetValue();
        if (string.IsNullOrWhiteSpace(value))
        {
            AddBrokenRule("Code", DomainErrors.CodeRequired);
        }
    }
}

public class Code : StringValueObject
{
    private Code(string value) : base(value)
    {
    }

    public static Code Create(string value)
    {
        return new Code(DomainGuards.NormalizeCode(value));
    }

    public static Code Default()
    {
        return new Code(string.Empty);
    }

    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new CodeValidator(this));
    }
}

public record DateRange
{
    public DateTimeOffset StartsAt { get; init; }
    public DateTimeOffset? EndsAt { get; init; }

    private DateRange(DateTimeOffset startsAt, DateTimeOffset? endsAt)
    {
        if (endsAt.HasValue && endsAt.Value < startsAt)
        {
            throw new ArgumentException("End date cannot be earlier than start date.");
        }
        StartsAt = startsAt;
        EndsAt = endsAt;
    }

    public static DateRange Create(DateTimeOffset startsAt, DateTimeOffset? endsAt = null)
    {
        return new DateRange(startsAt, endsAt);
    }

    public static DateRange Default()
    {
        return new DateRange(DateTimeOffset.MinValue, null);
    }

    public bool Contains(DateTimeOffset value) => value >= StartsAt && (!EndsAt.HasValue || value <= EndsAt.Value);
}

public class TextValueObject : StringValueObject
{
    private TextValueObject(string value) : base(value) { }

    public static TextValueObject Create(string value)
    {
        return new TextValueObject(value ?? string.Empty);
    }
}
