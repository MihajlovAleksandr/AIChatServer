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

    public void ReconnectUser(int oldId, int newId, KnownUser knownUser)
    {
        lock (syncObj)
        {
            ReconnectUserWithoutLock(oldId, newId, knownUser);
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
            ConnectUserWithoutLock(userId, knownUser);
            return knownUser;
        }
        else
        {
            var newUser = new KnownUser(DB.GetUserById(userId), connection);
            ConnectUserWithoutLock(userId, newUser);
            return newUser;
        }
    }

    public void ConnectUserWithoutLock(int userId, ServerUser serverUser)
    {
        bool isNewUser = users.TryAdd(userId, serverUser);
        if (!isNewUser)
        {
            users[userId].AddConnection(serverUser);
            OnConnected.Invoke(serverUser, false);
        }
        OnConnected.Invoke(serverUser, isNewUser);
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