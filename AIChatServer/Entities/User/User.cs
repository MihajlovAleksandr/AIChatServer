namespace AIChatServer.Entities.User
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string? Password { get; set; }
        public DateTime? Premium { get; set; }
        public int Points { get; set; }
        public UserData UserData { get; set; }
        public Preference Preference { get; set; }
        public string? GoogleId { get; set; }

        public User()
        {

        }

        public User(string email, string verifyParam, RegistrationType registrationType)
        {
            Email = email;
            if(registrationType == RegistrationType.Password)
            {
                Password = verifyParam;
            }
            else if(registrationType == RegistrationType.Google)
            {
                GoogleId = verifyParam;
            }
        }

        public RegistrationType? GetRegistrationType()
        {
            if (Password== null || Password == string.Empty)
            {
                if (GoogleId == string.Empty) return null;
                return RegistrationType.Google;
            }
            if (GoogleId == null || GoogleId == string.Empty) return RegistrationType.Password;
            return RegistrationType.PasswordAndGoogle;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not User other) return false;
            return Id.Equals(other.Id);
        }

        public override string ToString()
        {
            return $"User {{{Id}}}:\n{Email}\n{Premium}/{Points}\n{UserData}\n{Preference}";
        }
        
    }
}
