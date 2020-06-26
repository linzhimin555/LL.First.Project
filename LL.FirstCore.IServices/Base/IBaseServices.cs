using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LL.FirstCore.IServices.Base
{
    public interface IBaseServices<TEntity> where TEntity : class
    {
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
        /// 伪分页查询
        /// </summary>
        /// <param name="where"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="asc"></param>
        /// <param name="orderby"></param>
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
