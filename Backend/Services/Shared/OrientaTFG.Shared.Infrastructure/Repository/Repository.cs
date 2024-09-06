using Microsoft.EntityFrameworkCore;
using OrientaTFG.Shared.Infrastructure.DBContext;

namespace OrientaTFG.Shared.Infrastructure.Repository;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// The OrientaTFG DB Context
    /// </summary>
    private readonly OrientaTFGContext orientaTFGContext;

    /// <summary>
    /// The <TEntity> DB Set
    /// </summary>
    private readonly DbSet<TEntity> dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository<TEntity>"/> class
    /// </summary>
    /// <param name="orientaTFGContext">The OrientaTFG DB Context</param>
    public Repository(OrientaTFGContext orientaTFGContext)
    {
        this.orientaTFGContext = orientaTFGContext;
        this.dbSet = orientaTFGContext.Set<TEntity>();
    }

    /// <summary>
    /// Gets an entity by its id
    /// </summary>
    /// <param name="id">The entity's id</param>
    /// <returns>The entity</returns>
    public async Task<TEntity?> GetByIdAsync(int id)
    {
        return await dbSet.FindAsync(id);
    }

    /// <summary>
    /// Gets all the entities
    /// </summary>
    /// <returns>IEnumerable<TEntity></returns>
    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await dbSet.ToListAsync();
    }

    /// <summary>
    /// Gets the IQueryable to execute queries
    /// </summary>
    /// <returns>IQueryable<TEntity></returns>
    public IQueryable<TEntity> AsQueryable()
    {
        return dbSet.AsQueryable();
    }

    /// <summary>
    /// Adds an entity to de database
    /// </summary>
    /// <param name="entity">The entity to add</param>
    public async Task AddAsync(TEntity entity)
    {
        await dbSet.AddAsync(entity);
        await SaveChangesAsync();
    }

    /// <summary>
    /// Updates an entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    public async Task UpdateAsync(TEntity entity)
    {
        dbSet.Update(entity);
        await SaveChangesAsync();
    }

    /// <summary>
    /// Deletes an entity by its id
    /// </summary>
    /// <param name="id">The entity's id</param>
    public async Task DeleteAsync(int id)
    {
        var entity = dbSet.Find(id);
        if (entity != null)
        {
            dbSet.Remove(entity);
            await SaveChangesAsync();
        }
    }

    /// <summary>
    /// Saves the realized changes
    /// </summary>
    private async Task SaveChangesAsync()
    {
        await orientaTFGContext.SaveChangesAsync();
    }
}
