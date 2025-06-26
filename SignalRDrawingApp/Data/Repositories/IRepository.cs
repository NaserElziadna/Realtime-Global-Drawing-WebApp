using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SignalRDrawingApp.Data.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        // Get
        Task<TEntity?> GetByIdAsync(object id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        
        // Add
        Task AddAsync(TEntity entity);
        Task AddRangeAsync(IEnumerable<TEntity> entities);
        
        // Remove
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
        
        // Update
        void Update(TEntity entity);
    }
} 