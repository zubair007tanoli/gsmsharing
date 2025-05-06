using discussionspot.Data;
using discussionspot.Interfaces;
using discussionspot.Repositories;

namespace discussionspot.Extensions
{
    /// <summary>
    /// Extension methods for repositories
    /// </summary>
    public static class RepositoryExtensions
    {
        /// <summary>
        /// Gets the underlying DbContext from the repository
        /// </summary>
        public static ApplicationDbContext GetDbContext<TEntity>(this EfRepository<TEntity> repository) where TEntity : class
        {
            // Get the protected _context field using reflection
            var contextField = typeof(EfRepository<TEntity>).GetField("_context",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (contextField == null)
                throw new InvalidOperationException("Cannot access the _context field of the repository");

            var context = contextField.GetValue(repository) as ApplicationDbContext;

            if (context == null)
                throw new InvalidOperationException("The _context field is not of type ApplicationDbContext");

            return context;
        }

        /// <summary>
        /// Creates an IQueryable for performing custom LINQ queries
        /// </summary>
        public static IQueryable<TEntity> AsQueryable<TEntity>(this IRepository<TEntity> repository) where TEntity : class
        {
            if (repository is EfRepository<TEntity> efRepository)
            {
                var dbContext = efRepository.GetDbContext();
                return dbContext.Set<TEntity>();
            }

            throw new InvalidOperationException("This extension method only works with EfRepository implementations");
        }

        /// <summary>
        /// Creates a connection asynchronously (for ADO.NET repositories)
        /// </summary>
        public static async Task<Microsoft.Data.SqlClient.SqlConnection> CreateConnectionAsync<TEntity>(
            this AdoRepository<TEntity> repository) where TEntity : class, new()
        {
            // Get the protected CreateConnection method using reflection
            var createConnectionMethod = typeof(AdoRepository<TEntity>).GetMethod("CreateConnection",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (createConnectionMethod == null)
                throw new InvalidOperationException("Cannot access the CreateConnection method of the repository");

            var connection = createConnectionMethod.Invoke(repository, null) as Microsoft.Data.SqlClient.SqlConnection;

            if (connection == null)
                throw new InvalidOperationException("CreateConnection did not return a SqlConnection");

            // Ensure connection is open
            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            return connection;
        }
    }
}
