namespace Ums.Domain.Identity.UserAccount.MfaEnrollment;

public class MfaEnrollmentProps : IProps
{
    public IdValueObject Id { get; set; }
    public UserAccountId UserAccountId { get; set; }
    public MfaMethod Method { get; set; }
    public MfaEnrollmentStatus Status { get; set; }
    public AuditValueObject Audit { get; private set; }

    public MfaEnrollmentProps(
        IdValueObject id,
        UserAccountId userAccountId,
        MfaMethod method,
        ActorId createdBy)
    {
        Id = id;
        UserAccountId = userAccountId;
        Method = method;
        Status = MfaEnrollmentStatus.NotEnrolled;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
