namespace AIChatServer.Entities.User
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
        public override bool Equals(object? obj)
        {
            User other = obj as User;
            if (other == null) return false;
            return Id.Equals(other.Id);
        }
        public override string ToString()
        {
            return $"User {{{Id}}}:\n{Email}\n{Premium}/{Points}\n{UserData}\n{Preference}";
        }
    }
}
