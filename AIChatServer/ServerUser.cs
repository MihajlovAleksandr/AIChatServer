using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
{
    public abstract class ServerUser
    {
        private List<Connection> connections;
        protected event EventHandler<Command> GotCommand;
        public event EventHandler Disconnected;
        public ServerUser()
        {
            connections = [];
        }
        ~ServerUser()
        {
            Console.WriteLine($"\n\n\nDestroy: {GetType()}\n\n\n");
        }
        
        protected ServerUser(ServerUser user)
        {
            connections = [];
            AddConnection(user);
        }
        protected ServerUser(Connection connection)
        {
            connections = [];
            AddConnection(connection);
        }
        public void AddConnection(Connection connection)
        {
            connection.CommandGot += OnCommandGot;
            connection.Disconnected += Disconnect;
            connections.Add(connection);
        }
        public void AddConnection(ServerUser user)
        {
            foreach (Connection connection in connections)
                AddConnection(connection);
        }
        private void OnCommandGot(object sender, Command command)
        {
            GotCommand.Invoke(sender, command);
        }
        private void Disconnect(object? sender, EventArgs args)
        {
            if (sender is not Connection connection) return;
            connection.CommandGot -= GotCommand.Invoke;
            connections.Remove(connection);
            if (connections.Count == 0) Disconnected.Invoke(this, new EventArgs());
        }
        public void SendCommandForAllConnections(Command command)
        {
            Console.WriteLine($"Sendinng command to client: {JsonHelper.Serialize(command)}");
            byte[] bytes = JsonHelper.SerializeToBytes(command);
            foreach (var connection in connections)
            {
                connection.SendCommand(bytes);
            }
        }
        public static void SendCommand(Connection connection, Command command)
        {
            Console.WriteLine($"Sendinng command to client: {JsonHelper.Serialize(command)}");
            connection.SendCommand(JsonHelper.SerializeToBytes(command));
        }

    }
}