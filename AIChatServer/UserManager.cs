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
        public UserManager()
        {
            users = new Dictionary<int, ServerUser>();
            UnknownUser unknownUser = new UnknownUser();
            tokenManager = new TokenManager();
            unknownUser.UserChanged += KnowUser;
            users.Add(0, unknownUser);
            Task.Run(GetNewConnections);
        }
        private async void GetNewConnections()
        {
            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add("https://192.168.165.151:8888/");
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


        public void ConnectUser(UnknownUser user)
        {
            lock (syncObj)
            {
                users[0].AddConnection(user);
            }
        }


        public void DisconnectUser(int id)
        {
            lock (syncObj)
            {
                users.Remove(id);
            }
        }


        private void KnowUser(object? sender, User e)
        {
            WebSocket? webSocket = sender as WebSocket ?? throw new ArgumentException("webSocket was not webSocket, WHAT?!");
            lock (syncObj) {
                Command command = new Command("CreateToken");
                command.AddData("token", tokenManager.GenerateToken(e.id));
                ServerUser.SendCommand(webSocket, command);
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
                Console.WriteLine("I know u...");
                knownUser.CommandGot += (sender, command) =>
                {
                    Console.WriteLine($"{sender}: {command}");
                    Message message = command.GetData<Message>("message");
                    Console.WriteLine($"Got message: {message}");
                    command = new Command("SendMessage");
                    command.AddData("message", message);
                    knownUser.SendCommandForAllConnections(command);

                };
            }
            else
            {
                Console.WriteLine("I don't know u...");
                ServerUser serverUser = users[0];
                serverUser.AddConnection(webSocket);
            }
           

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
