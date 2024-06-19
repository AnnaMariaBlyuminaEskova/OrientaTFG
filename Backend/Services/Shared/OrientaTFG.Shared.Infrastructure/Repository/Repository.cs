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
    public TEntity GetById(int id)
    {
        return dbSet.Find(id);
    }

    /// <summary>
    /// Gets all the entities
    /// </summary>
    /// <returns>IEnumerable<TEntity></returns>
    public IEnumerable<TEntity> GetAll()
    {
        return dbSet.ToList();
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
    public void Add(TEntity entity)
    {
        dbSet.Add(entity);
    }

    /// <summary>
    /// Updates an entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    public void Update(TEntity entity)
    {
        dbSet.Update(entity);
    }

    /// <summary>
    /// Deletes an entity by its id
    /// </summary>
    /// <param name="id">The entity's id</param>
    public void Delete(int id)
    {
        var entity = dbSet.Find(id);
        if (entity != null)
        {
            dbSet.Remove(entity);
        }
    }

    /// <summary>
    /// Saves the realized changes
    /// </summary>
    public void SaveChanges()
    {
        orientaTFGContext.SaveChanges();
    }
}
