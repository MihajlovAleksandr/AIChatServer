using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer.Entities.Chats
{
    public class UserInChatData(string name, DateTime joinTime)
    {
        public string Name { get; set; } = name;
        public DateTime JoinTime { get; set; } = joinTime;
    }
}
