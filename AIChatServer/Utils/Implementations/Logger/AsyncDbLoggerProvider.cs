using AIChatServer.Entities.Logger;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Collections.Concurrent;

namespace AIChatServer.Utils.Implementations.Logger
{
    public class AsyncDbLoggerProvider : ILoggerProvider
    {
        private readonly string _connectionString;
        private readonly BlockingCollection<LogEntry> _queue = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _worker;

        public AsyncDbLoggerProvider(string connectionString)
        {
            _connectionString = connectionString
                ?? throw new ArgumentNullException(nameof(connectionString));
            ClearOldLogs();

            _worker = Task.Run(ProcessQueueAsync);
        }

        public ILogger CreateLogger(string categoryName)
        {
            var type = Type.GetType(categoryName) ?? typeof(object);
            var loggerType = typeof(AsyncDbLogger<>).MakeGenericType(type);
            return (ILogger)Activator.CreateInstance(loggerType, _queue)!;
        }

        private void ClearOldLogs()
        {
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();

                using var cmd = new NpgsqlCommand("DELETE FROM logs;", conn);
                cmd.ExecuteNonQuery();

                Console.WriteLine("[Logger] Старые логи удалены из таблицы logs.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Logger] Ошибка при очистке логов: {ex.Message}");
            }
        }

        private async Task ProcessQueueAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var entry = _queue.Take(_cts.Token);

                    await using var conn = new NpgsqlConnection(_connectionString);
                    await conn.OpenAsync(_cts.Token);

                    await using var cmd = new NpgsqlCommand(@"
                        INSERT INTO logs (timestamp, level, message, source) 
                        VALUES (@timestamp, @level, @message, @source)", conn);

                    cmd.Parameters.AddWithValue("timestamp", entry.Timestamp);
                    cmd.Parameters.AddWithValue("level", entry.Level);
                    cmd.Parameters.AddWithValue("message", entry.Message);
                    cmd.Parameters.AddWithValue("source", entry.Source);

                    await cmd.ExecuteNonQueryAsync(_cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Logger] Ошибка записи лога в БД: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _worker.Wait();
            _cts.Dispose();
            _queue.Dispose();
        }
    }
}
