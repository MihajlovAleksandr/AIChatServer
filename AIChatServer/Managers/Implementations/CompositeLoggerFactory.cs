using AIChatServer.Managers.Interfaces;
using AIChatServer.Utils.Implementations;
using AIChatServer.Utils.Implementations.Logger;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations
{
    public class CompositeLoggerFactory(string connectionString) : ICompositeLoggerFactory, IDisposable
    {
        private readonly AsyncDbLoggerProvider _dbLoggerProvider = new AsyncDbLoggerProvider(connectionString);
        private readonly ConsoleHelper _consoleHelper = new ConsoleHelper();

        public CompositeLogger<T> Create<T>()
        {
            var dbLogger = (ILogger<T>)_dbLoggerProvider.CreateLogger(typeof(T).FullName!);
            var consoleLogger = new ConsoleLogger<T>(_consoleHelper);

            return new CompositeLogger<T>([dbLogger, consoleLogger]);
        }

        public void Dispose() => _dbLoggerProvider.Dispose();
    }
}
