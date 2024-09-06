namespace OrientaTFG.Shared.Infrastructure.Repository
{
    public interface IRepository<TEntity> 
    {
        /// <summary>
        /// Gets an entity by its id
        /// </summary>
        /// <param name="id">The entity's id</param>
        /// <returns>The entity</returns>
        Task<TEntity?> GetByIdAsync(int id);

        /// <summary>
        /// Gets all the entities
        /// </summary>
        /// <returns>IEnumerable<TEntity></returns>
        Task<IEnumerable<TEntity>> GetAllAsync();

        /// <summary>
        /// Gets the IQueryable to execute queries
        /// </summary>
        /// <returns>IQueryable<TEntity></returns>
        IQueryable<TEntity> AsQueryable();

        /// <summary>
        /// Adds an entity to de database
        /// </summary>
        /// <param name="entity">The entity to add</param>
        Task AddAsync(TEntity entity);

        /// <summary>
        /// Updates an entity
        /// </summary>
        /// <param name="entity">The entity to update</param>
        Task UpdateAsync(TEntity entity);

        /// <summary>
        /// Deletes an entity by its id
        /// </summary>
        /// <param name="id">The entity's id</param>
        Task DeleteAsync(int id);
    }
}
