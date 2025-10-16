using AIChatServer.Utils;
using System.Net.WebSockets;
using System.Text;

namespace AIChatServer.Entities.Connection
{
    public class Connection
    {
        public int Id { get; set; }
        private readonly WebSocket webSocket;
        private event EventHandler<Command> CommandGotHandler;
        public event EventHandler<Command> CommandGot
        {
            add { CommandGotHandler = value; }
            remove { CommandGotHandler = value; }
        }
        private event EventHandler DisconnectedHandler;
        public event EventHandler Disconnected
        {
            add { DisconnectedHandler = value; }
            remove { DisconnectedHandler = value; }
        }
        public Connection(int id, WebSocket webSocket)
        {
            Console.WriteLine("Create Connection");
            Id = id;
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
            var buffer = new byte[1024 * 4];
            var receivedData = new List<byte>();

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                    receivedData.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));
                    if (result.EndOfMessage)
                    {
                        try
                        {
                            var message = Encoding.UTF8.GetString(receivedData.ToArray());
                            Console.WriteLine(message);

                            Command command = JsonHelper.Deserialize<Command>(receivedData.ToArray());
                            command.SetSender(this);
                            CommandGotHandler?.Invoke(this, command);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Ошибка при обработке команды: " + e.Message);
                        }
                        finally
                        {
                            receivedData.Clear();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка при обмене данными: " + e.Message);
            }
            finally
            {
                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Закрытие соединения", CancellationToken.None);
                }
                DisconnectedHandler?.Invoke(this, EventArgs.Empty);
                Console.WriteLine("Клиент отключился.");
            }
        }

        public async Task SendCommand(byte[] command)
        {
            try
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(command),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send Command: {ex.Message}");
            }
        }
    }
}