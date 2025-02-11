namespace AIChatServer
{
    public class User
    {
        public int id;
        public string? name;
        public string username;
        public string password;
        public DateTime? premium;
        public int age;
        public char? gender;
        public Preference preference;
        public override string ToString()
        {
            return $"User {{{id}}}:\n{username}\n{gender}: {name} ({age}) \n{premium}\n{preference}";
        }
    }
}
