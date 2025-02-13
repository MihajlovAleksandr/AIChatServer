using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
{
    public class KnownUser : ServerUser
    {
        private User user;
        public event EventHandler<Command> CommandGot;

        public int GetUserId()
        {
            return user.id;
        }
        public KnownUser(User user)
        {
            this.user = user;
            base.CommandGot += (command) =>
            {
                CommandGot?.Invoke(user, command);
            };
        }
        public KnownUser(User user, WebSocket socket)
        {
            this.user = user;
            base.CommandGot += (command) =>
            {
                CommandGot?.Invoke(user, command);
            };
            AddConnection(socket);
        }
    }
}
