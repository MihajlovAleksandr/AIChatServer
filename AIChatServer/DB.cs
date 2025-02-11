using System.Configuration;
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


