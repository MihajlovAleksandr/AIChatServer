using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
{
    public class Connection
    {
        public int Id { get; set; }
        private readonly WebSocket webSocket;
        private event EventHandler<Command> CommandGotHandler;
        public event EventHandler<Command> CommandGot
        {
            add { CommandGotHandler = value; }
            remove { CommandGotHandler = null; }
        }
        private event EventHandler DisconnectedHandler;
        public event EventHandler Disconnected
        {
            add { DisconnectedHandler = value; }
            remove { DisconnectedHandler = null; }
        }
        public Connection(int id, WebSocket webSocket)
        {
            Console.WriteLine("Create Connection");
            this.Id = id;
            this.webSocket = webSocket;
            Task.Run(() => { HandleClient(webSocket); });
        }
        public Connection(string device, WebSocket webSocket)
        {
            Console.WriteLine("Create Connection");
            this.webSocket = webSocket;
            Task.Run(() => { HandleClient(webSocket); });
        }
        private async void HandleClient(WebSocket webSocket)
        {
            byte[] buffer;
            while (webSocket.State == WebSocketState.Open)
            {
                buffer = new byte[1024];
                try
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine(Encoding.UTF8.GetString(buffer));
                        Command command = JsonHelper.Deserialize<Command>(buffer);
                        command.SetSender(this);
                        CommandGotHandler.Invoke(this, command);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Ошибка при обмене данными: " + e.Message);
                    break;
                }
            }
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрытие соединения", CancellationToken.None);
            DisconnectedHandler?.Invoke(this, new EventArgs());
            Console.WriteLine("Клиент отключился.");
        }

        public async void SendCommand(byte[] command)
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
