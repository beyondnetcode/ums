namespace Ums.Shell.Ddd.Interfaces
{
    /// <summary>
    /// Represents a generic repository interface.
    /// </summary>
    /// <typeparam name="T">The type of entity.</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Gets the unit of work associated with the repository.
        /// </summary>
        IUnitOfWork UnitOfWork { get; }
    }
}
