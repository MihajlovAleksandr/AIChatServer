using MySql.Data.MySqlClient;
namespace AIChatServer
{

    public static class DatabaseHelper
    {
        public static MySqlConnection GetConnection(string connectionString)
        {
            var connection = new MySqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public static List<User> GetAllUsers(string connectionString)
        {
            var users = new List<User>();
            string getUsersQuery = @"
                SELECT u.Id, u.Username, u.Name, u.Age, u.Gender,
                       p.MinAge, p.MaxAge, p.Gender as PrefGender
                FROM Users u
                LEFT JOIN Preferences p ON u.Preference = p.Id";

            using (var connection = GetConnection(connectionString))
            using (var getUsersCommand = new MySqlCommand(getUsersQuery, connection))
            using (var reader = getUsersCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    var user = new User
                    {
                        id = reader.GetInt32("Id"),
                        username = reader.GetString("Username"),
                        name = reader.GetString("Name"),
                        age = reader.GetInt32("Age"),
                        gender = reader.GetChar("Gender"),
                        Preference = new Preference
                        {
                            MinAge = reader.IsDBNull(reader.GetOrdinal("MinAge")) ? 0 : reader.GetInt32("MinAge"),
                            MaxAge = reader.IsDBNull(reader.GetOrdinal("MaxAge")) ? 0 : reader.GetInt32("MaxAge"),
                            Gender = reader.IsDBNull(reader.GetOrdinal("PrefGender")) ? null : reader.GetChar("PrefGender")
                        }
                    };
                    users.Add(user);
                }
            }
            Thread.Sleep(5000);
            return users;
        }

        public static List<Message> GetAllMessages(string connectionString)
        {
            var messages = new List<Message>();
            string getMessagesQuery = "SELECT * FROM Message";

            using (var connection = GetConnection(connectionString))
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


