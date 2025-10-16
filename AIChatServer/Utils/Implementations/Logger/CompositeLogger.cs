using Microsoft.Extensions.Logging;

namespace AIChatServer.Utils.Implementations.Logger
{
    public class CompositeLogger<T>(ILogger<T>[] loggers) : ILogger<T>
    {
        private readonly ILogger<T>[] _loggers = loggers
            ?? throw new ArgumentNullException(nameof(loggers));

        public IDisposable BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            foreach (var logger in _loggers)
                logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}
