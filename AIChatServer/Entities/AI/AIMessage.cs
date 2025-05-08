using AIChatServer.Entities.Chats;
using Newtonsoft.Json;
using System.Reflection.Metadata;

namespace AIChatServer.Entities.AI
{

    public class AIMessage
    {
        private int id;
        private string role;
        private string content;
        private int chatId;
        [JsonIgnore]
        public int Id { get { return id; } set { id = value; } }
        [JsonIgnore]
        public int ChatId { get { return chatId; } set { chatId = value; } }
        [JsonProperty("role")]
        public string Role { get { return role; } set { role = value; } }
        [JsonProperty("content")]
        public string Content { get { return content; } set { content = value; } }

        public AIMessage(int id,int chatId, string role, string content)
        {
            this.id = id;
            this.chatId = chatId;
            this.role = role;
            this.content = content;
        }
        public AIMessage()
        {

        }
    }

}
