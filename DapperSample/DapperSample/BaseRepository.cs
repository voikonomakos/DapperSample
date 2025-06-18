using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DapperSample
{
    public abstract class BaseRepository
    {
        protected string ConnectionString;

        protected BaseRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public async Task<List<T>> GetAll<T>(string tableName)
        {
            string query = $"SELECT * FROM {tableName}";

            return await Get<T>(query);
        }

        protected async Task<List<T>> Get<T>(string query)
        {
            return await WithConnection(async conn =>
            {
                var data = await conn.QueryAsync<T>(query);

                return data.ToList();
            });
        }

        protected async Task<T> Find<T>(string query, int id)
        {
            return await WithConnection(async conn =>
            {
                return await conn.QuerySingleOrDefaultAsync<T>(query, new {id});
            });
        }

        protected async Task<T> GetScalar<T>(string query)
        {
            return await WithConnection(async conn =>
            {
                var result = await conn.QuerySingleAsync<T>(query);

                return result;
            });
        }

        protected async Task<List<T>> Get<T>(string query, object parameters)
        {
            return await WithConnection(async conn =>
            {
                var data = await conn.QueryAsync<T>(query, parameters);

                return data.ToList();
            });
        }

        protected async Task ExecuteAsync<T>(string cmdText, T data)
        {
            await WithConnection(async conn =>
            {
                await conn.ExecuteAsync(cmdText, data);
            });
        }

        protected async Task ExecuteAsync<T>(string cmdText, List<T> data)
        {
            await WithConnection(async conn =>
            {
                await conn.ExecuteAsync(cmdText, data);
            });
        }

        protected async Task ExecuteAsync(string cmdText)
        {
            await WithConnection(async conn =>
            {
                await conn.ExecuteAsync(cmdText);
            });
        }

        protected async Task DeleteAll(string tableName)
        {
            await ExecuteAsync($"DELETE FROM {tableName}");
        }

        protected async Task<T> WithConnection<T>(Func<SqlConnection, Task<T>> getData)
        {
            try
            {
                await using var connection = new SqlConnection(ConnectionString);
                await connection.OpenAsync();
                var result = await getData(connection);
                await connection.CloseAsync();
                return result;
            }
            catch (TimeoutException ex)
            {
                throw new Exception(string.Format("{0}.WithConnection() experienced a SQL timeout", GetType().FullName), ex);
            }
            catch (SqlException ex)
            {
                throw new Exception(string.Format("{0}.WithConnection() experienced a SQL exception {1}", GetType().FullName, ex.Message), ex);
            }
        }

        protected async Task WithConnection(Func<SqlConnection, Task> action)
        {
            try
            {
                await using var connection = new SqlConnection(ConnectionString);
                await connection.OpenAsync();
                await action(connection);
                await connection.CloseAsync();
            }
            catch (TimeoutException ex)
            {
                throw new Exception(string.Format("{0}.WithConnection() experienced a SQL timeout", GetType().FullName), ex);
            }
            catch (SqlException ex)
            {
                throw new Exception(string.Format("{0}.WithConnection() experienced a SQL exception {1}", GetType().FullName, ex.Message), ex);
            }
        }
    }
}
