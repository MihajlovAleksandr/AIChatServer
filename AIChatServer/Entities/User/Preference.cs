namespace AIChatServer.Entities.User
{
    public class Preference
    {
        public Guid Id { get; set; }
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public PreferenceGender Gender { get; set; }

        public override string ToString()
        {
            return $"Preference {{{Id}}}:\n{Gender}{MinAge}-{MaxAge}";
        }
    }
}
