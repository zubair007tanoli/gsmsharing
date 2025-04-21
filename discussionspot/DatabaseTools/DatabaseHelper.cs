using Microsoft.Data.SqlClient;
using System.Data;

namespace discussionspot.DatabaseTools
{
    public class DatabaseHelper
    {
        private readonly DatabaseConfig _config;
        private readonly ILogger<DatabaseHelper> _logger;
        private SqlConnection _connection;
        private bool _disposed;

        public DatabaseHelper(IConfiguration configuration, ILogger<DatabaseHelper> logger)
        {
            _config = new DatabaseConfig
            {
                ConnectionString = configuration.GetConnectionString("DiscussionspotConnection")
                    ?? throw new ArgumentNullException("Connection string 'DiscussionspotConnection' not found.")
            };
            _logger = logger;
            _connection = new SqlConnection(_config.ConnectionString);
        }

        public async Task EnsureConnectionOpenAsync()
        {
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.OpenAsync();
            }
        }

        public async Task<T?> ExecuteScalarAsync<T>(string sql, Dictionary<string, object>? parameters = null)
        {
            for (int attempt = 1; attempt <= _config.MaxRetryAttempts; attempt++)
            {
                try
                {
                    await EnsureConnectionOpenAsync();
                    using var command = CreateCommand(sql, parameters);
                    var result = await command.ExecuteScalarAsync();

                    if (result == null || result == DBNull.Value)
                    {
                        return default;
                    }

                    return (T)Convert.ChangeType(result, typeof(T));
                }
                catch (SqlException ex) when (_config.EnableRetry && attempt < _config.MaxRetryAttempts)
                {
                    _logger.LogWarning(ex, "Database operation failed, attempt {Attempt} of {MaxAttempts}",
                        attempt, _config.MaxRetryAttempts);
                    await Task.Delay(100 * attempt);
                }
            }
            throw new Exception("Maximum retry attempts exceeded");
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object>? parameters = null)
        {
            await EnsureConnectionOpenAsync();
            using var command = CreateCommand(sql, parameters);
            return await command.ExecuteNonQueryAsync();
        }

        public async Task<DataTable> ExecuteQueryAsync(string sql, Dictionary<string, object> parameters = null)
        {
            await EnsureConnectionOpenAsync();
            using var command = CreateCommand(sql, parameters);
            var dataTable = new DataTable();
            using var reader = await command.ExecuteReaderAsync();
            dataTable.Load(reader);
            return dataTable;
        }

        private SqlCommand CreateCommand(string sql, Dictionary<string, object> parameters)
        {
            var command = new SqlCommand(sql, _connection)
            {
                CommandTimeout = _config.CommandTimeout
            };

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            return command;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _connection?.Dispose();
                }
                _disposed = true;
            }
        }

        ~DatabaseHelper()
        {
            Dispose(false);
        }
    }
}
