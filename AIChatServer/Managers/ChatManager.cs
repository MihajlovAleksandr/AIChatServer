using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;
using AIChatServer.Utils;
using Org.BouncyCastle.Tls;
using System.Threading.Tasks;

namespace AIChatServer.Managers
{
    public class ChatManager
    {
        private readonly int aiId;
        private readonly int probabilityAIChat;
        private Dictionary<int,Chat> chatList;
        private List<User> usersWithoutChat;
        public event Action<Chat> OnChatCreated;
        public event Action<Chat> OnChatEnded;
        private readonly object syncObjChats = new object();
        private readonly object syncObjUsers = new object();
        private Random random;

        public ChatManager(int aiId, int probabilityAIChat)
        {
            chatList = DB.GetChats();
            random = new Random();
            usersWithoutChat = new List<User>();
            this.aiId = aiId;
            this.probabilityAIChat = probabilityAIChat;
        }
        public int[] GetUsersInChat(int chat)
        {
            lock (syncObjChats)
            {
                return chatList[chat].Users;
            }
        }

        public void EndChat(int chatId)
        {
            lock (syncObjChats)
            {
                Chat chat = chatList[chatId];
                chat.EndTime = DB.EndChat(chatId);
                OnChatEnded.Invoke(chat);
                chatList.Remove(chatId);
            }
        }
        public async Task SearchChatAsync(User user, string type)
        {
            switch(type)
            {
                case "random": 
                    await CreateRandomChat(user);
                    break;
                case "ai":
                    CreateAIChat(user);
                    break;
                case "human":
                    CreateUserChat(user);
                    break;
            }   

        }
        private async Task CreateRandomChat(User user)
        {
            double randomValue = random.NextDouble() * 100;
            if (randomValue < probabilityAIChat)
            {
                await Task.Delay(random.Next(500, 10000));
                CreateAIChat(user, "random");
            }
            else CreateUserChat(user, "random");
        }
        private void CreateAIChat(User user, string type= "ai")
        {
            CreateChat([user.Id, aiId], type);
        }
        private void CreateUserChat(User user, string type = "human")
        {
            lock (syncObjUsers)
            {
                for (int i = 0; i < usersWithoutChat.Count; i++)
                {
                    if (user.UserData.IsFits(usersWithoutChat[i].Preference))
                    {
                        if (usersWithoutChat[i].UserData.IsFits(user.Preference))
                        {
                            CreateChat([user, usersWithoutChat[i]], type);
                            return;
                        }
                    }
                }
                usersWithoutChat.Add(user);
            }
        }
        private void CreateChat(User[] users, string type)
        {
            usersWithoutChat.Remove(users[1]);
            CreateChat(users.Select(user => user.Id).ToArray(), type);
        }
        private void CreateChat(int[] users, string type)
        {
            Chat chat = DB.CreateChat(users, type);
            lock (syncObjChats)
            {
                chatList.Add(chat.Id, chat);
            }
            OnChatCreated.Invoke(chat);
        }
        public void StopSearchingChat(int userId)
        {
            lock (syncObjUsers)
            {
                for (int i = 0; i < usersWithoutChat.Count; i++)
                {
                    if (usersWithoutChat[i].Id == userId)
                    {
                        usersWithoutChat.RemoveAt(i); 
                        return;
                    }
                }
            }
        }
        public List<int> GetUserChats(int userId)
        {
            List<int> chats = new List<int>();
            foreach(var item in chatList)
            {
                if (item.Value.Users.Contains(userId))
                {
                    chats.Add(item.Key);
                }
            }
            return chats;
        }
        public string? GetChatType(int chatId)
        {
            if(chatList.TryGetValue(chatId, out Chat? chat))
            {
                return chat.Type;
            }
            return null;
        }
        public bool IsSearchingChat(int userId)
        {
            return usersWithoutChat.Exists(user => user.Id == userId);
        }
    }
}
