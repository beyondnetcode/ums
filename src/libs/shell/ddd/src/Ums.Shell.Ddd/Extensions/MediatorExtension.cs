namespace Ums.Shell.Ddd.Extensions
{
    /// <summary>
    /// Extension methods for the Mediator class.
    /// </summary>
    public static class MediatorExtension
    {
        private const string KeyNameDomainEvents = "DomainEvents";
        private const string KeyNameClearDomainEvents = "ClearDomainEvents";

        /// <summary>
        /// Dispatches the domain events of the specified entity using the mediator.
        /// </summary>
        /// <param name="mediator">The mediator instance.</param>
        /// <param name="entity">The entity object.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task DispatchDomainEventsAsync(this IMediator mediator, object entity)
        {
            ArgumentNullException.ThrowIfNull(mediator, nameof(mediator));
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));

            var type = entity.GetType();

            if (type.GetProperty(KeyNameDomainEvents)?.GetValue(entity) is IEnumerable domainEvents)
            {
                var publishTasks = new List<Task>();
                foreach (var domainEvent in domainEvents)
                {
                    publishTasks.Add(mediator.Publish(domainEvent));
                }
                await Task.WhenAll(publishTasks).ConfigureAwait(false);

                type.GetMethod(KeyNameClearDomainEvents)?.Invoke(entity, null);
            }
        }
    }
}
