using Microsoft.Extensions.Logging;

namespace AIChatServer.Utils.Implementations.Logger
{
    public class ConsoleLogger<T>(ConsoleHelper consoleHelper) : ILogger<T>
    {
        private readonly ConsoleHelper _consoleHelper = consoleHelper
            ?? throw new ArgumentNullException(nameof(consoleHelper));

        public IDisposable BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (formatter == null) return;

            var message = formatter(state, exception);
            var (foreground, background) = GetColors(logLevel);

            var logText = $"{DateTime.UtcNow:u} [{logLevel}] {typeof(T).FullName}: {message}";
            if (exception != null)
                logText += $"\nException: {exception}";

            _consoleHelper.Write(logText, foreground, background);
        }

        private (ConsoleColor foreground, ConsoleColor background) GetColors(LogLevel logLevel) => logLevel switch
        {
            LogLevel.Trace => (ConsoleColor.DarkGray, ConsoleColor.Black),
            LogLevel.Debug => (ConsoleColor.Gray, ConsoleColor.Black),
            LogLevel.Information => (ConsoleColor.Cyan, ConsoleColor.Black),
            LogLevel.Warning => (ConsoleColor.Yellow, ConsoleColor.Black),
            LogLevel.Error => (ConsoleColor.Red, ConsoleColor.Black),
            LogLevel.Critical => (ConsoleColor.White, ConsoleColor.Red),
            _ => (ConsoleColor.White, ConsoleColor.Black)
        };
    }
}
