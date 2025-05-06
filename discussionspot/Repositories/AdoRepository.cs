using discussionspot.Interfaces;
using Microsoft.Data.SqlClient;
using System.Linq.Expressions;

namespace discussionspot.Repositories
{
    /// <summary>
    /// Base repository implementation using ADO.NET for direct database access
    /// This is a partial implementation - specialized repositories will need to extend this
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    public abstract class AdoRepository<TEntity> : IRepository<TEntity> where TEntity : class, new()
    {
        protected readonly string _connectionString;
        protected readonly string _tableName;
        protected readonly string _primaryKeyName;

        protected AdoRepository(IConfiguration configuration, string tableName, string primaryKeyName)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            _tableName = tableName;
            _primaryKeyName = primaryKeyName;
        }

        protected SqlConnection CreateConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        #region Unimplemented methods that require entity-specific implementation

        public virtual TEntity? GetById(object id)
        {
            using (var connection = CreateConnection())
            using (var command = new SqlCommand($"SELECT * FROM {_tableName} WHERE {_primaryKeyName} = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return MapEntityFromReader(reader);
                    }
                }
            }
            return null;
        }

        public virtual async Task<TEntity?> GetByIdAsync(object id)
        {
            using (var connection = CreateConnection())
            using (var command = new SqlCommand($"SELECT * FROM {_tableName} WHERE {_primaryKeyName} = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapEntityFromReader(reader);
                    }
                }
            }
            return null;
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            var entities = new List<TEntity>();

            using (var connection = CreateConnection())
            using (var command = new SqlCommand($"SELECT * FROM {_tableName}", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    entities.Add(MapEntityFromReader(reader));
                }
            }

            return entities;
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            var entities = new List<TEntity>();

            using (var connection = CreateConnection())
            using (var command = new SqlCommand($"SELECT * FROM {_tableName}", connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    entities.Add(MapEntityFromReader(reader));
                }
            }

            return entities;
        }

        public virtual int Count(Expression<Func<TEntity, bool>>? predicate = null)
        {
            using (var connection = CreateConnection())
            using (var command = new SqlCommand($"SELECT COUNT(*) FROM {_tableName}", connection))
            {
                return (int)command.ExecuteScalar();
            }
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null)
        {
            using (var connection = CreateConnection())
            using (var command = new SqlCommand($"SELECT COUNT(*) FROM {_tableName}", connection))
            {
                return (int)(await command.ExecuteScalarAsync());
            }
        }

        public virtual void Add(TEntity entity)
        {
            var (sql, parameters) = GenerateInsertCommand(entity);

            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }

                command.ExecuteNonQuery();
            }
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            var (sql, parameters) = GenerateInsertCommand(entity);

            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }

                await command.ExecuteNonQueryAsync();
            }
        }

        public virtual void Update(TEntity entity)
        {
            var (sql, parameters) = GenerateUpdateCommand(entity);

            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }

                command.ExecuteNonQuery();
            }
        }

        public virtual void Remove(TEntity entity)
        {
            var idProperty = typeof(TEntity).GetProperty(_primaryKeyName);
            if (idProperty == null)
                throw new InvalidOperationException($"Property {_primaryKeyName} not found on type {typeof(TEntity).Name}");

            var id = idProperty.GetValue(entity);
            if (id == null)
                throw new InvalidOperationException($"Primary key value is null for entity of type {typeof(TEntity).Name}");

            using (var connection = CreateConnection())
            using (var command = new SqlCommand($"DELETE FROM {_tableName} WHERE {_primaryKeyName} = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                command.ExecuteNonQuery();
            }
        }

        #endregion

        #region Helper methods for specific repositories to implement

        /// <summary>
        /// Maps a data reader to an entity
        /// </summary>
        protected abstract TEntity MapEntityFromReader(SqlDataReader reader);

        /// <summary>
        /// Generates an INSERT command for the entity
        /// </summary>
        protected abstract (string sql, List<SqlParameter> parameters) GenerateInsertCommand(TEntity entity);

        /// <summary>
        /// Generates an UPDATE command for the entity
        /// </summary>
        protected abstract (string sql, List<SqlParameter> parameters) GenerateUpdateCommand(TEntity entity);

        #endregion

        #region Not implemented methods - use EF repository for these

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException("Use EF repository for LINQ operations");
        }

        public Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException("Use EF repository for LINQ operations");
        }

        public TEntity? FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException("Use EF repository for LINQ operations");
        }

        public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException("Use EF repository for LINQ operations");
        }

        public bool Any(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException("Use EF repository for LINQ operations");
        }

        public Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException("Use EF repository for LINQ operations");
        }

        public void AddRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                Add(entity);
            }
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                await AddAsync(entity);
            }
        }

        public void UpdateRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                Update(entity);
            }
        }

        public void RemoveRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                Remove(entity);
            }
        }

        public Task<IEnumerable<TEntity>> GetPagedAsync(int skip, int take, Expression<Func<TEntity, bool>>? predicate = null,
            Expression<Func<TEntity, object>>? orderBy = null, bool ascending = true)
        {
            throw new NotImplementedException("Use EF repository for LINQ operations");
        }

        #endregion
    }
}
