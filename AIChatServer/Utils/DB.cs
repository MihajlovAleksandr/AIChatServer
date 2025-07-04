﻿using System.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using AIChatServer.Entities.AI;
using AIChatServer.Entities.Chats;
using AIChatServer.Entities.Connection;
using AIChatServer.Entities.User;
using MySql.Data.MySqlClient;

namespace AIChatServer.Utils
{
    public static class DB
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["aichat"].ConnectionString;

        public static MySqlConnection GetConnection()
        {
            var connection = new MySqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public static User GetUserById(int id)
        {
            string getUsersQuery = @"
                SELECT 
                    u.Id, 
                    u.Email, 
                    u.Password,
                    u.PremiumUntil AS Premium,
                    u.Points,
                    ud.Id AS UserDataId,
                    ud.Name,
                    ud.Gender,
                    ud.Age,
                    p.Id AS PreferenceId,
                    p.MinAge, 
                    p.MaxAge, 
                    p.PreferredGender AS PrefGender
                FROM 
                    Users u
                LEFT JOIN 
                    UserData ud ON u.Id = ud.UserId
                LEFT JOIN 
                    Preferences p ON u.Id = p.UserId
                WHERE 
                    u.Id = @userId";

            using (var connection = GetConnection())
            {
                using (var getUsersCommand = new MySqlCommand(getUsersQuery, connection))
                {
                    getUsersCommand.Parameters.AddWithValue("@userId", id);

                    using (var reader = getUsersCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var user = new User
                            {
                                Id = reader.GetInt32("Id"),
                                Email = reader.GetString("Email"),
                                Password = reader.GetString("Password"),
                                Premium = reader.IsDBNull("Premium") ? null : reader.GetDateTime("Premium"),
                                Points = reader.GetInt32("Points"),
                                UserData = new UserData
                                {
                                    Id = reader.GetInt32("UserDataId"),
                                    Name = reader.GetString("Name"),
                                    Gender = reader.GetChar("Gender"),
                                    Age = reader.GetInt32("Age")
                                },
                                Preference = new Preference
                                {
                                    Id = reader.GetInt32("PreferenceId"),
                                    MinAge = reader.GetInt32("MinAge"),
                                    MaxAge = reader.GetInt32("MaxAge"),
                                    Gender = reader.GetString("PrefGender")
                                }
                            };
                            return user;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        private static User GetUserByEmail(string email)
        {
            string getUsersQuery = @"
                SELECT 
                    u.Id, 
                    u.Email, 
                    u.Password,
                    u.PremiumUntil AS Premium,
                    u.Points,
                    ud.Id AS UserDataId,
                    ud.Name,
                    ud.Gender,
                    ud.Age,
                    p.Id AS PreferenceId,
                    p.MinAge, 
                    p.MaxAge, 
                    p.PreferredGender AS PrefGender
                FROM 
                    Users u
                LEFT JOIN 
                    UserData ud ON u.Id = ud.UserId
                LEFT JOIN 
                    Preferences p ON u.Id = p.UserId
                WHERE
                    u.Email = @email";

            using (var connection = GetConnection())
            {
                using (var getUsersCommand = new MySqlCommand(getUsersQuery, connection))
                {
                    getUsersCommand.Parameters.AddWithValue("@email", email);

                    using (var reader = getUsersCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var user = new User
                            {
                                Id = reader.GetInt32("Id"),
                                Email = reader.GetString("Email"),
                                Password = reader.GetString("Password"),
                                Premium = reader.IsDBNull("Premium") ? null : reader.GetDateTime("Premium"),
                                Points = reader.GetInt32("Points"),
                                UserData = new UserData
                                {
                                    Id = reader.GetInt32("UserDataId"),
                                    Name = reader.GetString("Name"),
                                    Gender = reader.GetChar("Gender"),
                                    Age = reader.GetInt32("Age")
                                },
                                Preference = new Preference
                                {
                                    Id = reader.GetInt32("PreferenceId"),
                                    MinAge = reader.GetInt32("MinAge"),
                                    MaxAge = reader.GetInt32("MaxAge"),
                                    Gender = reader.GetString("PrefGender")
                                }
                            };
                            return user;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        public static User LoginIn(string email, string password)
        {
            User user = GetUserByEmail(email);
            if (user != null)
            {
                if (VerifyPassword(password, user.Password))
                {
                    return user;
                }
            }
            return null;
        }

        public static bool IsEmailFree(string email)
        {
            string query = @"
                SELECT COUNT(*)
                FROM Users
                WHERE Email = @Email";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count == 0;
                }
            }
        }

        public static int? AddUser(User user, int connectionId)
        {
            using (var connection = GetConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int userId = InsertUser(user, connection, transaction);
                        InsertUserData(user.UserData, userId, connection, transaction);
                        int preferenceId = InsertPreference(user.Preference, userId, connection, transaction);
                        UpdateConnection(connectionId, userId, connection, transaction);
                        InsertNotifications(userId, connection, transaction);
                        transaction.Commit();
                        return userId;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Error: {ex.Message}");
                        return null;
                    }
                }
            }
        }

        private static int InsertUser(User user, MySqlConnection connection, MySqlTransaction transaction)
        {
            string insertUserQuery = @"
        INSERT INTO Users (Email, Password, PremiumUntil, Points) 
        VALUES (@Email, @Password, @PremiumUntil, @Points);
        SELECT LAST_INSERT_ID();";

            using (var command = new MySqlCommand(insertUserQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@Password", HashPassword(user.Password));
                command.Parameters.AddWithValue("@PremiumUntil", user.Premium);
                command.Parameters.AddWithValue("@Points", user.Points);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static int InsertUserData(UserData userData, int userId, MySqlConnection connection, MySqlTransaction transaction)
        {
            string insertUserDataQuery = @"
        INSERT INTO UserData (UserId, Name, Gender, Age) 
        VALUES (@UserId, @Name, @Gender, @Age);
        SELECT LAST_INSERT_ID();";

            using (var command = new MySqlCommand(insertUserDataQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@Name", userData.Name);
                command.Parameters.AddWithValue("@Gender", userData.Gender);
                command.Parameters.AddWithValue("@Age", userData.Age);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static int InsertPreference(Preference preference, int userId, MySqlConnection connection, MySqlTransaction transaction)
        {
            string insertPreferenceQuery = preference == null ? @"
        INSERT INTO Preferences (UserId) 
        VALUES (@UserId);
        SELECT LAST_INSERT_ID();" : @"
        INSERT INTO Preferences (UserId, MinAge, MaxAge, PreferredGender) 
        VALUES (@UserId, @MinAge, @MaxAge, @PreferredGender);
        SELECT LAST_INSERT_ID();";

            using (var command = new MySqlCommand(insertPreferenceQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                if (preference != null)
                {
                    command.Parameters.AddWithValue("@MinAge", preference.MinAge);
                    command.Parameters.AddWithValue("@MaxAge", preference.MaxAge);
                    command.Parameters.AddWithValue("@PreferredGender", preference.Gender);
                }

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        public static void InsertNotifications(int userId, MySqlConnection connection, MySqlTransaction transaction)
        {
            string addNotification = "INSERT INTO notifications(userId) VALUES (@UserId)";

            using (var command = new MySqlCommand(addNotification, connection, transaction))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.ExecuteNonQuery();
            }
        }

        private static int UpdateConnection(int id, int userId, MySqlConnection connection, MySqlTransaction transaction)
        {
            string addConnectionQuery = @"
            UPDATE connections SET UserId = @UserId WHERE Id = @Id;";
            using (var command = new MySqlCommand(addConnectionQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@Id", id);
                int connectionId = Convert.ToInt32(command.ExecuteNonQuery());
                return connectionId;
            }
        }

        public static int AddConnection(string device)
        {
            string addConnectionQuery = @"
            INSERT INTO Connections (Device) 
            VALUES (@Device);
            SELECT LAST_INSERT_ID();";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(addConnectionQuery, connection))
                {
                    command.Parameters.AddWithValue("@Device", device);
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }
        public static void UpdateConnection(int connectionId, int userId)
        {
            string addConnectionQuery = "UPDATE connections SET UserId = @UserId WHERE Id = @Id;";
            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(addConnectionQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Id", connectionId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static ConnectionInfo GetConnectionInfo(int connectionId, int defaultUserId = 0)
        {
            string getMessagesQuery = @"SELECT Id, UserId, Device, LastOnline FROM connections WHERE
                                       Id = @ConnectionId";

            using (var connection = GetConnection())
            using (var getMessagesCommand = new MySqlCommand(getMessagesQuery, connection))
            {
                getMessagesCommand.Parameters.AddWithValue("@ConnectionId", connectionId);
                using (var reader = getMessagesCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new ConnectionInfo(reader.GetInt32("Id"), reader.IsDBNull("UserId") ? defaultUserId : reader.GetInt32("UserId"), reader.GetString("Device"), reader.IsDBNull("LastOnline") ? null : reader.GetDateTime("LastOnline"));
                    }
                }
            }
            return null;
        }

        public static List<ConnectionInfo> GetAllUserConnections(int userId)
        {
            string getMessagesQuery = @"SELECT Id, UserId, Device, LastOnline FROM connections WHERE
                                       UserId = @UserId";
            List<ConnectionInfo> info = new List<ConnectionInfo>();
            using (var connection = GetConnection())
            using (var getMessagesCommand = new MySqlCommand(getMessagesQuery, connection))
            {
                getMessagesCommand.Parameters.AddWithValue("UserId", userId);
                using (var reader = getMessagesCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        info.Add(new ConnectionInfo(reader.GetInt32("Id"), reader.GetInt32("UserId"), reader.GetString("Device"), reader.IsDBNull("LastOnline") ? null : reader.GetDateTime("LastOnline")));
                    }
                }
            }
            return info;
        }

        public static int[] GetConnectionCount(int userId)
        {
            string getMessagesQuery = @"SELECT 
    COUNT(*) AS DevicesCount, 
    CASE 
        WHEN COUNT(*) = 0 THEN 0
        ELSE SUM(CASE WHEN LastOnline IS NULL THEN 1 ELSE 0 END)
    END AS OnlineDevicesCount
FROM connections 
WHERE UserId = @UserId";

            using (var connection = GetConnection())
            using (var getMessagesCommand = new MySqlCommand(getMessagesQuery, connection))
            {
                getMessagesCommand.Parameters.AddWithValue("@UserId", userId);
                using (var reader = getMessagesCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return [reader.GetInt32("DevicesCount"), reader.GetInt32("OnlineDevicesCount")];
                    }
                }
            }
            return [-1, -1];
        }

        public static bool VerifyConnection(int id, int userId, string device, out DateTime lastConnection)
        {
            string verifyCommand = @"SELECT COUNT(*), LastConnection
                            FROM connections 
                            WHERE Id = @Id AND UserId = @UserId AND Device = @Device;";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(verifyCommand, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Device", device);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int count = reader.GetInt32(0);
                            lastConnection = reader.IsDBNull(1) ? DateTime.MinValue : reader.GetDateTime(1);
                            return count == 1;
                        }
                    }
                }
            }
            lastConnection = DateTime.MinValue;
            return false;
        }

        public static void DeleteUnknownConnection(int id)
        {
            string updatePreferenceQuery = @"
            DELETE FROM connections WHERE Id = @Id;";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(updatePreferenceQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            string hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }

        public static List<Message> GetAllMessages()
        {
            var messages = new List<Message>();
            string getMessagesQuery = "SELECT * FROM Messages";

            using (var connection = GetConnection())
            using (var getMessagesCommand = new MySqlCommand(getMessagesQuery, connection))
            using (var reader = getMessagesCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    var message = new Message
                    {
                        Id = reader.GetInt32("Id"),
                        Chat = reader.GetInt32("ChatId"),
                        Sender = reader.GetInt32("UserId"),
                        Text = reader.GetString("Text"),
                        Time = reader.GetDateTime("Time"),
                        LastUpdate = reader.GetDateTime("LastUpdate")
                    };
                    messages.Add(message);
                }
            }
            return messages;
        }

        public static string ChangePassword(int userId, string password)
        {
            password = HashPassword(password);
            string updateUserQuery = @"
            UPDATE Users 
            SET Password = @Password
            WHERE Id = @Id";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(updateUserQuery, connection))
                {
                    command.Parameters.AddWithValue("@Password", password);
                    command.Parameters.AddWithValue("@Id", userId);

                    int result = command.ExecuteNonQuery();
                    if (result > 0) return password;
                }
            }
            return string.Empty;
        }

        public static bool UpdateUserData(UserData userData, int userId)
        {
            string updateUserDataQuery = @"
            UPDATE UserData 
            SET Name = @Name, Gender = @Gender, Age = @Age 
            WHERE UserId = @UserId";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(updateUserDataQuery, connection))
                {
                    command.Parameters.AddWithValue("@Name", userData.Name);
                    command.Parameters.AddWithValue("@Gender", userData.Gender);
                    command.Parameters.AddWithValue("@Age", userData.Age);
                    command.Parameters.AddWithValue("@UserId", userId);

                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }

        public static bool UpdatePreference(Preference preference, int userId)
        {
            string updatePreferenceQuery = @"
            UPDATE Preferences 
            SET MinAge = @MinAge, MaxAge = @MaxAge, PreferredGender = @PreferredGender 
            WHERE UserId = @UserId";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(updatePreferenceQuery, connection))
                {
                    command.Parameters.AddWithValue("@MinAge", preference.MinAge);
                    command.Parameters.AddWithValue("@MaxAge", preference.MaxAge);
                    command.Parameters.AddWithValue("@PreferredGender", preference.Gender);
                    command.Parameters.AddWithValue("@UserId", userId);

                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }

        public static Chat CreateChat(int[] users, string type)
        {
            Chat chat = CreateChat(type);
            chat.Users = users;
            foreach (int userId in users)
                AddUserToChat(userId, chat.Id);
            return chat;
        }

        public static DateTime EndChat(int chatId)
        {
            string endChatQuery = "UPDATE Chats SET EndTime = NOW() WHERE Id = @Id";

            string getChatQuery = "SELECT EndTime FROM Chats WHERE Id = @Id";

            using (var connection = GetConnection())
            {
                using (var updateCommand = new MySqlCommand(endChatQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@Id", chatId);
                    updateCommand.ExecuteNonQuery();
                }

                using (var selectCommand = new MySqlCommand(getChatQuery, connection))
                {
                    selectCommand.Parameters.AddWithValue("@Id", chatId);
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetDateTime("EndTime");

                        }
                    }
                }
            }

            throw new Exception($"Чат с ID {chatId} не найден после обновления");
        }

        private static Chat CreateChat(string type)
        {
            string createChatQuery = @"
                INSERT INTO Chats (Type) VALUES (@Type);
                SELECT * FROM Chats
                ORDER BY id DESC
                LIMIT 1;";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(createChatQuery, connection))
                {
                    command.Parameters.AddWithValue("@Type", type);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Chat chat = new Chat()
                            {
                                Id = reader.GetInt32("Id"),
                                CreationTime = reader.GetDateTime("CreationTime"),
                                Type = reader.GetString("Type")
                            };
                            return chat;
                        }
                    }
                }
            }
            throw new Exception("Can't read chat from DB");
        }

        private static bool AddUserToChat(int userId, int chatId)
        {
            string addUserToChatQuery = @"
            INSERT INTO UsersChats (UserId, ChatId) 
            VALUES (@UserId, @ChatId)";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(addUserToChatQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ChatId", chatId);

                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }

        public static Message SendMessage(Message message)
        {
            string sendMessageQuery = @"
                INSERT INTO Messages (ChatId, UserId, Text) 
                VALUES (@ChatId, @UserId, @Text);
                SELECT LAST_INSERT_ID();";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(sendMessageQuery, connection))
                {
                    command.Parameters.AddWithValue("@ChatId", message.Chat);
                    command.Parameters.AddWithValue("@UserId", message.Sender);
                    command.Parameters.AddWithValue("@Text", message.Text);

                    var insertedId = Convert.ToInt32(command.ExecuteScalar());

                    if (insertedId > 0)
                    {
                        string getMessageQuery = @"
                            SELECT Id, ChatId, UserId, Text, Time, LastUpdate 
                            FROM Messages 
                            WHERE Id = @Id";

                        using (var getCommand = new MySqlCommand(getMessageQuery, connection))
                        {
                            getCommand.Parameters.AddWithValue("@Id", insertedId);

                            using (var reader = getCommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    return new Message
                                    {
                                        Id = reader.GetInt32("Id"),
                                        Chat = reader.GetInt32("ChatId"),
                                        Sender = reader.GetInt32("UserId"),
                                        Text = reader.GetString("Text"),
                                        Time = reader.GetDateTime("Time"),
                                        LastUpdate = reader.GetDateTime("LastUpdate")
                                    };
                                }
                            }
                        }
                    }

                    return null;
                }
            }
        }

        public static List<Message> GetMessagesByChatId(int chatId)
        {
            var messages = new List<Message>();
            string getMessagesQuery = "SELECT * FROM Messages WHERE ChatId = @ChatId";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(getMessagesQuery, connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var message = new Message
                            {
                                Id = reader.GetInt32("Id"),
                                Chat = reader.GetInt32("ChatId"),
                                Sender = reader.GetInt32("UserId"),
                                Text = reader.GetString("Text"),
                                Time = reader.GetDateTime("Time"),
                                LastUpdate = reader.GetDateTime("LastUpdate")
                            };
                            messages.Add(message);
                        }
                    }
                }
            }
            return messages;
        }

