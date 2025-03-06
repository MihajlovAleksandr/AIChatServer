using Newtonsoft.Json;

namespace AIChatServer
{
    public class User
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public DateTime? Premium { get; set; }

        public int Points { get; set; }

        public UserData UserData { get; set; }
        
        public Preference Preference { get; set; }
        public User()
        {

        }
        public User(string email, string password)
        {
            Email = email;
            Password = password;
            
        }
        public override string ToString()
        {
            return $"User {{{Id}}}:\n{Email}\n{Premium}/{Points}\n{UserData}\n{Preference}";
        }
    }
}
