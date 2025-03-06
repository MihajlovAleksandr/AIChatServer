using Newtonsoft.Json;

namespace AIChatServer
{
    public class Preference
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("minAge")]

        public int MinAge { get; set; }
        [JsonProperty("maxAge")]

        public int MaxAge { get; set; }
        [JsonProperty("gender")]

        public string Gender { get; set; }
        public override string ToString()
        {
            return $"Preference {{{Id}}}:\n{Gender}{MinAge}-{MaxAge}";
        }
    }
}
