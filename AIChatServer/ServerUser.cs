using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
{
    public class ServerUser
    {
        private User user;
        private List<WebSocket> webSocket;
        public ServerUser() { }
        public int GetUserId()
        {
            return user.id;
        }
    }
}
