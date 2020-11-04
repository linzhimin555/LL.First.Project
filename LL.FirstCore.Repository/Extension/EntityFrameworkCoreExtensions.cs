using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LL.FirstCore.Repository.Extension
{
    /// <summary>
    /// ECore 扩展
    /// </summary>
    public static class EntityFrameworkCoreExtensions
    {
        private static DbCommand CreateCommand(DatabaseFacade facade, string sql, out DbConnection connection, params object[] parameters)
        {
            var conn = facade.GetDbConnection();
            connection = conn;
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddRange(parameters);
            return cmd;
        }

        private static DataTable SqlQuery(this DatabaseFacade facade, string sql, params object[] parameters)
        {
            var command = CreateCommand(facade, sql, out DbConnection conn, parameters);
            var reader = command.ExecuteReader();
            var dt = new DataTable();
            dt.Load(reader);
            reader.Close();
            conn.Close();
            return dt;
        }

        public static List<T> SqlQuery<T>(this DatabaseFacade facade, string sql, params object[] parameters) where T : class, new()
        {
            var dt = SqlQuery(facade, sql, parameters);
            return dt.ToList<T>();
        }

        public static async Task<List<T>> SqlQueryAsync<T>(this DatabaseFacade facade, string sql, params object[] parameters) where T : class, new()
        {
            var dt = await Task.Run(() => SqlQuery(facade, sql, parameters));
            return dt.ToList<T>();
        }

        private static List<T> ToList<T>(this DataTable dt) where T : class, new()
        {
            var propertyInfos = typeof(T).GetProperties();
            var list = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                var t = new T();
                foreach (PropertyInfo p in propertyInfos)
                {
                    if (dt.Columns.IndexOf(p.Name) != -1 && row[p.Name] != DBNull.Value)
                        p.SetValue(t, row[p.Name], null);
                }
                list.Add(t);
            }
            return list;
        }

        public static async Task SaveChangesAndCancelTrackingAsync(this DbContext dbContext, bool isOutException)
        {
            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                foreach (var entry in dbContext.ChangeTracker.Entries().Where(w =>
                       w.State == EntityState.Added
                    || w.State == EntityState.Modified
                    || w.State == EntityState.Deleted))
                {
                    entry.State = EntityState.Detached;
                };
                if (isOutException) throw new Exception(ex.Message);
            }
        }
    }
}
