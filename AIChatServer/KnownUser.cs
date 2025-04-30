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
        public event EventHandler<Command> CommandGot;
        public KnownUser(User user, Connection connection): base(connection)
        {
            User = user;
            GotCommand += OnGotCommand;
        }
        public KnownUser(UnknownUser unknownUser): base(unknownUser)
        {
            GotCommand += OnGotCommand;
        }

        private void OnGotCommand(object sender, Command command)
        {
            CommandGot.Invoke(this, command);
        }
    }
}
