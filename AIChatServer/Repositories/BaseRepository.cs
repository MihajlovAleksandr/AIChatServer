using Npgsql;

namespace AIChatServer.Repositories
{
    public abstract class BaseRepository(string connectionString)
    {
        private readonly string _connectionString = connectionString 
            ?? throw new ArgumentNullException(nameof(connectionString));

        protected NpgsqlConnection GetConnection()
        {
            var connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        protected async Task<NpgsqlConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
    }
}
