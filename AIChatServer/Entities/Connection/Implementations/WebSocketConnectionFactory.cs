using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Utils.Interfaces;
using AIChatServer.Utils.Interfaces.Mapper;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;

namespace AIChatServer.Entities.Connection.Implementations
{
    public class WebSocketConnectionFactory(ISerializer serializer, IRequestMapper<CommandRequestWithSender, Command> mapper, ILogger<WebSocketConnection> logger) : IConnectionFactory
    {
        private readonly ISerializer _serializer = serializer
                        ?? throw new ArgumentNullException(nameof(serializer));
        private readonly ILogger<WebSocketConnection> _logger = logger
                        ?? throw new ArgumentNullException(nameof(logger));
        private readonly IRequestMapper<CommandRequestWithSender, Command> _mapper = mapper
                        ?? throw new ArgumentNullException(nameof(mapper));

        public IConnection Create(Guid id, WebSocket webSocket)
        {
            return new WebSocketConnection(id, webSocket, _serializer, _mapper ,_logger);
        }
    }
}
