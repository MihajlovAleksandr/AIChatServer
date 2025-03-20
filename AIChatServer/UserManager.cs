using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Math.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
{
    public class UserManager
    {
        private int id = 0;
        ConnectionManager connectionManager;
        public UserManager()
        {
            connectionManager = new ConnectionManager();
            Task.Run(GetNewConnections);
        }
        private async void GetNewConnections()
        {
            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add("https://192.168.100.7:8888/");
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

        private void KnowUser(object? sender, User e)
        {
            Connection? connection = sender as Connection ?? throw new ArgumentException("connection was not Connection, WHAT?!");
            Command command = new Command("CreateToken");
            command.AddData("token", TokenManager.GenerateToken(e.Id, connection.Id));
            Console.WriteLine(connection.Id);
            DB.UpdateConnection(connection.Id, e.Id);
            ServerUser.SendCommand(connection, command);
            command = new Command("LoginIn");
            command.AddData("userId", e.Id);
            ServerUser.SendCommand(connection, command);
            KnownUser knownUser = new KnownUser(e, connection);
            connectionManager.ReconnectUser(id, e.Id, knownUser);
            knownUser.CommandGot += KnowUserGotCommand;
            knownUser.Disconnected += DissconnectUser;
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

            if (id.Item1 != 0 && DB.VerifyConnection(id.Item2, id.Item1, device))
            {
                connection = new Connection(id.Item2, webSocketContext.WebSocket);
                lock (connectionManager.syncObj)
                {
                    KnownUser knownUser = connectionManager.GetUserWithoutLock(id.Item1, connection);
                    if (knownUser != null)
                    {
                        knownUser.CommandGot += KnowUserGotCommand;
                        knownUser.Disconnected += DissconnectUser;
                        connectionManager.ConnectUserWithoutLock(knownUser.User.Id, knownUser);
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
            user.SendCommandForAllConnections(new Command("LogOut"));
            return user;
        }

        private int GetUnknownUserId()
        {
            id--;
            return id;
        }

        private (int, int) GetIdFromToken(string token)
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
            Console.WriteLine($"{sender}: {command}");
            Message message = command.GetData<Message>("message");
            Console.WriteLine($"Got message: {message}");
            command = new Command("SendMessage");
            command.AddData("message", message);
            KnownUser knownUser = (KnownUser)sender;
            knownUser.SendCommandForAllConnections(command);
        }
        private void DissconnectUser(object sender, EventArgs eventArgs)
        {
            ServerUser su = (ServerUser)sender;
            connectionManager.DisconnectUser(su.User.Id);
        }
    }
}
