using AIChatServer.Entities.Connection;
using AIChatServer.Utils;

namespace AIChatServer.Entities.User.ServerUsers
{
    public abstract class ServerUser : IUnknownUser
    {
        private List<Connection.Connection> connections;
        protected event EventHandler<Command> GotCommand;
        public event EventHandler Disconnected;
        private readonly object syncObj = new object();

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
            User = user.User;
            connections = [];
            AddConnection(user);
        }

        protected ServerUser(Connection.Connection connection)
        {
            connections = [];
            AddConnection(connection);
        }

        public void AddConnection(Connection.Connection connection)
        {
            lock (syncObj)
            {
                connection.CommandGot += OnCommandGot;
                connection.Disconnected += Disconnect;
                DB.SetLastConnection(connection.Id, true);

                if (GetType() == typeof(KnownUser))
                {
                    if (connections.Count > 0)
                    {
                        Command onConnectCommand = new Command("ConnectionsChange");
                        ConnectionInfo connectionInfo = DB.GetConnectionInfo(connection.Id, User.Id);
                        onConnectCommand.AddData("connectionInfo", connectionInfo);
                        onConnectCommand.AddData("count", DB.GetConnectionCount(connectionInfo.UserId));
                        onConnectCommand.AddData("isOnline", true);

                        foreach (Connection.Connection connectionInList in connections)
                        {
                            SendCommand(connectionInList, onConnectCommand);
                        }
                    }
                }
                connections.Add(connection);
            }
        }

        public void AddConnection(ServerUser user)
        {
            lock (syncObj)
            {
                foreach (Connection.Connection connection in user.connections)
                    AddConnection(connection);
            }
        }

        public Connection.Connection? RemoveConnection(int id)
        {
            lock (syncObj)
            {
                for (int i = 0; i < connections.Count; i++)
                {
                    if (connections[i].Id == id)
                    {
                        Connection.Connection connection = connections[i];
                        connections.Remove(connection);
                       return connection;
                    }
                }
                return null;
            }
        }

        private void OnCommandGot(object sender, Command command)
        {
            GotCommand?.Invoke(sender, command);
        }

        private void Disconnect(object? sender, EventArgs args)
        {
            if (sender is not Connection.Connection connection) return;

            lock (syncObj)
            {
                DB.SetLastConnection(connection.Id, false);
                connection.CommandGot -= GotCommand.Invoke;
                connections.Remove(connection);

                if (connections.Count == 0)
                {
                    Disconnected?.Invoke(this, new EventArgs());
                }
                else
                {
                    Command onDisconnectCommand = new Command("ConnectionsChange");
                    ConnectionInfo connectionInfo = DB.GetConnectionInfo(connection.Id);
                    onDisconnectCommand.AddData("connectionInfo", connectionInfo);
                    onDisconnectCommand.AddData("count", DB.GetConnectionCount(connectionInfo.UserId));

                    foreach (Connection.Connection connectionInList in connections)
                    {
                        SendCommand(connectionInList, onDisconnectCommand);
                    }
                }
            }
        }

        public void SendCommand(Command command)
        {
            List<Connection.Connection> connectionsCopy;

            lock (syncObj)
            {
                connectionsCopy = new List<Connection.Connection>(connections);
            }

            foreach (var connection in connectionsCopy)
            {
                SendCommand(connection, command);
            }
        }

        public void SendCommand(int connectionId, Command command)
        {
            Connection.Connection targetConnection = null;

            lock (syncObj)
            {
                for (int i = 0; i < connections.Count; i++)
                {
                    if (connections[i].Id == connectionId)
                    {
                        targetConnection = connections[i];
                        break;
                    }
                }
            }

            if (targetConnection != null)
            {
                SendCommand(targetConnection, command);
            }
        }

        public static void SendCommand(Connection.Connection connection, Command command)
        {
            Console.WriteLine($"Sendinng command to client: {JsonHelper.Serialize(command)}");
            connection.SendCommand(JsonHelper.SerializeToBytes(command));
        }

        Connection.Connection IUnknownUser.GetCurrentConnection()
        {
            lock (syncObj)
            {
                return connections[0];
            }
        }
    }
}