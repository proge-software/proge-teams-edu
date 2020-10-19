using Microsoft.EntityFrameworkCore;
using Proge.Teams.Edu.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.DAL.Repositories
{
    public interface IRepository
    {
        TEntity Create<TEntity>(TEntity entity) where TEntity : class;
        Task<TEntity> CreateAsync<TEntity>(TEntity entity) where TEntity : class;
        void Delete<TEntity>(object id) where TEntity : class;
        void Delete<TEntity>(TEntity entity) where TEntity : class;
        void DeleteAll<TEntity>(IEnumerable<TEntity> list) where TEntity : class;
        void SoftDelete<TEntity>(object id) where TEntity : BaseEntity;
        IEnumerable<TEntity> Get<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null, int? skip = null, int? take = null) where TEntity : class;
        IEnumerable<TEntity> GetAll<TEntity>(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null, int? skip = null, int? take = null) where TEntity : class;
        Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null, int? skip = null, int? take = null) where TEntity : class;
        Task<IEnumerable<TEntity>> GetAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null, int? skip = null, int? take = null) where TEntity : class;
        TEntity GetById<TEntity, TKey>(TKey id) where TEntity : class;
        Task<TEntity> GetByIdAsync<TEntity, TKey>(TKey id) where TEntity : class;
        int GetCount<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class;
        Task<int> GetCountAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class;
        bool GetExists<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class;
        Task<bool> GetExistsAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class;
        TEntity GetFirst<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null) where TEntity : class;
        Task<TEntity> GetFirstAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null) where TEntity : class;
        TEntity GetOne<TEntity>(Expression<Func<TEntity, bool>> filter = null, string includeProperties = null) where TEntity : class;
        Task<TEntity> GetOneAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, string includeProperties = null) where TEntity : class;
        bool Save();
        Task<bool> SaveAsync(CancellationToken cancellationToken = default);
        void Update<TEntity>(TEntity entity) where TEntity : class;
        void Detach<TEntity>(TEntity entity) where TEntity : class;

        IQueryable<T> _defaultCollection<T>() where T : BaseEntity;
        IQueryable<T> _defaultCollectionWithDeleted<T>() where T : BaseEntity;
        IQueryable<T> _defaultReadOnlyCollection<T>() where T : BaseEntity;
    }

    public class Repository<TContext> : IRepository where TContext : DbContext
    {
        protected readonly TContext _dbContext;

        public Repository(TContext context)
        {
            _dbContext = context;
            //_dbContext.Database.Log = (s => System.Diagnostics.Debug.Write(s));
        }

        protected virtual IQueryable<TEntity> GetQueryable<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null, int? skip = null, int? take = null) where TEntity : class
        {
            includeProperties = includeProperties ?? string.Empty;
            IQueryable<TEntity> query = _dbContext.Set<TEntity>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return query;
        }

        public virtual IEnumerable<TEntity> GetAll<TEntity>(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null, int? skip = null, int? take = null) where TEntity : class
        {
            return GetQueryable(null, orderBy, includeProperties, skip, take).ToList();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync<TEntity>(Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null, int? skip = null, int? take = null) where TEntity : class
        {
            return await GetQueryable(null, orderBy, includeProperties, skip, take).ToListAsync();
        }

        public virtual IEnumerable<TEntity> Get<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null, int? skip = null, int? take = null) where TEntity : class
        {
            return GetQueryable(filter, orderBy, includeProperties, skip, take).ToList();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null, int? skip = null, int? take = null) where TEntity : class
        {
            return await GetQueryable(filter, orderBy, includeProperties, skip, take).ToListAsync();
        }

        public virtual TEntity GetById<TEntity, TKey>(TKey id) where TEntity : class
        {
            return _dbContext.Set<TEntity>().Find(id);
        }

        public virtual async Task<TEntity> GetByIdAsync<TEntity, TKey>(TKey id) where TEntity : class
        {
            var entity = await _dbContext.Set<TEntity>().FindAsync(id);
            _dbContext.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        public virtual int GetCount<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            return GetQueryable(filter).Count();
        }

        public virtual async Task<int> GetCountAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            return await GetQueryable(filter).CountAsync();
        }

        public virtual bool GetExists<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            return GetQueryable<TEntity>(filter).Any();
        }

        public virtual async Task<bool> GetExistsAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null) where TEntity : class
        {
            return await GetQueryable(filter).AnyAsync();
        }

        public virtual TEntity GetFirst<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null) where TEntity : class
        {
            return GetQueryable(filter, orderBy, includeProperties).FirstOrDefault();
        }

        public virtual async Task<TEntity> GetFirstAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = null) where TEntity : class
        {
            return await GetQueryable(filter, orderBy, includeProperties).FirstOrDefaultAsync();
        }

        public virtual TEntity GetOne<TEntity>(Expression<Func<TEntity, bool>> filter = null, string includeProperties = null) where TEntity : class
        {
            return GetQueryable(filter, null, includeProperties).SingleOrDefault();
        }

        public virtual async Task<TEntity> GetOneAsync<TEntity>(Expression<Func<TEntity, bool>> filter = null, string includeProperties = null) where TEntity : class
        {
            return await GetQueryable(filter, null, includeProperties).SingleOrDefaultAsync();
        }

        public virtual TEntity Create<TEntity>(TEntity entity) where TEntity : class
        {
            return _dbContext.Set<TEntity>().Add(entity).Entity;
        }

        public virtual async Task<TEntity> CreateAsync<TEntity>(TEntity entity) where TEntity : class
        {
            return (await _dbContext.Set<TEntity>().AddAsync(entity)).Entity;
        }

        public virtual void Update<TEntity>(TEntity entity) where TEntity : class
        {
            _dbContext.Set<TEntity>().Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Detach<TEntity>(TEntity entity) where TEntity : class
        {
            _dbContext.Set<TEntity>().Attach(entity);
            _dbContext.Entry(entity).State = EntityState.Detached;
        }

        public virtual void Delete<TEntity>(object id) where TEntity : class
        {
            TEntity entity = _dbContext.Set<TEntity>().Find(id);
            Delete(entity);
        }


        public virtual void Delete<TEntity>(TEntity entity) where TEntity : class
        {
            var dbSet = _dbContext.Set<TEntity>();
            if (_dbContext.Entry(entity).State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }
            dbSet.Remove(entity);
        }

        public virtual void DeleteAll<TEntity>(IEnumerable<TEntity> list) where TEntity : class
        {
            _dbContext.Set<TEntity>().RemoveRange(list);
        }

        public virtual void SoftDelete<TEntity>(object id) where TEntity : BaseEntity
        {
            var entity = _dbContext.Set<TEntity>().Find(id);
            var dbSet = _dbContext.Set<TEntity>();

            if (_dbContext.Entry(entity).State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }

            entity.DeletedOn = DateTime.UtcNow;
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public IQueryable<T> _defaultCollection<T>() where T : BaseEntity => _dbContext.Set<T>().NotDeleted().AsQueryable();
        public IQueryable<T> _defaultCollectionWithDeleted<T>() where T : BaseEntity => _dbContext.Set<T>().AsQueryable();
        public IQueryable<T> _defaultReadOnlyCollection<T>() where T : BaseEntity => _defaultCollection<T>().AsNoTracking();


        public virtual bool Save()
        {
            return _dbContext.SaveChanges() > 0;
        }

        public virtual async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
