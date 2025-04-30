using AIChatServer;

public class ConnectionManager
{
    private Dictionary<int, ServerUser> users;
    public readonly object syncObj = new object();
    public event EventHandler<bool> OnConnected;

    public ConnectionManager()
    {
        users = new Dictionary<int, ServerUser>();
    }

    public void ConnectUser(int userId, ServerUser serverUser)
    {
        lock (syncObj)
        {
            ConnectUserWithoutLock(userId, serverUser);
        }
    }

    public void DisconnectUser(int id)
    {
        lock (syncObj)
        {
            DisconnectUserWithoutLock(id);
        }
    }

    public bool ReconnectUser(int oldId, int newId, KnownUser knownUser)
    {
        lock (syncObj)
        {
            return ReconnectUserWithoutLock(oldId, newId, knownUser);
        }
    }
    public ServerUser? ReconnectUser(int oldId, int newId)
    {
        lock (syncObj)
        {
             return ReconnectUserWithoutLock(oldId, newId);
        }
    }

    public void DisconnectUserWithoutLock(int id)
    {
        users.Remove(id);
    }

    public bool ReconnectUserWithoutLock(int oldId, int newId, KnownUser knownUser)
    {
        users.Remove(oldId);
        return ConnectUserWithoutLock(newId, knownUser);
    }
    public ServerUser? ReconnectUserWithoutLock(int oldId, int newId)
    {
        if (users.Remove(oldId, out ServerUser serverUser))
        {
            if (users.ContainsKey(newId))
            {
                ConnectUserWithoutLock(newId, serverUser);
            }
            else Console.WriteLine($"User {{{newId}}} disconnected");
        }
        else
        {
            return null;
        }
        return serverUser;
    }

    public KnownUser? GetUserWithoutLock(int userId, Connection connection)
    {
        if (userId == 0)
            return null;

        if (userId < 0)
        {
            userId = -userId;
            var command = new Command("RefreshToken");
            command.AddData("token", TokenManager.GenerateToken(userId, connection.Id));
            ServerUser.SendCommand(connection, command);
        }

        if (users.TryGetValue(userId, out var existingUser))
        {
            var knownUser = (KnownUser)existingUser;
            var newUser = new KnownUser(existingUser.User, connection);
            ConnectUserWithoutLock(userId, newUser);
            return newUser;
        }
        else
        {
            var newUser = new KnownUser(DB.GetUserById(userId), connection);
            ConnectUserWithoutLock(userId, newUser);
            return newUser;
        }
    }

    public bool ConnectUserWithoutLock(int userId, ServerUser serverUser)
    {
        bool isNewUser = users.TryAdd(userId, serverUser);
        if (!isNewUser)
        {
            users[userId].AddConnection(serverUser);
        }
        OnConnected.Invoke(serverUser, isNewUser);
        return isNewUser;
    }

    public KnownUser[] GetConnectedUsers(int[] users)
    {
        lock (syncObj)
        {
            List<KnownUser> knownUsers = [];
            for (int i = 0; i < users.Length; i++)
            {
                if (this.users.TryGetValue(users[i], out ServerUser? serverUser))
                {
                    knownUsers.Add((KnownUser)serverUser);
                }
            }
            return knownUsers.ToArray();
        }
    }
}