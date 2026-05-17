namespace Ums.Domain.Identity.System.Action;

public class Action
{
    public ActionCode Code { get; }
    public Name Description { get; }

    private Action(ActionCode code, Name description)
    {
        Code = code;
        Description = description;
    }

    public static Result<Action> Create(ActionCode code, Name description)
    {
        if (string.IsNullOrWhiteSpace(code.GetValue()))
        {
            return Result<Action>.Failure(DomainErrors.ValueObject.PropertyRequired);
        }

        var action = new Action(code, description);
        return Result<Action>.Success(action);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Action other) return false;
        return Code.GetValue() == other.Code.GetValue();
    }

    public override int GetHashCode()
    {
        return Code.GetValue().GetHashCode();
    }
}
