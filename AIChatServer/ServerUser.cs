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
        public event Action Disconnected;
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
                        break;
                    }
                    else
                    {
                        Command command = JsonHelper.Deserialize<Command>(buffer);
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
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрытие соединения", CancellationToken.None);
            webSockets.Remove(webSocket);
            if (webSockets.Count == 0)
            {
                Disconnected?.Invoke();
            }
            Console.WriteLine("Клиент отключился.");
        }
        protected void DeleteSocketFromList(WebSocket webSocket)
        {
            webSockets.Remove(webSocket);
        }
        public void SendCommandForAllConnections(Command command)
        {
            Console.WriteLine($"Sendinng command to client: {JsonHelper.Serialize(command)}");
            byte[] bytes = JsonHelper.SerializeToBytes(command);
            foreach (var webSocket in webSockets)
            {
                SendCommand(webSocket, bytes);
            }
        }

        public static void SendCommand(WebSocket webSocket, Command command)
        {
            Console.WriteLine($"Sendinng command to client: {JsonHelper.Serialize(command)}");
            SendCommand(webSocket, JsonHelper.SerializeToBytes(command));
        }

        private async static void SendCommand(WebSocket webSocket, byte[] command)
        {
            try
            {
                await webSocket.SendAsync(new ArraySegment<byte>(command), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send Command: {ex.Message}");
            }
        }
    }
}
