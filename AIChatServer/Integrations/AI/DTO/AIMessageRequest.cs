using Newtonsoft.Json;

namespace AIChatServer.Integrations.AI.DTO
{
    public class AIMessageRequest (string content, string role)
    {
        [JsonProperty("role")]
        public string Role { get; set; } = role;
        [JsonProperty("content")]
        public string Content { get; set; } = content;
    }
}
