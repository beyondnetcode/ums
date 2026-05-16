namespace Ums.Domain.Kernel.ValueObjects;

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
