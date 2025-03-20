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

        public static bool VerifyConnection(int id, int userId, string device)
        {
            string verifyCommand = @"SELECT COUNT(*) FROM connections WHERE Id = @Id && UserId = @UserId && Device = @Device;";
            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(verifyCommand, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Device", device);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count == 1;
                }
            }
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

        public static int CreateChat(List<int> users)
        {
            int chatId = CreateChat();
            foreach (int userId in users)
                AddUserToChat(userId, chatId);
            return chatId;
        }

        public static int CreateChat()
        {
            string createChatQuery = @"
                INSERT INTO Chats () VALUES ();
                SELECT LAST_INSERT_ID();";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(createChatQuery, connection))
                {
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public static bool AddUserToChat(int userId, int chatId)
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

        public static bool SendMessage(Message message)
        {
            string sendMessageQuery = @"
            INSERT INTO Messages (ChatId, UserId, Text) 
            VALUES (@ChatId, @UserId, @Text)";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(sendMessageQuery, connection))
                {
                    command.Parameters.AddWithValue("@ChatId", message.Chat);
                    command.Parameters.AddWithValue("@UserId", message.Sender);
                    command.Parameters.AddWithValue("@Text", message.Text);

                    int result = command.ExecuteNonQuery();
                    return result > 0;
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
        public static bool SetLastOnline(int userId)
        {
            string updatePreferenceQuery = @"
            UPDATE Connections
            SET LastOnline = CURRENT_TIMESTAMP
            WHERE UserId = @UserId";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(updatePreferenceQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    int result = command.ExecuteNonQuery();

                    return result > 0;
                }
            }
        }
        
    }
}