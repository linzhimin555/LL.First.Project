using LL.FirstCore.IRepository.Base;
using LL.FirstCore.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LL.FirstCore.Services.Base
{
    public class BaseServices<TEntity> : IBaseServices<TEntity> where TEntity : class, new()
    {
        IBaseRepository<TEntity> _baseRepository;
        public BaseServices(IBaseRepository<TEntity> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        #region Insert
        public TEntity Insert(TEntity entity, bool isSave)
        {
            return _baseRepository.Insert(entity, isSave);
        }

        public async Task<TEntity> InsertAsync(TEntity entity, bool isSave)
        {
            return await _baseRepository.InsertAsync(entity, isSave);
        }

        public void AddRange(IEnumerable<TEntity> entitys, bool isSave)
        {
            _baseRepository.AddRange(entitys, isSave);
        }

        public void AddRangeAsync(IEnumerable<TEntity> entitys)
        {
            _baseRepository.AddRangeAsync(entitys);
        }
        #endregion

        #region Query
        public TEntity GetEntity(object keyValue)
        {
            return _baseRepository.GetEntity(keyValue);
        }

        public async Task<TEntity> GetEntityAsync(object keyValue)
        {
            return await _baseRepository.GetEntityAsync(keyValue);
        }

        public TEntity GetSingleOrDefault(Expression<Func<TEntity, bool>> where = null)
        {
            return _baseRepository.GetSingleOrDefault(where);
        }

        public async Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> where = null)
        {
            return await _baseRepository.GetSingleOrDefaultAsync(where);
        }

        public int Count(Expression<Func<TEntity, bool>> where = null)
        {
            return _baseRepository.Count();
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> where = null)
        {
            return await _baseRepository.CountAsync(where);
        }

        public bool Exist(Expression<Func<TEntity, bool>> where = null)
        {
            return _baseRepository.Exist(where);
        }

        public async Task<bool> ExistAsync(Expression<Func<TEntity, bool>> where = null)
        {
            return await _baseRepository.ExistAsync(where);
        }

        public IEnumerable<TEntity> GetByPagination(Expression<Func<TEntity, bool>> where, int pageSize, int pageIndex, bool asc = true, params Func<TEntity, object>[] orderby)
        {
            return _baseRepository.GetByPagination(where, pageSize, pageIndex, asc, orderby);
        }

        public List<TEntity> GetBySql(string sql, params object[] parameters)
        {
            return _baseRepository.GetBySql(sql, parameters);
        }
        #endregion

        #region Update
        public void Update(TEntity entity)
        {
            _baseRepository.Update(entity);
        }

        public void Update(IEnumerable<TEntity> entities)
        {
            _baseRepository.Update(entities);
        }

        public int Update(TEntity model, params string[] updateColumns)
        {
            return _baseRepository.Update(model, updateColumns);
        }
        #endregion

        #region Delete
        public void Remove(object keyValue)
        {
            _baseRepository.Remove(keyValue);
        }

        public void Remove(TEntity entity)
        {
            _baseRepository.Remove(entity);
        }

        public async Task<int> RemoveAsync(object keyValue)
        {
            return await _baseRepository.RemoveAsync(keyValue);
        }

        public int DeleteBySql(string sql, params object[] parameters)
        {
            return _baseRepository.DeleteBySql(sql, parameters);
        }

        public async Task<int> ExecuteSqlWithNonQueryAsync(string sql, params object[] parameters)
        {
            return await _baseRepository.ExecuteSqlWithNonQueryAsync(sql, parameters);
        }
        #endregion
    }
}
