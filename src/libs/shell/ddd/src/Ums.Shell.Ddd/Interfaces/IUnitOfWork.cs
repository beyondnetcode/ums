namespace Ums.Shell.Ddd.Interfaces
{
    /// <summary>
    /// Represents a unit of work for managing database transactions.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Asynchronously saves all changes made in this unit of work to the underlying database.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously saves the specified entity to the underlying database.
        /// </summary>
        /// <param name="entity">The entity to be saved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains a boolean value indicating whether the save operation was successful.</returns>
        Task<bool> SaveEntitiesAsync(object entity, CancellationToken cancellationToken = default);
    }
}
