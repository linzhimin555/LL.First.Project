﻿using LL.FirstCore.IRepository.Base;
using LL.FirstCore.Repository.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LL.FirstCore.Repository.Base
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        private BaseDbContext _dbContext;
        private DbSet<TEntity> Table { get; }
        private DatabaseFacade Database { get; }

        public BaseRepository(BaseDbContext context)
        {
            //_dbContext.Database.GetDbConnection().ConnectionString = string.Empty;
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

        public TEntity GetSingleOrDefault(Expression<Func<TEntity, bool>> @where = null)
        {
            return where == null ? Table.SingleOrDefault() : Table.SingleOrDefault(where);
        }
        public async Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> @where = null)
        {
            return await (where == null ? Table.SingleOrDefaultAsync() : Table.SingleOrDefaultAsync(where));
        }

        public int Count(Expression<Func<TEntity, bool>> @where = null)
        {
            return @where == null ? Table.Count(@where) : Table.Count(@where);
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> @where = null)
        {
            return await (where == null ? Table.CountAsync() : Table.CountAsync(@where));
        }

        public bool Exist(Expression<Func<TEntity, bool>> @where = null)
        {
            return @where == null ? Table.Any() : Table.Any(@where);
        }

        public async Task<bool> ExistAsync(Expression<Func<TEntity, bool>> @where = null)
        {
            return await (@where == null ? Table.AnyAsync() : Table.AnyAsync(@where));
        }

        public IEnumerable<TEntity> GetByPagination(Expression<Func<TEntity, bool>> @where, int pageSize, int pageIndex, bool asc = true, params Func<TEntity, object>[] @orderby)
        {
            var filter = Entities.Where(where);
            if (orderby != null)
            {
                foreach (var func in orderby)
                {
                    filter = asc ? filter.OrderBy(func).AsQueryable() : filter.OrderByDescending(func).AsQueryable();
                }
            }
            return filter.Skip(pageSize * (pageIndex - 1)).Take(pageSize);
        }

        public List<TEntity> GetBySql(string sql, params object[] parameters)
        {
            return Table.FromSqlRaw(sql, parameters).Cast<TEntity>().ToList();
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

        public virtual int Update(TEntity model, params string[] updateColumns)
        {
            if (updateColumns != null && updateColumns.Length > 0)
            {
                if (_dbContext.Entry(model).State == EntityState.Added || _dbContext.Entry(model).State == EntityState.Detached)
                    Table.Attach(model);
                foreach (var propertyName in updateColumns)
                {
                    _dbContext.Entry(model).Property(propertyName).IsModified = true;
                }
            }
            else
            {
                _dbContext.Entry(model).State = EntityState.Modified;
            }

            return SaveChanges();
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

        public int DeleteBySql(string sql, params object[] parameters)
        {
            return Database.ExecuteSqlRaw(sql, CancellationToken.None, parameters);
        }

        public async Task<int> ExecuteSqlWithNonQueryAsync(string sql, params object[] parameters)
        {
            return await Database.ExecuteSqlRawAsync(sql, CancellationToken.None, parameters);
        }
        #endregion
    }
}
