using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;
using AIChatServer.Utils;

namespace AIChatServer.Managers
{
    public class ChatManager
    {
        private Dictionary<int,Chat> chatList;
        private List<User> usersWithoutChat;
        public Action<Chat> OnChatCreated;
        public Action<Chat> OnChatEnded;
        private readonly object syncObjChats = new object();
        private readonly object syncObjUsers = new object();

        public ChatManager()
        {
            chatList = DB.GetChats();
            usersWithoutChat = new List<User>();
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
        public void SearchChat(User user)
        {
            //CreateChat([user]);
            //return;
            lock (syncObjUsers) {
                for (int i =0;i <usersWithoutChat.Count;i++)
                {
                    if (user.UserData.IsFits(usersWithoutChat[i].Preference))
                    {
                        if (usersWithoutChat[i].UserData.IsFits(user.Preference))
                        {
                            CreateChat([user, usersWithoutChat[i]]);  
                            return;
                        }
                    }
                }
                usersWithoutChat.Add(user);
            }
        }
        private void CreateChat(User[] users)
        {
            usersWithoutChat.Remove(users[1]);
            Chat chat = DB.CreateChat([users[0].Id, users[1].Id]);
            //Chat chat = DB.CreateChat([users[0].Id]);

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
        public bool IsSearchingChat(int userId)
        {
            return usersWithoutChat.Exists(user => user.Id == userId);
        }
    }
}
