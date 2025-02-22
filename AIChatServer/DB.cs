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
                                premium = reader.GetDateTime(reader.GetOrdinal("Premium")),
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
                                premium = reader.GetDateTime(reader.GetOrdinal("Premium")),
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


