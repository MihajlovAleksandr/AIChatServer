using AIChatServer.Entities.Connection;
using AIChatServer.Entities.User;
using AIChatServer.Entities.User.ServerUsers;
using AIChatServer.Utils;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.WebSockets;

namespace AIChatServer.Managers
{
    public class UserManager
    {
        private int id = 0;
        private readonly ConnectionManager connectionManager;
        public event EventHandler<Command> CommandGot;
        public event EventHandler<bool> OnConnectionEvent;
        public event Func<int, bool> IsChatSearching;
        private readonly string serverURL;


        public UserManager()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var aiSettings = configuration.GetSection("ServerSettings");
            serverURL = aiSettings["URL"] ?? throw new ArgumentNullException("URL");
            connectionManager = new ConnectionManager();
            connectionManager.OnConnected += OnUserConnected;
            Task.Run(GetNewConnections);
        }
        private void OnUserConnected(object? sender, bool isNewUser)
        {
            if (isNewUser)
            {
                OnConnectionEvent.Invoke(sender, true);
            }
        }
        private async void GetNewConnections()
        {
            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add(serverURL);
            try
            {
                httpListener.Start();
                Console.WriteLine("Сервер запущен. Ожидание подключений...");
            }
            catch (HttpListenerException e)
            {
                Console.WriteLine("Ошибка при запуске сервера: " + e.Message);
                return;
            }

            while (true)
            {
                HttpListenerContext httpContext = null;
                try
                {
                    httpContext = await httpListener.GetContextAsync();

                    if (httpContext.Request.IsWebSocketRequest)
                    {
                        _ = HandleClient(httpContext);
                    }
                    else
                    {
                        httpContext.Response.StatusCode = 400;
                        httpContext.Response.Close();
                        Console.WriteLine("Получен некорректный запрос.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка при получении контекста: " + e.Message);
                }
            }
        }

        private void KnowUser(UnknownUser unknownUser)
        {
            KnowUserWithoutReconnect(unknownUser);
            KnownUser knownUser = new KnownUser(unknownUser);
            if (connectionManager.ReconnectUser(unknownUser.Id, unknownUser.User.Id, knownUser))
            {
                knownUser.CommandGot += KnowUserGotCommand;
                knownUser.Disconnected += DissconnectUser;
            }
        }
        public bool KnowUser(int oldId, User user)
        {
            ServerUser? serverUser = connectionManager.ReconnectUser(oldId, user.Id);
            if (serverUser is not UnknownUser unknownUser) return false;
            unknownUser.SetUser(user);
            KnowUserWithoutReconnect(unknownUser);
            return true;

        }
        private static void KnowUserWithoutReconnect(UnknownUser unknownUser)
        {
            IUnknownUser user = unknownUser;
            Connection connection = user.GetCurrentConnection();
            Command command = new Command("CreateToken");
            command.AddData("token", TokenManager.GenerateToken(unknownUser.User.Id, connection.Id));
            Console.WriteLine(connection.Id);
            DB.UpdateConnection(connection.Id, unknownUser.User.Id);
            ServerUser.SendCommand(connection, command);
            command = new Command("LoginIn");
            command.AddData("userId", unknownUser.User.Id);
            ServerUser.SendCommand(connection, command);
        }
        private async Task HandleClient(HttpListenerContext httpContext)
        {
            WebSocketContext webSocketContext = null;
            try
            {
                webSocketContext = await httpContext.AcceptWebSocketAsync(null);
                Console.WriteLine($"Клиент подключился через WebSocket.");

                string clientIp = httpContext.Request.RemoteEndPoint?.Address.ToString();
                Console.WriteLine(clientIp);
            }
            catch (Exception e)
            {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.Close();
                Console.WriteLine("Ошибка при установлении WebSocket-соединения: " + e.Message);
                return;
            }
            string? token = webSocketContext.Headers["token"];
            string device = webSocketContext.Headers["device"] ?? throw new ArgumentException("Device is not sent");
            var id = GetIdFromToken(token);
            Connection connection;

            if (id.Item1 != 0 && DB.VerifyConnection(id.Item2, id.Item1, device, out DateTime lastConnection))
            {
                connection = new Connection(id.Item2, webSocketContext.WebSocket);
                lock (connectionManager.syncObj)
                {
                    KnownUser? knownUser = connectionManager.GetUserWithoutLock(id.Item1, connection);
                    if (knownUser != null)
                    {
                        knownUser.CommandGot += KnowUserGotCommand;
                        knownUser.Disconnected += DissconnectUser;
                        Command command = new Command("LoginIn");
                        command.AddData("userId", knownUser.User.Id);
                        ServerUser.SendCommand(connection, command);
                        ServerUser.SendCommand(connection, GetSyncDBCommand(id.Item1, lastConnection));
                        Console.WriteLine("I know u...");
                    }
                }
            }
            else
            {
                Console.WriteLine("I don't know u...");
                connection = new Connection(DB.AddConnection(device), webSocketContext.WebSocket);
                int userId = GetUnknownUserId();
                connectionManager.ConnectUser(userId, GetUnknownUser(connection, userId));
            }
        }
        private UnknownUser GetUnknownUser(Connection connection, int id)
        {
            UnknownUser user = new UnknownUser(connection, id);
            user.UserChanged += KnowUser;
            user.Disconnected += DissconnectUser;
            user.SendCommand(new Command("Logout"));
            return user;
        }
        public void CreateUnknownUser(Connection connection)
        {
            int userId = GetUnknownUserId();
            UnknownUser user = new UnknownUser(connection, userId);
            user.UserChanged += KnowUser;
            user.Disconnected += DissconnectUser;
            connectionManager.ConnectUser(userId, user);
        }
        private int GetUnknownUserId()
        {
            id--;
            return id;
        }

        private static (int, int) GetIdFromToken(string token)
        {
            if (TokenManager.ValidateToken(token, out int userId, out int connectionId, out DateTime expirationTime))
            {
                if (expirationTime < DateTime.UtcNow.AddDays(7))
                {
                    return (-1*userId, connectionId);
                }
                return (userId, connectionId);
            }
            return (0,0);
        }

        private void KnowUserGotCommand(object sender, Command command)
        {
            CommandGot.Invoke(sender, command);
        }
        private void DissconnectUser(object sender, EventArgs eventArgs)
        {
            ServerUser su = (ServerUser)sender;
            connectionManager.DisconnectUser(su.User.Id);
            OnConnectionEvent.Invoke(sender, false);
        }
        public void SendCommand(int[] users, Command command)
        {
            KnownUser[] connectedUsers = connectionManager.GetConnectedUsers(users);
            foreach(KnownUser user in connectedUsers)
            {
                user.SendCommand(command);
            }
        }
        public Command GetSyncDBCommand(int userId, DateTime lastOnline)
        {
            var chats = DB.GetNewChats(userId, lastOnline);
            var messages = DB.GetNewMessages(userId, lastOnline);
            Command syncDBCommand = new Command("SyncDB");
            syncDBCommand.AddData("newMessages", messages.Item1.GroupBy(m => m.Chat)
            .SelectMany(g => g.OrderBy(m => m.Time))
            .ToList());
            syncDBCommand.AddData("oldMessages", messages.Item2);
            syncDBCommand.AddData("newChats", chats.Item1);
            syncDBCommand.AddData("oldChats", chats.Item2);
            syncDBCommand.AddData("isChatSearching", IsChatSearching.Invoke(userId));
            return syncDBCommand;
        }
    }
}
