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
                SELECT u.Id, u.Username, u.Name, u.Age, u.Gender, preference,
                p.MinAge, p.MaxAge, p.Gender as PrefGender
                FROM Users u  
                LEFT JOIN Preferences p ON u.Preference = p.Id WHERE u.Id = @userId";
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
                                username = reader.GetString(reader.GetOrdinal("Username")),
                                name = reader.GetString(reader.GetOrdinal("Name")),
                                age = reader.GetInt32(reader.GetOrdinal("Age")),
                                gender = reader.GetChar(reader.GetOrdinal("Gender")),
                                preference = new Preference
                                {
                                    Id = reader.IsDBNull(reader.GetOrdinal("Preference")) ? 0 : reader.GetInt32(reader.GetOrdinal("preference")),
                                    MinAge = reader.IsDBNull(reader.GetOrdinal("MinAge")) ? 0 : reader.GetInt32(reader.GetOrdinal("MinAge")),
                                    MaxAge = reader.IsDBNull(reader.GetOrdinal("MaxAge")) ? 0 : reader.GetInt32(reader.GetOrdinal("MaxAge")),
                                    Gender = reader.IsDBNull(reader.GetOrdinal("PrefGender")) ? null : reader.GetChar("PrefGender")
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
        private static User GetUserByUsername(string username)
        {
            string getUsersQuery = @"
                SELECT u.Id, u.Username, u.Name, u.Age, u.Gender, preference, u.Password,
                p.MinAge, p.MaxAge, p.Gender as PrefGender
                FROM Users u  
                LEFT JOIN Preferences p ON u.Preference = p.Id WHERE u.Username = @username";
            using (var connection = GetConnection())
            {
                using (var getUsersCommand = new MySqlCommand(getUsersQuery, connection))
                {
                    getUsersCommand.Parameters.AddWithValue("@username", username);

                    using (var reader = getUsersCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var user = new User
                            {
                                id = reader.GetInt32(reader.GetOrdinal("Id")),
                                username = reader.GetString(reader.GetOrdinal("Username")),
                                name = reader.GetString(reader.GetOrdinal("Name")),
                                age = reader.GetInt32(reader.GetOrdinal("Age")),
                                password = reader.GetString(reader.GetOrdinal("Password")),
                                gender = reader.GetChar(reader.GetOrdinal("Gender")),
                                preference = new Preference
                                {
                                    Id = reader.IsDBNull(reader.GetOrdinal("Preference")) ? 0 : reader.GetInt32(reader.GetOrdinal("preference")),
                                    MinAge = reader.IsDBNull(reader.GetOrdinal("MinAge")) ? 0 : reader.GetInt32(reader.GetOrdinal("MinAge")),
                                    MaxAge = reader.IsDBNull(reader.GetOrdinal("MaxAge")) ? 0 : reader.GetInt32(reader.GetOrdinal("MaxAge")),
                                    Gender = reader.IsDBNull(reader.GetOrdinal("PrefGender")) ? null : reader.GetChar("PrefGender")
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
        public static User LoginIn(string username, string password)
        {
            User user = GetUserByUsername(username);
            if (user != null)
            {
                if (VerifyPassword(password, user.password))
                {
                    return user;
                }
            }
            return null;
        }
        private static string HashPassword(string password)
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
                        Id = reader.GetInt32("Id"),
                        Text = reader.GetString("Text"),
                        User = reader.GetInt32("User"),
                        Chat = reader.GetInt32("Chat"),
                        Time = reader.GetDateTime("Time")
                    };
                    messages.Add(message);
                }
            }
            return messages;
        }
    }
}


