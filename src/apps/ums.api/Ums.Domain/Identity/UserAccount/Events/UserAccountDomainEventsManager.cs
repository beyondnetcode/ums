namespace Ums.Domain.Identity.UserAccount;

public class UserAccountDomainEventsManager : DomainEventsManager
{
    public UserAccountDomainEventsManager(IAggregateRoot aggregateRoot) : base(aggregateRoot) { }

    private void Apply(UserRegisteredEvent @event) { }
    private void Apply(UserActivatedEvent @event) { }
    private void Apply(UserBlockedEvent @event) { }
    private void Apply(UserRestoredEvent @event) { }
    private void Apply(UserDeletedEvent @event) { }
    private void Apply(UserSignupDeniedEvent @event) { }
    private void Apply(MfaEnrolledEvent @event) { }
    private void Apply(MfaVerifiedEvent @event) { }
    private void Apply(AuthenticationAttemptedEvent @event) { }
}
