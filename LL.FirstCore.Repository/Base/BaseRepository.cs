using LL.FirstCore.IRepository.Base;
using LL.FirstCore.Repository.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
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
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private readonly string ConnStr;

        public BaseRepository(BaseDbContext context)
        {
            this._dbContext = context;
            if (_dbContext == null)
                return;

            Table = _dbContext.Set<TEntity>();
            ConnStr = _dbContext.Database.GetDbConnection().ConnectionString;
        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            if (_dbContext.Database.CurrentTransaction == null)
            {
                _dbContext.Database.BeginTransaction(isolationLevel);
            }
        }

        public void Commit()
        {
            var transaction = _dbContext.Database.CurrentTransaction;
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

        public void Rollback()
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

        public async Task<int> SaveChangesAsync()
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

        public IEnumerable<TEntity> GetBySql(string sql, params object[] parameters)
        {
            return Table.FromSqlRaw(sql, parameters).Cast<TEntity>().AsEnumerable();
        }

        public IQueryable<TEntity> Entities
        {
            get { return Table.AsNoTracking(); }
        }

        public IQueryable<TEntity> TrankingEntities
        {
            get { return Table; }
        }

        #region 显示编译查询
        public IEnumerable<TEntity> GetByCompileQuery(Expression<Func<TEntity, bool>> filter)
        {
            if (filter == null) filter = m => true;
            return EF.CompileQuery((BaseDbContext context) => context.Set<TEntity>().AsNoTracking().Where(filter).AsEnumerable())(_dbContext);
        }

        public async Task<IEnumerable<TEntity>> GetByCompileQueryAsync(Expression<Func<TEntity, bool>> filter)
        {
            if (filter == null) filter = m => true;
            return await EF.CompileAsyncQuery((BaseDbContext context) => context.Set<TEntity>().AsNoTracking().Where(filter).AsEnumerable())(_dbContext);
        }
        #endregion
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
                await SaveChangesAsync();
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

        /// <summary>
        /// 指定列的数据更新(局部更新)
        /// 知识点:在实体上使用Attach方法时，其状态将设置为Unchanged
        /// Detached：对象存在，但未由对象服务跟踪。在创建实体之后、但将其添加到对象上下文之前，该实体处于此状态；
        /// Unchanged：自对象加载到上下文中后，或自上次调用 SaveChanges() 方法后，此对象尚未经过修改；
        /// Added：上下文正在跟踪该实体，但是该实体尚不存在于数据库中；
        /// Deleted：实体正在由上下文跟踪，并存在于数据库中。它有被标记为要从数据库中删除。
        /// Modified：实体正在由上下文跟踪，并存在于数据库中。它的部分或全部属性值已被修改。
        /// <param name="model"></param>
        /// <param name="updateColumns"></param>
        /// <returns></returns>
        public virtual int Update(TEntity model, params string[] updateColumns)
        {
            if (model == null)
                return 0;

            if (_dbContext.Entry(model).State == EntityState.Added || _dbContext.Entry(model).State == EntityState.Detached)
                Table.Attach(model);
            var entry = _dbContext.Entry(model);
            if (updateColumns != null && updateColumns.Length > 0)
            {
                foreach (var propertyName in updateColumns)
                {
                    entry.Property(propertyName).IsModified = true;
                }
            }
            else
            {
                //全字段更新
                entry.State = EntityState.Modified;
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

        public async Task<int> RemoveListAsync(IEnumerable<TEntity> list)
        {
            foreach (var item in list)
            {
                Table.Attach(item);
                Table.Remove(item);
            }

            return await SaveChangesAsync();
        }

        public int DeleteBySql(string sql, params object[] parameters)
        {
            return Database.ExecuteSqlRaw(sql, CancellationToken.None, parameters);
        }
        #endregion

        #region Sql
        public int ExecuteSqlWithNonQuery(string sql, params object[] parameters)
        {
            return Database.ExecuteSqlRaw(sql, CancellationToken.None, parameters);
        }

        public async Task<int> ExecuteSqlWithNonQueryAsync(string sql, params object[] parameters)
        {
            return await Database.ExecuteSqlRawAsync(sql, CancellationToken.None, parameters);
        }
        #endregion

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
