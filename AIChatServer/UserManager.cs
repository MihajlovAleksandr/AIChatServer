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
        private Dictionary<int, ServerUser> users;
        private readonly TokenManager tokenManager;
        private readonly object syncObj = new object();
        private int id = 0;
        public UserManager()
        {
            users = new Dictionary<int, ServerUser>();
            tokenManager = new TokenManager();
            Task.Run(GetNewConnections);
        }
        private async void GetNewConnections()
        {
            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add("https://192.168.100.11:8888/");
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


        public void ConnectUser(KnownUser serverUser)
        {
            lock (syncObj)
            {
                int userId = serverUser.GetUserId();
                if (!users.TryAdd(userId, serverUser))
                {
                    users[userId].AddConnection(serverUser);
                }
            }
        }

        private void KnowUser(object? sender, User e)
        {
            WebSocket? webSocket = sender as WebSocket ?? throw new ArgumentException("webSocket was not webSocket, WHAT?!");
            Command command = new Command("CreateToken");
            command.AddData("token", tokenManager.GenerateToken(e.id));
            ServerUser.SendCommand(webSocket, command);
            KnownUser knownUser = new KnownUser(e, webSocket);
            lock (syncObj) {
                users.Remove(id);
                users.Add(e.id, knownUser);
            }
        }


        private async Task HandleClient(HttpListenerContext httpContext)
        {
            WebSocketContext webSocketContext = null;
            try
            {
                webSocketContext = await httpContext.AcceptWebSocketAsync(null);
                Console.WriteLine($"Клиент подключился через WebSocket.");
            }
            catch (Exception e)
            {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.Close();
                Console.WriteLine("Ошибка при установлении WebSocket-соединения: " + e.Message);
                return;
            }
            WebSocket webSocket = webSocketContext.WebSocket;
            string? token = webSocketContext.Headers["token"];
            Console.WriteLine($"token: {token}");
            KnownUser? knownUser = GetUserFromToken(token, webSocket);
            if (knownUser != null)
            {
                knownUser.CommandGot += (sender, command) =>
                {
                   Console.WriteLine(users.Count);
                    Console.WriteLine($"{sender}: {command}");
                    Message message = command.GetData<Message>("message");
                    Console.WriteLine(message.time);
                    command = new Command("SendMessage");
                    command.AddData("message", message);
                    knownUser.SendCommandForAllConnections(command);

                };
                users.Add(knownUser.GetUserId(), knownUser);
                Console.WriteLine("I know u...");
            }
            else
            {
                Console.WriteLine("I don't know u...");
                int id = GetUnknownUserId();
                users.Add(id, GetUnknownUser(webSocket, id));
            }
           

        }
        private int GetUnknownUserId()
        {
            id--;
            return id;
        }
        private UnknownUser GetUnknownUser(WebSocket socket, int id)
        {
            UnknownUser user = new UnknownUser(socket, id);
            user.UserChanged += KnowUser;
            return user;
        }

        private KnownUser? GetUserFromToken(string token, WebSocket webSocket)
        {
            int userId = GetUserIdFromToken(token);
            Console.WriteLine($"{userId}");
            if (userId != 0)
            {
                if(userId<0)
                {
                    userId = -1*userId;
                    Command command = new Command("RefreshToken");
                    command.AddData("token", tokenManager.GenerateToken(userId));
                    ServerUser.SendCommand(webSocket, command);
                }
                KnownUser knownUser;
                if (users.TryGetValue(userId, out var user))
                {
                    knownUser = (KnownUser)user;
                    knownUser.AddConnection(webSocket);
                }
                else
                {
                    knownUser = new KnownUser(DB.GetUserById(userId), webSocket);
                }
                return knownUser;
                
            }
            return null;
        }



        private int GetUserIdFromToken(string token)
        {
            if (tokenManager.ValidateToken(token, out int userId, out DateTime expirationTime))
            {
                if (expirationTime < DateTime.UtcNow.AddDays(7))
                {
                    return -1*userId;
                }
                return userId;
            }
            return 0;
        }
    }
}