        public static int[] GetUsersInSameChats(int targetUserId)
        {
            var userIds = new List<int>();

            const string query = @"
        SELECT DISTINCT u1.UserId
        FROM aichat.userschats u1
        JOIN aichat.userschats u2 ON u1.ChatId = u2.ChatId
        WHERE u2.UserId = @targetUserId AND u1.UserId != @targetUserId";

            using var connection = GetConnection();
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@targetUserId", targetUserId);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                userIds.Add(reader.GetInt32(0));
            }

            return userIds.ToArray();
        }
        public static ConnectionInfo RemoveConnection(int id)
        {
            string getConnectionQuery = @"
    SELECT Id, UserId, Device, LastOnline 
    FROM Connections 
    WHERE Id = @Id";

            string removeConnectionQuery = @"
    UPDATE Connections SET UserId = NULL 
    WHERE Id = @Id";

            using (var connection = GetConnection())
            {
                ConnectionInfo connectionInfo = null;
                using (var command = new MySqlCommand(getConnectionQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            connectionInfo = new ConnectionInfo(
                                reader.GetInt32(0),
                                reader.GetInt32(1),
                                reader.GetString(2),
                                reader.IsDBNull(3) ? null : reader.GetDateTime(3)
                            );
                        }
                    }
                }

                if (connectionInfo != null)
                {
                    using (var command = new MySqlCommand(removeConnectionQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.ExecuteNonQuery();
                    }
                }

                return connectionInfo;
            }
        }

        public static bool IsUserPremium(int userId)
        {
            string isPremiumQuery = @"
            SELECT PremiumUntil 
            FROM Users 
            WHERE Id = @UserId AND PremiumUntil > NOW()";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(isPremiumQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }
        }
        public static bool SetLastConnection(int connectionId, bool isOnline)
        {
            string updatePreferenceQuery = isOnline ? @"
            UPDATE Connections
            SET LastConnection = NULL, LastOnline = NULL
            WHERE Id =@ConnectionId"
            : @"
            UPDATE Connections
            SET LastOnline = COALESCE(LastOnline, NOW()),
            LastConnection = NOW()
            WHERE Id = @ConnectionId";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(updatePreferenceQuery, connection))
                {
                    command.Parameters.AddWithValue("@ConnectionId", connectionId);
                    int result = command.ExecuteNonQuery();

                    return result > 0;
                }
            }
        }
        public static bool SetLastOnline(int connectionId, bool isOnline)
        {
            string updatePreferenceQuery = isOnline ? @"
            UPDATE Connections
            SET LastOnline = NULL
            WHERE Id =@ConnectionId"
            : @"
            UPDATE Connections
            SET LastOnline = NOW()
            WHERE Id = @ConnectionId";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(updatePreferenceQuery, connection))
                {
                    command.Parameters.AddWithValue("@ConnectionId", connectionId);
                    int result = command.ExecuteNonQuery();

                    return result > 0;
                }
            }
        }
        public static (List<Chat>, List<Chat>) GetNewChats(int userId, DateTime lastOnline)
        {
            var chats = (new List<Chat>(), new List<Chat>());
            string getMessagesQuery = @" SELECT c.Id, c.CreationTime, c.EndTime, c.Type
                                        FROM Chats c
                                        JOIN UsersChats uc ON c.Id = uc.ChatId
                                        WHERE uc.UserId = @UserId 
                                        AND (c.CreationTime > @LastOnline OR 
                                             (c.EndTime IS NOT NULL AND c.EndTime > @LastOnline));";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(getMessagesQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@LastOnline", lastOnline);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Chat chat;
                            chat = new Chat()
                            {
                                Id = reader.GetInt32("Id"),
                                CreationTime = reader.GetDateTime("CreationTime"),
                                EndTime = reader.IsDBNull("EndTime") ? null : reader.GetDateTime("EndTime"),
                                Type = reader.GetString("Type")
                            };
                            if (chat.CreationTime > lastOnline)
                            {
                                chats.Item1.Add(chat);
                            }
                            else
                            {
                                chats.Item2.Add(chat);
                            }
                        }
                    }
                }
            }
            return chats;
        }

        public static (List<Message>, List<Message>) GetNewMessages(int userId, DateTime lastOnline)
        {
            var messages = (new List<Message>(), new List<Message>());
            string getMessagesQuery = @" SELECT m.*
                                            FROM Messages m
                                            JOIN UsersChats uc ON m.ChatId = uc.ChatId
                                            WHERE uc.UserId = @UserId
                                            AND (m.LastUpdate > @LastOnline)
                                            ORDER BY m.ChatId;";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(getMessagesQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@LastOnline", lastOnline);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var message = new Message
                            {
                                Id = reader.GetInt32("Id"),
                                Chat = reader.GetInt32("ChatId"),
                                Sender = reader.GetInt32("UserId"),
                                Text = reader.GetString("Text"),
                                Time = reader.GetDateTime("Time"),
                                LastUpdate = reader.GetDateTime("LastUpdate")
                            };
                            if (message.Time > lastOnline)
                                messages.Item1.Add(message);
                            else
                                messages.Item2.Add(message);
                        }
                    }
                }
            }
            return messages;
        }
        public static (List<int>, List<UserData>, List<bool>) LoadUsers(int chatId)
        {
            (List<int>, List<UserData>, List<bool>) data = (new List<int>(), new List<UserData>(), new List<bool>());
            string isOnlineQuery = @"
                SELECT 
    ud.*,
    CASE WHEN EXISTS (
        SELECT 1 FROM connections 
        WHERE UserId = ud.UserId AND LastOnline IS NULL
    ) THEN 1 ELSE 0 END AS IsOnline
FROM
    userdata ud
WHERE
    ud.UserId IN (
        SELECT UserId FROM userschats WHERE ChatId = @ChatId
    )";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(isOnlineQuery, connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Item1.Add(reader.GetInt32("UserId"));
                            data.Item2.Add(new UserData()
                            {
                                Id = reader.GetInt32("Id"),
                                Name = reader.GetString("Name"),
                                Gender = reader.GetChar("Gender"),
                                Age = reader.GetInt32("Age")
                            });
                            data.Item3.Add(reader.GetBoolean("IsOnline"));

                        }
                    }
                }
            }
            return data;
        }
        public static Dictionary<int, Chat> GetChats()
        {
            var chats = new Dictionary<int, Chat>();

            string getChatsQuery = "SELECT * FROM Chats WHERE EndTime IS NULL";
            string getUserChatsQuery = "SELECT ChatId, UserId FROM userschats WHERE ChatId = @ChatId";

            using (var connection = GetConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var getChatsCommand = new MySqlCommand(getChatsQuery, connection, transaction))
                        {
                            using (var reader = getChatsCommand.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var chat = new Chat
                                    {
                                        Id = reader.GetInt32("Id"),
                                        CreationTime = reader.GetDateTime("CreationTime"),
                                        Type = reader.GetString("Type"),
                                        Users = Array.Empty<int>()
                                    };
                                    chats.Add(chat.Id, chat);
                                }
                            }
                        }
                        using (var getUserChatsCommand = new MySqlCommand(getUserChatsQuery, connection, transaction))
                        {
                            getUserChatsCommand.Parameters.Add("@ChatId", MySqlDbType.Int32);

                            foreach (var chat in chats.Values)
                            {
                                getUserChatsCommand.Parameters["@ChatId"].Value = chat.Id;

                                var userIds = new List<int>();
                                using (var reader = getUserChatsCommand.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        userIds.Add(reader.GetInt32("UserId"));
                                    }
                                }

                                chat.Users = userIds.ToArray();
                            }
                        }

                        transaction.Commit();
                        return chats;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

        }
        public static bool UpdateNotifications(int userId, bool value)
        {
            string updateQuery = @"
        UPDATE notifications 
        SET EmailNotifications = @EmailNotifications
        WHERE UserId = @UserId";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@EmailNotifications", value);
                    command.Parameters.AddWithValue("@UserId", userId);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public static bool GetNotifications(int userId)
        {
            string selectQuery = @"
    SELECT EmailNotifications 
    FROM notifications 
    WHERE UserId = @UserId";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToBoolean(result);
                    }
                    return false;
                }
            }
        }
        public static AIMessage AddAIMessage(AIMessage aIMessage, string type)
        {
            string query = @"
                INSERT INTO AIMessages (ChatId, Text, Role, Type) 
                VALUES (@ChatId, @Text, @Role, @Type);
                SELECT LAST_INSERT_ID();";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ChatId", aIMessage.ChatId);
                    command.Parameters.AddWithValue("@Text", aIMessage.Content);
                    command.Parameters.AddWithValue("@Role", aIMessage.Role);
                    command.Parameters.AddWithValue("@Type", type);

                    int messageId = Convert.ToInt32(command.ExecuteScalar());

                    return new AIMessage(messageId, aIMessage.ChatId, aIMessage.Role, aIMessage.Content);
                }
            }
        }
        public static Dictionary<int, AIMessageDispatcher> GetAIMessagesByChat(List<int> chatIds)
        {
            var dispatchers = new Dictionary<int, AIMessageDispatcher>();
            if (chatIds == null || chatIds.Count == 0)
            {
                return dispatchers;
            }

            var messagesByChat = new Dictionary<int, (List<AIMessage> MainMessages, List<AIMessage> CompressedMessages)>();
            string query = $@"
                    SELECT Id, ChatId, Text, Role, Type 
                    FROM AIMessages 
                    WHERE ChatId IN ({string.Join(",", chatIds.Select((_, i) => $"@ChatId{i}"))}) 
                    ORDER BY ChatId, Id ASC";
            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(query, connection))
                {
                    for (int i = 0; i < chatIds.Count; i++)
                    {
                        command.Parameters.AddWithValue($"@ChatId{i}", chatIds[i]);
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var chatId = reader.GetInt32("ChatId");
                            var message = new AIMessage(
                                reader.GetInt32("Id"),
                                chatId,
                                reader.GetString("Role"),
                                reader.GetString("Text")
                            );
                            if (!messagesByChat.TryGetValue(chatId, out var messageLists))
                            {
                                messageLists = (new List<AIMessage>(), new List<AIMessage>());
                                messagesByChat[chatId] = messageLists;
                            }
                            if (reader.GetString("Type") == "message")
                            {
                                messageLists.MainMessages.Add(message);
                            }
                            else
                            {
                                messageLists.CompressedMessages.Add(message);
                            }
                        }
                    }
                }
            }

            foreach (var chatId in chatIds)
            {
                if (messagesByChat.TryGetValue(chatId, out var messages))
                {
                    var dispatcher = new AIMessageDispatcher();
                    dispatcher.SetMainMessages(messages.MainMessages);
                    dispatcher.SetCompressedMessages(messages.CompressedMessages);
                    dispatchers[chatId] = dispatcher;
                }
                else
                {
                    dispatchers[chatId] = new AIMessageDispatcher();
                }
            }

            return dispatchers;
        }
        public static bool DeleteAIMessages(List<AIMessage> messages)
        {
            if (messages == null || messages.Count == 0)
                return true;

            string query = @"
                DELETE FROM AIMessages 
                WHERE Id IN (" + string.Join(",", messages.Select((_, i) => $"@Id{i}")) + ")";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(query, connection))
                {
                    for (int i = 0; i < messages.Count; i++)
                    {
                        command.Parameters.AddWithValue($"@Id{i}", messages[i].Id);
                    }

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }
        public static bool AddChatTokenUsage(int chatId)
        {
            string query = @"
                INSERT INTO ChatTokenUsage (ChatId) 
                VALUES (@ChatId);";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
        public static bool UseToken(int chatId, int tokenCount)
        {
            string query = @"
                UPDATE ChatTokenUsage
                SET TokensCount = TokensCount + @TokensCount
                WHERE ChatId = @ChatId";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    command.Parameters.AddWithValue("@TokensCount", tokenCount);
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
    }
}