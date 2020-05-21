using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LL.FirstCore.IRepository.Base
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        #region Query
        TEntity GetEntity(object keyValue);


        Task<TEntity> GetEntityAsync(object keyValue);
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
        #endregion

        #region Delete
        void Remove(object keyValue);

        void Remove(TEntity entity);

        Task<int> RemoveAsync(object keyValue);
        #endregion
    }
}
