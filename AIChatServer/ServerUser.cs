using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
{
    public abstract class ServerUser
    {
        private List<WebSocket> webSockets;
        protected event Action<Command> CommandGot;
        public ServerUser()
        {
            webSockets = new List<WebSocket>();
        }
        public void AddConnection(WebSocket webSocket)
        {
            webSockets.Add(webSocket);
            Task.Run(() => { HandleClient(webSocket); });
        }
        public void AddConnection(ServerUser user)
        {
            webSockets.Add(user.webSockets[0]);
            Task.Run(() => { HandleClient(user.webSockets[0]); });
        }
        private async void HandleClient(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024];

            while (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрытие соединения", CancellationToken.None);
                    }
                    else
                    {
                        Command command = CommandConverter.GetCommand(buffer);
                        command.SetSender(webSocket);
                        CommandGot.Invoke(command);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка при обмене данными: " + e.Message);
                    break;
                }
            }
            webSockets.Remove(webSocket);
            Console.WriteLine("Клиент отключился.");
        }
    }
}
