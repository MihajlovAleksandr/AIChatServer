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
        private readonly object syncObj = new object();
        public UserManager()
        {
            users = new Dictionary<int, ServerUser>();
            
            Task.Run(GetNewConnections);
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
        public async void GetNewConnections()
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



        private async Task HandleClient(HttpListenerContext httpContext)
        {
            WebSocketContext webSocketContext = null;
            try
            {
                webSocketContext = await httpContext.AcceptWebSocketAsync(null);
                Console.WriteLine($"Клиент подключился через WebSocket. {webSocketContext.Headers["userId"]}");
            }
            catch (Exception e)
            {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.Close();
                Console.WriteLine("Ошибка при установлении WebSocket-соединения: " + e.Message);
                return;
            }
            WebSocket webSocket = webSocketContext.WebSocket;

            KnownUser serverUser = new KnownUser(new User());
            serverUser.AddConnection(webSocket);
            serverUser.CommandGot += (sender, command) =>
            {
                Console.WriteLine(command);
            };

        }
    }
}
