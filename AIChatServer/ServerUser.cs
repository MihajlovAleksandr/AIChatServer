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

        public User User { get; protected set; }
        public ServerUser()
        {
            connections = [];
        }
        ~ServerUser()
        {
            Console.WriteLine($"Destroy: {GetType()}");
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
        public void SendCommand(Command command)
        {
            foreach (var connection in connections)
            {
                SendCommand(connection, command);
            }
        }
        public void SendCommand(int connectionId, Command command)
        {
            for(int i = 0; i < connections.Count; i++)
            {
                if (connections[i].Id == connectionId)
                {
                    SendCommand(connections[i], command);
                }
            }
        }
        public static void SendCommand(Connection connection, Command command)
        {
            Console.WriteLine($"Sendinng command to client: {JsonHelper.Serialize(command)}");
            connection.SendCommand(JsonHelper.SerializeToBytes(command));
        }
    }
}