using AIChatServer.Managers.Interfaces;
using System.Net;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations
{
    public class ConnectionListener(string serverUrl, IClientHandler clientHandler, 
        ILogger<ConnectionListener> logger) : IConnectionListener
    {
        private readonly string _serverUrl = serverUrl 
            ?? throw new ArgumentNullException(nameof(serverUrl));
        private readonly IClientHandler _clientHandler = clientHandler
            ?? throw new ArgumentNullException(nameof(clientHandler));
        private readonly ILogger<ConnectionListener> _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));

        public void Start()
        {
            Task.Run(async () =>
            {
                using HttpListener listener = new();
                listener.Prefixes.Add(_serverUrl);

                try
                {
                    listener.Start();
                    _logger.LogInformation("Server started on {ServerUrl}. Waiting for connections...", _serverUrl);
                }
                catch (HttpListenerException e)
                {
                    _logger.LogError(e, "Failed to start server on {ServerUrl}.", _serverUrl);
                    return;
                }

                while (true)
                {
                    HttpListenerContext context;
                    try
                    {
                        context = await listener.GetContextAsync();

                        if (context.Request.IsWebSocketRequest)
                        {
                            _logger.LogInformation("Incoming WebSocket request from {RemoteEndpoint}.", context.Request.RemoteEndPoint);
                            _ = _clientHandler.HandleClientAsync(context);
                        }
                        else
                        {
                            context.Response.StatusCode = 400;
                            context.Response.Close();
                            _logger.LogWarning("Received invalid HTTP request from {RemoteEndpoint}.", context.Request.RemoteEndPoint);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error while receiving HTTP context.");
                    }
                }
            });
        }
    }
}
