using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Transactions;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace AIChatServer
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
                                Premium = reader.IsDBNull("Premium") ? (DateTime?)null : reader.GetDateTime("Premium"),
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
                                Premium = reader.IsDBNull("Premium") ? (DateTime?)null : reader.GetDateTime("Premium"),
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
            string insertPreferenceQuery = (preference==null)? @"
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

        public static bool VerifyConnection(int id, int userId, string device, out DateTime lastOnline)
        {
            string verifyCommand = @"SELECT COUNT(*), LastOnline 
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
                            lastOnline = reader.IsDBNull(1)? DateTime.MinValue : reader.GetDateTime(1);
                            return count == 1;
                        }
                    }
                }
            }
            lastOnline = DateTime.MinValue;
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

        private static bool VerifyPassword(string password, string hashedPassword)
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

        public static bool ChangePassword(int userId, string password)
        {
            string updateUserQuery = @"
            UPDATE Users 
            SET Password = @Password
            WHERE Id = @Id";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(updateUserQuery, connection))
                {
                    command.Parameters.AddWithValue("@Password", HashPassword(password));
                    command.Parameters.AddWithValue("@Id", userId);

                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
            }
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

        public static Chat CreateChat(int[] users)
        {
            Chat chat = CreateChat();
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

        private static Chat CreateChat()
        {
            string createChatQuery = @"
                INSERT INTO Chats () VALUES ();
                SELECT * FROM Chats
                ORDER BY id DESC
                LIMIT 1;";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(createChatQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Chat chat = new Chat()
                            {
                                Id = reader.GetInt32("Id"),
                                CreationTime = reader.GetDateTime("CreationTime")
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

                    // Получаем ID вставленной записи
                    var insertedId = Convert.ToInt32(command.ExecuteScalar());

                    if (insertedId > 0)
                    {
                        // Получаем полные данные о сообщении
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

        public static Dictionary<int, List<int>> GetUserChatsDictionary(int targetUserId)
        {
            var userChats = new Dictionary<int, List<int>>();
            string getUserChatsQuery = @"
SELECT u1.UserId, u1.ChatId
FROM aichat.userschats u1
JOIN aichat.userschats u2 ON u1.ChatId = u2.ChatId
WHERE u2.UserId = @targetUserId AND u1.UserId != @targetUserId";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(getUserChatsQuery, connection))
                {
                    command.Parameters.AddWithValue("@targetUserId", targetUserId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int userId = reader.GetInt32("UserId");
                            int chatId = reader.GetInt32("ChatId");

                            if (!userChats.ContainsKey(userId))
                            {
                                userChats[userId] = new List<int>();
                            }
                            userChats[userId].Add(chatId);
                        }
                    }
                }
            }
            return userChats;
        }
        public static bool RemoveConnection(int id)
        {
            string removeConnectionQuery = @"
            DELETE FROM Connections 
            WHERE Id = @Id";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(removeConnectionQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    int result = command.ExecuteNonQuery();
                    return result > 0;
                }
            }
        }

        public static bool MakePointsTransaction(int userId, int amount, string description)
        {
            string addTransactionQuery = @"
                INSERT INTO PointsTransactions (UserId, Amount, Description) 
                VALUES (@UserId, @Amount, @Description)";

            string updatePointsQuery = @"
                UPDATE Users
                SET Points = Points + @Amount
                WHERE Id = @UserId";

            using (var connection = GetConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var addTransactionCommand = new MySqlCommand(addTransactionQuery, connection, transaction))
                        {
                            addTransactionCommand.Parameters.AddWithValue("@UserId", userId);
                            addTransactionCommand.Parameters.AddWithValue("@Amount", amount);
                            addTransactionCommand.Parameters.AddWithValue("@Description", description);
                            addTransactionCommand.ExecuteNonQuery();
                        }

                        using (var updatePointsCommand = new MySqlCommand(updatePointsQuery, connection, transaction))
                        {
                            updatePointsCommand.Parameters.AddWithValue("@UserId", userId);
                            updatePointsCommand.Parameters.AddWithValue("@Amount", amount);
                            updatePointsCommand.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }


        public static List<PointsTransaction> GetPointsTransactionsByUserId(int userId)
        {
            var transactions = new List<PointsTransaction>();
            string getTransactionsQuery = "SELECT * FROM PointsTransactions WHERE UserId = @UserId";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(getTransactionsQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var transaction = new PointsTransaction
                            {
                                Id = reader.GetInt32("Id"),
                                UserId = reader.GetInt32("UserId"),
                                Amount = reader.GetInt32("Amount"),
                                Description = reader.GetString("Description"),
                                TransactionTime = reader.GetDateTime("TransactionTime")
                            };
                            transactions.Add(transaction);
                        }
                    }
                }
            }
            return transactions;
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
        public static bool SetLastOnline(int connectionId, bool isOnline)
        {
            string updatePreferenceQuery = (isOnline) ? @"
            UPDATE Connections
            SET LastOnline = null
            WHERE Id = @ConnectionId"
            : @"
            UPDATE Connections
            SET LastOnline = CURRENT_TIMESTAMP
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
            string getMessagesQuery = @" SELECT c.Id, c.CreationTime, c.EndTime
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
                                EndTime = reader.IsDBNull("EndTime")? null: reader.GetDateTime("EndTime")
                            };
                            if (chat.CreationTime>lastOnline)
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
                                            AND (m.LastUpdate > @LastOnline);";

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
                        JOIN
                    userschats uc ON ud.UserId = uc.UserId
                        JOIN
                    connections c ON c.UserId = uc.UserId
                WHERE
                    uc.ChatId = @ChatId";

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
    }
}