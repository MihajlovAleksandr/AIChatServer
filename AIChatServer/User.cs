namespace AIChatServer
{
    public class User
    {
        public int id;
        public string email;
        public string password;
        public DateTime? premium;
        public Preference preference;
        public UserData userData;
        public override string ToString()
        {
            return $"User {{{id}}}:\n{email}\n{premium}\n{userData}\n{preference}";
        }
    }
}
