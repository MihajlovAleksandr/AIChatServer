namespace AIChatServer
{
    public class ChatManager
    {
        private Dictionary<int,Chat> chatList;
        private readonly object syncObj = new object();
        public ChatManager()
        {
            chatList = new Dictionary<int,Chat>();
        }
        public int[] GetUsers(int user)
        {
            lock (syncObj)
            {
                return GetUsersDanger(user);
            }
        }

        public void DeleteChat(int user)
        {
            lock (syncObj)
            {
                int[] users = GetUsersDanger(user);
                foreach (int userId in users)
                {
                    chatList.Remove(userId);
                }
            }
        }
        public void AddChat(Chat chat)
        {
            lock (syncObj)
            {
                foreach (int user in chat.Users)
                {
                    chatList.Add(user, chat);
                }
            }
        }
        private int[] GetUsersDanger(int user)
        {
            return chatList[user].Users; 
        }
    }
}
