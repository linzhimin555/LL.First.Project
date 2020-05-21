using LL.FirstCore.IRepository.Base;
using LL.FirstCore.Repository.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.FirstCore.Repository.Base
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        private BaseDbContext _dbContext;
        private DbSet<TEntity> Table { get; }
        private DatabaseFacade DataBase { get; }

        public BaseRepository(BaseDbContext context)
        {
            this._dbContext = context;
            if (_dbContext == null)
                return;

            Table = _dbContext.Set<TEntity>();
        }

        public void BeginTransaction()
        {
            if (_dbContext.Database.CurrentTransaction == null)
            {
                _dbContext.Database.BeginTransaction();
            }
        }

        public void Commit()
        {
            var transaction = this._dbContext.Database.CurrentTransaction;
            if (transaction != null)
            {
                try
                {
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void Rollbasck()
        {
            if (_dbContext.Database.CurrentTransaction != null)
            {
                _dbContext.Database.CurrentTransaction.Rollback();
            }
        }

        public int SaveChanges()
        {
            return _dbContext.SaveChanges();
        }

        public async Task<int> SaveChangeAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }

        #region Query
        public TEntity GetEntity(object keyValue)
        {
            return this.Table.Find(keyValue);
        }

        public async Task<TEntity> GetEntityAsync(object keyValue)
        {
            return await this.Table.FindAsync(keyValue);
        }

        public IQueryable<TEntity> Entities
        {
            get { return Table.AsNoTracking(); }
        }

        public IQueryable<TEntity> TrankingEntities
        {
            get { return Table; }
        }
        #endregion

        #region Insert
        public TEntity Insert(TEntity entity, bool isSave = false)
        {
            Table.Add(entity);
            if (isSave)
            {
                SaveChanges();
            }
            return entity;
        }

        public async Task<TEntity> InsertAsync(TEntity entity, bool isSave = false)
        {
            await Table.AddAsync(entity);
            if (isSave)
            {
                await SaveChangeAsync();
            }

            return entity;
        }

        public void AddRange(IEnumerable<TEntity> entitys, bool isSave = true)
        {
            Table.AddRange(entitys);
            if (isSave)
            {
                SaveChanges();
            }
        }

        public async void AddRangeAsync(IEnumerable<TEntity> entitys)
        {
            await Table.AddRangeAsync(entitys);
        }
        #endregion

        #region Update
        public void Update(TEntity entity)
        {
            Table.Update(entity);
        }

        public void Update(IEnumerable<TEntity> entities)
        {
            Table.UpdateRange(entities);
        }
        #endregion

        #region Delete
        public void Remove(object keyValue)
        {
            Remove(GetEntity(keyValue));
        }

        public void Remove(TEntity entity)
        {
            Table.Remove(entity);
        }

        public async Task<int> RemoveAsync(object keyValue)
        {
            var result = 0;
            var entity = await Table.FindAsync(keyValue);
            if (entity != null)
            {
                Table.Remove(entity);
                result = SaveChanges();
            }

            return result;
        }
        #endregion
    }
}
