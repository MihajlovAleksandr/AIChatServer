using MySql.Data.MySqlClient;
namespace AIChatServer
{
    public static class DB
    {
        public static Dictionary<string, object>[] GetData(string table, params string[] columns)
        {
            string connectionString = "Server=localhost;Database=aichat;User ID=root;Password=OlezkaTelezka;";
            string query = $"SELECT {ParseArray(columns)} FROM {table}";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);

                try
                {
                    connection.Open();
                    MySqlDataReader reader = command.ExecuteReader();
                    List<Dictionary<string, object>> values = new List<Dictionary<string, object>>();
                    while (reader.Read())
                    {
                        Dictionary<string, object> value = new Dictionary<string, object>();
                        for (int i = 0; i < columns.Length; i++)
                        {
                            value.Add(columns[i], reader[columns[i]]);
                        }
                        values.Add(value);
                    }
                    return values.ToArray();
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
            }
            return null;
        }
        private static string ParseArray(string[] data)
        {
            string str = "";
            for (int i = 0; i < data.Length; i++)
            {
                str += $"{data[i]}, ";
            }
            return str.Remove(str.Length-2);
        }
    }
}
