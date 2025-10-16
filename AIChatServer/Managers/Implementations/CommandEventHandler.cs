using AIChatServer.Entities.Connection;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AIChatServer.Managers.Implementations
{
    public class CommandEventHandler : IEventHandler
    {
        private readonly ConcurrentDictionary<string, ICommandHandler> _handlers;
        private readonly ILogger<CommandEventHandler> _logger;

        public CommandEventHandler(ConcurrentDictionary<string, ICommandHandler> handlers,
                                   ILogger<CommandEventHandler> logger)
        {
            _handlers = handlers;
            _logger = logger;
        }

        public async Task HandleAsync(object? sender, object? args)
        {
            if (sender is not IServerUser user || args is not Command command)
                throw new ArgumentException("Invalid command event arguments");

            if (_handlers.TryGetValue(command.Operation, out var handler))
                await handler.HandleAsync(user, command);
            else
                _logger.LogError("No handler found for command {Command}", command.Operation);
        }

        public void Subscribe() { }
    }
}
