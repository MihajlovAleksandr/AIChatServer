using AIChatServer;

public class ConnectionManager
{
    private Dictionary<int, ServerUser> users;
    public readonly object syncObj = new object();

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

    public void ReconnectUser(int oldId, int newId, KnownUser knownUser)
    {
        lock (syncObj)
        {
            ReconnectUserWithoutLock(oldId, newId, knownUser);
        }
    }

    public KnownUser? GetUser(int userId, Connection connection)
    {
        lock (syncObj)
        {
            return GetUserWithoutLock(userId, connection);
        }
    }

    public void ConnectUserWithoutLock(int userId, ServerUser serverUser)
    {
        if (!users.TryAdd(userId, serverUser))
        {
            users[userId].AddConnection(serverUser);
        }
    }

    public void DisconnectUserWithoutLock(int id)
    {
        users.Remove(id);

    }

    public void ReconnectUserWithoutLock(int oldId, int newId, KnownUser knownUser)
    {
        users.Remove(oldId);
        ConnectUserWithoutLock(newId, knownUser);
    }

    public KnownUser? GetUserWithoutLock(int userId, Connection connection)
    {
        if (userId != 0)
        {
            if (userId < 0)
            {
                userId = -1 * userId;
                Command command = new Command("RefreshToken");
                command.AddData("token", TokenManager.GenerateToken(userId, connection.Id));
                ServerUser.SendCommand(connection, command);
            }

            if (users.TryGetValue(userId, out var user))
            {
                var knownUser = (KnownUser)user;
                knownUser.AddConnection(connection);
                return knownUser;
            }
            else
            {
                var knownUser = new KnownUser(DB.GetUserById(userId), connection);
                users[userId] = knownUser;
                return knownUser;
            }
        }
        return null;
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