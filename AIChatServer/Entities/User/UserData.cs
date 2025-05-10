using Newtonsoft.Json;
using System.Text.Json;

namespace AIChatServer.Entities.User
{
    public class UserData
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("gender")]
        public char Gender { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("age")]
        public int Age { get; set; }

        public bool IsFits(Preference preference)
        {
            if (preference.Gender[0] == Gender || preference.Gender=="Any")
            {
                if (Age >= preference.MinAge && Age <= preference.MaxAge)
                {
                    return true;
                }
            }
            return false;
        }
        public override string ToString()
        {
            return $"UserData {{{Id}}}\n{Gender}{Age}\n{Name}";
        }
    }
}
