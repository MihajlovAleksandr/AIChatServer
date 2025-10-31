namespace AIChatServer.Repositories.Constants
{
    public static class ConnectionQueries
    {
        public const string AddConnection = @"
            INSERT INTO connections (device) 
            VALUES (@Device)
            RETURNING id;";

        public const string GetConnectionInfo = @"
            SELECT id, user_id, device, last_connection 
            FROM connections 
            WHERE id = @ConnectionId";

        public const string GetAllUserConnections = @"
            SELECT id, user_id, device, last_connection 
            FROM connections 
            WHERE user_id = @UserId";

        public const string VerifyConnection = @"
            SELECT last_connection, COUNT(*) as connection_count
            FROM connections 
            WHERE id = @Id AND user_id = @UserId AND device = @Device
            GROUP BY last_connection;";

        public const string RemoveConnection =
            "UPDATE connections SET user_id = NULL WHERE id = @Id";

        public const string SetLastConnectionOnline =
            "UPDATE connections SET last_connection = NULL WHERE id = @ConnectionId";

        public const string SetLastConnectionOffline =
            "UPDATE connections SET last_connection = CURRENT_TIMESTAMP WHERE id = @ConnectionId";

        public const string GetConnectionCount = @"
            SELECT 
                COUNT(*) AS devices_count, 
                CASE WHEN COUNT(*) = 0 THEN 0 ELSE SUM(CASE WHEN last_connection IS NULL THEN 1 ELSE 0 END) END AS online_devices_count
            FROM connections 
            WHERE user_id = @UserId";

        public const string DeleteUnknownConnection =
            "DELETE FROM connections WHERE id = @Id";

        public const string UpdateConnection =
            "UPDATE connections SET user_id = @UserId WHERE id = @Id";

        public const string GetLastUserOnline = @"
            SELECT 
                CASE 
                    WHEN COUNT(*) FILTER (WHERE last_connection IS NULL) > 0 THEN NULL
                    ELSE MAX(last_connection)
                END AS last_online
            FROM connections
            WHERE user_id = @UserId;";
    }
}