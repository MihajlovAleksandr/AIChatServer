using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Utils.Interfaces;
using AIChatServer.Utils.Interfaces.Mapper;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;

namespace AIChatServer.Entities.Connection.Implementations
{
    public class WebSocketConnection : IConnection, IAsyncDisposable
    {
        private readonly WebSocket _webSocket;
        private readonly ISerializer _serializer;
        private readonly IRequestMapper<CommandRequestWithSender, Command> _mapper;
        private readonly ILogger<WebSocketConnection> _logger;
        private event EventHandler<Command> CommandGotHandler;
        private event EventHandler DisconnectedHandler;

        public Guid Id { get; set; }
        public event EventHandler Disconnected
        {
            add { DisconnectedHandler = value; }
            remove { DisconnectedHandler = value; }
        }
        public event EventHandler<Command> CommandGot
        {
            add { CommandGotHandler = value; }
            remove { CommandGotHandler = value; }
        }

        public WebSocketConnection(Guid id, WebSocket webSocket, ISerializer serializer, IRequestMapper<CommandRequestWithSender,Command> mapper,
            ILogger<WebSocketConnection> logger)
        {
            Id = id;
            _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

            _logger.LogInformation("WebSocket connection created with Id: {ConnectionId}", Id);

            Task.Run(async () => await HandleClientAsync(webSocket));
        }

        private async Task HandleClientAsync(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var receivedData = new List<byte>();

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("WebSocket connection {ConnectionId} received close message.", Id);
                        break;
                    }

                    receivedData.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));

                    if (result.EndOfMessage)
                    {
                        try
                        {
                            var command = ParseToCommand(receivedData);
                            CommandGotHandler?.Invoke(this, command);
                            _logger.LogDebug("Command {{{}}} received and processed successfully on connection {ConnectionId}.", command, Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing command on connection {ConnectionId}.", Id);
                        }
                        finally
                        {
                            receivedData.Clear();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during data exchange on connection {ConnectionId}.", Id);
            }
            finally
            {
                await DisposeAsync();
            }
        }

        public async Task SendCommandAsync(byte[] command)
        {
            try
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    await _webSocket.SendAsync(new ArraySegment<byte>(command), WebSocketMessageType.Text, true, CancellationToken.None);
                    _logger.LogInformation("Command sent successfully on connection {ConnectionId}.", Id);
                }
                else
                {
                    _logger.LogWarning("Attempted to send command on closed WebSocket connection {ConnectionId}.", Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send command on connection {ConnectionId}.", Id);
            }
        }

        private Command ParseToCommand(List<byte> receivedData)
        {
            CommandRequest receivedCommand = _serializer.Deserialize<CommandRequest>(receivedData.ToArray()) ?? throw new ArgumentException(nameof(receivedCommand));
            return _mapper.ToModel(new CommandRequestWithSender(receivedCommand, this));
        }

        public async ValueTask DisposeAsync()
        {
            if (_webSocket.State == WebSocketState.Open || _webSocket.State == WebSocketState.CloseReceived)
            {
                try
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
                    _logger.LogInformation("WebSocket connection {ConnectionId} closed gracefully.", Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error closing WebSocket connection {ConnectionId}.", Id);
                }
            }

            DisconnectedHandler?.Invoke(this, EventArgs.Empty);
            CommandGotHandler = null;
            DisconnectedHandler = null;

            _logger.LogInformation("Client disconnected from connection {ConnectionId}.", Id);
        }
    }
}
