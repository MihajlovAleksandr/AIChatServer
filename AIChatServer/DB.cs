using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.MySqlClient;
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
                    u.preference, 
                    u.Premium,
                    u.UserData,
                    p.MinAge, 
                    p.MaxAge, 
                    p.Gender AS PrefGender,
                    d.Gender,
                    d.Name,
                    d.Age
                FROM 
                    Users u
                JOIN 
                    Preferences p ON u.Preference = p.Id
                JOIN 
                    UserData d ON u.UserData = d.id
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
                                id = reader.GetInt32(reader.GetOrdinal("Id")),
                                email = reader.GetString(reader.GetOrdinal("Email")),
                                premium = reader.IsDBNull(reader.GetOrdinal("Premium")) ? null : reader.GetDateTime(reader.GetOrdinal("Premium")),
                                preference = new Preference
                                {
                                    Id = reader.IsDBNull(reader.GetOrdinal("Preference")) ? 0 : reader.GetInt32(reader.GetOrdinal("Preference")),
                                    MinAge = reader.IsDBNull(reader.GetOrdinal("MinAge")) ? 0 : reader.GetInt32(reader.GetOrdinal("MinAge")),
                                    MaxAge = reader.IsDBNull(reader.GetOrdinal("MaxAge")) ? 0 : reader.GetInt32(reader.GetOrdinal("MaxAge")),
                                    Gender = reader.IsDBNull(reader.GetOrdinal("PrefGender")) ? null : reader.GetChar  (reader.GetOrdinal("PrefGender"))
                                },
                                userData = new UserData
                                {
                                    Id = reader.IsDBNull(reader.GetOrdinal("UserData")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserData")),
                                    Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Name")),
                                    Age = reader.IsDBNull(reader.GetOrdinal("Age")) ? 0 : reader.GetInt32(reader.GetOrdinal("Age")),
                                    Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? ' ' : reader.GetChar(reader.GetOrdinal("Gender"))
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
                    u.preference,
                    u.Premium,
                    u.UserData,
                    p.MinAge, 
                    p.MaxAge, 
                    p.Gender AS PrefGender,
                    d.Gender,
                    d.Name,
                    d.Age
                FROM 
                    Users u
                JOIN 
                    Preferences p ON u.Preference = p.Id
                JOIN 
                    UserData d ON u.UserData = d.id
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
                                id = reader.GetInt32(reader.GetOrdinal("Id")),
                                email = reader.GetString(reader.GetOrdinal("Email")),
                                password = reader.GetString(reader.GetOrdinal("Password")),
                                premium = reader.IsDBNull(reader.GetOrdinal("Premium")) ? null : reader.GetDateTime(reader.GetOrdinal("Premium")),
                                preference = new Preference
                                {
                                    Id = reader.IsDBNull(reader.GetOrdinal("Preference")) ? 0 : reader.GetInt32(reader.GetOrdinal("Preference")),
                                    MinAge = reader.IsDBNull(reader.GetOrdinal("MinAge")) ? 0 : reader.GetInt32(reader.GetOrdinal("MinAge")),
                                    MaxAge = reader.IsDBNull(reader.GetOrdinal("MaxAge")) ? 0 : reader.GetInt32(reader.GetOrdinal("MaxAge")),
                                    Gender = reader.IsDBNull(reader.GetOrdinal("PrefGender")) ? null : reader.GetChar(reader.GetOrdinal("PrefGender"))
                                },
                                userData = new UserData
                                {
                                    Id = reader.IsDBNull(reader.GetOrdinal("UserData")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserData")),
                                    Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Name")),
                                    Age = reader.IsDBNull(reader.GetOrdinal("Age")) ? 0 : reader.GetInt32(reader.GetOrdinal("Age")),
                                    Gender = reader.IsDBNull(reader.GetOrdinal("Gender")) ? ' ' : reader.GetChar(reader.GetOrdinal("Gender"))
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
                if (VerifyPassword(password, user.password))
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
        public static int? AddUser(User user)
        {
            using (var connection = GetConnection())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Вставка UserData
                        int userDataId = InsertUserData(user.userData, connection, transaction);

                        // Вставка Preferences
                        int preferenceId = InsertPreference(user.preference ?? new Preference(), connection, transaction);

                        // Вставка Users и получение id нового пользователя
                        int userId = InsertUser(user, userDataId, preferenceId, connection, transaction);

                        transaction.Commit();
                        return userId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return null;
                    }
                }
            }
        }

        private static int InsertUserData(UserData userData, MySqlConnection connection, MySqlTransaction transaction)
        {
            string insertUserDataQuery = @"
        INSERT INTO UserData (Name, Age, Gender) 
        VALUES (@Name, @Age, @Gender);
        SELECT LAST_INSERT_ID();";

            using (var command = new MySqlCommand(insertUserDataQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@Name", userData.Name);
                command.Parameters.AddWithValue("@Age", userData.Age);
                command.Parameters.AddWithValue("@Gender", userData.Gender);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static int InsertPreference(Preference preference, MySqlConnection connection, MySqlTransaction transaction)
        {
            string insertPreferenceQuery = @"
INSERT INTO Preferences (MinAge, MaxAge, Gender) 
VALUES (@MinAge, @MaxAge, @Gender);
SELECT LAST_INSERT_ID();";

            using (var command = new MySqlCommand(insertPreferenceQuery, connection, transaction))
            {
                // Проверяем на null значения и устанавливаем параметры соответственно
                command.Parameters.AddWithValue("@MinAge", (object)preference.MinAge);
                command.Parameters.AddWithValue("@MaxAge", (object)preference.MaxAge);
                command.Parameters.AddWithValue("@Gender", (object)preference.Gender);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        private static int InsertUser(User user, int userDataId, int preferenceId, MySqlConnection connection, MySqlTransaction transaction)
        {
            string insertUserQuery = @"
        INSERT INTO Users (Email, Password, Preference, Premium, UserData) 
        VALUES (@Email, @Password, @Preference, @Premium, @UserData);
        SELECT LAST_INSERT_ID();";

            using (var command = new MySqlCommand(insertUserQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@Email", user.email);
                command.Parameters.AddWithValue("@Password", HashPassword(user.password));
                command.Parameters.AddWithValue("@Preference", preferenceId);
                command.Parameters.AddWithValue("@Premium", user.premium);
                command.Parameters.AddWithValue("@UserData", userDataId);

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }
        public static bool UpdatePreference(int userId, Preference newPreference)
        {
            string updatePreferenceQuery = @"
                UPDATE Preferences
                SET MinAge = @MinAge, MaxAge = @MaxAge, Gender = @Gender
                WHERE Id = (
                    SELECT Preference 
                    FROM Users 
                    WHERE Id = @UserId)";

            using (var connection = GetConnection())
            {
                using (var command = new MySqlCommand(updatePreferenceQuery, connection))
                {
                    command.Parameters.AddWithValue("@MinAge", newPreference.MinAge);
                    command.Parameters.AddWithValue("@MaxAge", newPreference.MaxAge);
                    command.Parameters.AddWithValue("@Gender", newPreference.Gender);
                    command.Parameters.AddWithValue("@UserId", userId);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
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

        public static List<Message> GetAllMessages(string connectionString)
        {
            var messages = new List<Message>();
            string getMessagesQuery = "SELECT * FROM Message";

            using (var connection = GetConnection())
            using (var getMessagesCommand = new MySqlCommand(getMessagesQuery, connection))
            using (var reader = getMessagesCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    var message = new Message
                    {
                        id = reader.GetInt32("Id"),
                        text = reader.GetString("Text"),
                        sender = reader.GetInt32("User"),
                        chat = reader.GetInt32("Chat"),
                        time = reader.GetDateTime("Time")
                    };
                    messages.Add(message);
                }
            }
            return messages;
        }
    }
}


