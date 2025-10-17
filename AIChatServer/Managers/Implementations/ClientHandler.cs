using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.Connection;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils;
using System.Net.WebSockets;
using System.Net;
using AIChatServer.Utils.Interfaces;
using AIChatServer.Entities.DTO.Response;
using Microsoft.Extensions.Logging;
using AIChatServer.Entities.Exceptions;
using AIChatServer.Entities.User;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Managers.Implementations
{
    public class ClientHandler : IClientHandler, IUserEvents
    {
        private readonly IUserManager _userManager;
        private readonly IConnectionService _connectionService;
        private readonly ISerializer _serializer;
        private readonly ITokenManager _tokenService;
        private readonly IConnectionManager _connectionManager;
        private readonly ISyncManager _syncService;
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<ClientHandler> _logger;
        private readonly IResponseMapper<UserBanResponse, UserBan> _userBanMapper;

        public event EventHandler<Command> CommandGot;
        public event EventHandler<bool> ConnectionChanged;

        public ClientHandler(
            IUserManager userManager,
            IConnectionService connectionService,
            ISerializer serializer,
            ITokenManager tokenService,
            IConnectionManager connectionManager,
            ISyncManager syncService,
            IConnectionFactory connectionFactory,
            IResponseMapper<UserBanResponse, UserBan> userBanMapper,
            ILogger<ClientHandler> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _userManager.OnConnectionEvent += (s, e) => ConnectionChanged?.Invoke(s,e);
            _userManager.CommandGot += (s, e) => CommandGot?.Invoke(s, e);
            _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _userBanMapper = userBanMapper ?? throw new ArgumentNullException(nameof(userBanMapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleClientAsync(HttpListenerContext ctx)
        {
            WebSocketContext wsCtx;

            try
            {
                wsCtx = await ctx.AcceptWebSocketAsync(null);
                _logger.LogInformation("Client connected via WebSocket from {RemoteEndpoint}.", ctx.Request.RemoteEndPoint);
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 500;
                ctx.Response.Close();
                _logger.LogError(ex, "Error establishing WebSocket connection.");
                return;
            }


            try
            {
                string? token = wsCtx.Headers["token"];
                string device = wsCtx.Headers["device"] ?? throw new ArgumentException("Device header is missing");

                var (userId, connId, needToRefreshToken) = _tokenService.ParseToken(token);

                IConnection connection;

                if (userId != default && _connectionService.VerifyConnection(connId, userId, device, out DateTime lastConn))
                {
                    connection = _connectionFactory.Create(connId, wsCtx.WebSocket);

                    IServerUser? serverUser = await _connectionManager.СreateUserAsync(userId, needToRefreshToken, connection);
                    if (serverUser != null)
                    {
                        _logger.LogInformation("Authorized user {UserId} connected with connection {ConnectionId}.", userId, connId);

                        serverUser.Disconnected += (s, e) => ConnectionChanged?.Invoke(s, false);
                        serverUser.GotCommand += (s, e) => CommandGot?.Invoke(s, e);

                        CommandResponse loginCmd = new("LoginIn", new LoginInResponse(serverUser.User.Id));
                        await CommandSender.SendCommandAsync(connection, loginCmd, _serializer);
                        await CommandSender.SendCommandAsync(connection, _syncService.GetSyncCommand(userId, lastConn), _serializer);

                        _logger.LogInformation("LoginIn, SyncDB send to connection {ConnectionId}.", connId);

                    }
                }
                else
                {
                    connection = _connectionFactory.Create(_connectionService.AddConnection(device), wsCtx.WebSocket);

                    _logger.LogInformation("Unknown user connected. Assigned temporary connection {ConnectionId}.", connection.Id);
                    await _userManager.CreateUnknownUser(connection);
                }
            }
            catch (UserBannedException userBannedEx)
            {
                if (wsCtx?.WebSocket is { State: WebSocketState.Open })
                {
                    var tempConnId = _connectionService.AddConnection("banned_user");
                    await using IConnection connection = _connectionFactory.Create(tempConnId, wsCtx.WebSocket);

                    try
                    {
                        var banResponse = new CommandResponse(
                            "UserBanned",
                            _userBanMapper.ToDTO(userBannedEx.UserBan)
                        );

                        await CommandSender.SendCommandAsync(connection, banResponse, _serializer);
                        _logger.LogInformation("Ban notification sent to banned user (ConnId: {ConnectionId}).", connection.Id);
                    }
                    catch (Exception sendEx)
                    {
                        _logger.LogError(sendEx, "Failed to send ban notification to banned user.");
                    }
                    finally
                    {
                        try
                        {
                            await connection.DisposeAsync();
                            _logger.LogInformation("WebSocket connection disposed for banned user (ConnId: {ConnectionId}).", connection.Id);
                        }
                        catch (Exception disposeEx)
                        {
                            _logger.LogError(disposeEx, "Error disposing WebSocket connection for banned user.");
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                _logger.LogError("Failed to create user due to {ex}", ex);
            }
        }
    }
}

