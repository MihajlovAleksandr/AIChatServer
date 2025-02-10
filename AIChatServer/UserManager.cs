using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
{
    public class UserManager
    {
        private Dictionary<int, ServerUser> users;
        private readonly object syncObj = new object();
        public UserManager()
        {
            users = new Dictionary<int, ServerUser>();
        }
        public void ConnectUser(ServerUser serverUser)
        {
            lock (syncObj)
            {
                users.Add(serverUser.GetUserId(), serverUser);
            }
        }
        public void DisconnectUser(int id)
        {
            lock (syncObj)
            {
                users.Remove(id);
            }
        }
        
    }
}
