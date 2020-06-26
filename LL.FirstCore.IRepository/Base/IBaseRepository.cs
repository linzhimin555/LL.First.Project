using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LL.FirstCore.IRepository.Base
{
    public interface IBaseRepository<TEntity> : IDisposable where TEntity : class
    {
        #region Transaction
        /// <summary>
        /// 显式开启数据上下文事务
        /// </summary>
        /// <param name="isolationLevel">指定连接的事务锁定行为</param>
        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);

        /// <summary>
        /// 提交事务的更改
        /// </summary>
        void Commit();

        /// <summary>
        /// 显式回滚事务，仅在显式开启事务后有用
        /// </summary>
        void Rollback();

        /// <summary>
        /// 提交当前单元操作的更改
        /// </summary>
        int SaveChanges();
        Task<int> SaveChangesAsync();

        /// <summary>
        /// 获取 当前实体类型的查询数据集，数据将使用不跟踪变化的方式来查询，当数据用于展现时，推荐使用此数据集，如果用于新增，更新，删除时，请使用<see cref="TrackEntities"/>数据集
        /// </summary>
        IQueryable<TEntity> Entities { get; }
        #endregion

        #region Query
        TEntity GetEntity(object keyValue);
        Task<TEntity> GetEntityAsync(object keyValue);
        TEntity GetSingleOrDefault(Expression<Func<TEntity, bool>> @where = null);
        Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> @where = null);
        int Count(Expression<Func<TEntity, bool>> @where = null);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> @where = null);
        bool Exist(Expression<Func<TEntity, bool>> @where = null);
        Task<bool> ExistAsync(Expression<Func<TEntity, bool>> @where = null);
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="where">过滤条件</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="pageIndex">查询页</param>
        /// <param name="asc">升序或者降序</param>
        /// <param name="orderby">排序委托</param>
        /// <returns></returns>
        IEnumerable<TEntity> GetByPagination(Expression<Func<TEntity, bool>> @where, int pageSize, int pageIndex, bool asc = true, params Func<TEntity, object>[] @orderby);

        IEnumerable<TEntity> GetBySql(string sql, params object[] parameters);
        #endregion

        #region Insert
        TEntity Insert(TEntity entity, bool isSave);

        Task<TEntity> InsertAsync(TEntity entity, bool isSave);

        void AddRange(IEnumerable<TEntity> entitys, bool isSave);

        void AddRangeAsync(IEnumerable<TEntity> entitys);
        #endregion

        #region Update
        void Update(TEntity entity);

        void Update(IEnumerable<TEntity> entities);
        int Update(TEntity model, params string[] updateColumns);
        #endregion

        #region Delete
        void Remove(object keyValue);

        void Remove(TEntity entity);

        Task<int> RemoveAsync(object keyValue);

        int DeleteBySql(string sql, params object[] parameters);
        #endregion

        #region Sql
        int ExecuteSqlWithNonQuery(string sql, params object[] parameters);
        Task<int> ExecuteSqlWithNonQueryAsync(string sql, params object[] parameters);
        #endregion
    }
}
