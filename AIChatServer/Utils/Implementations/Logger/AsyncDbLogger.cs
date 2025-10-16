using AIChatServer.Entities.Logger;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AIChatServer.Utils.Implementations.Logger
{
    public class AsyncDbLogger<T>(BlockingCollection<LogEntry> queue) : ILogger<T>
    {
        private readonly BlockingCollection<LogEntry> _queue = queue
            ?? throw new ArgumentNullException(nameof(queue));

        public IDisposable BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (formatter == null) return;

            var message = formatter(state, exception);
            _queue.Add(new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = logLevel.ToString(),
                Message = message,
                Source = typeof(T).FullName!
            });
        }
    }
}
