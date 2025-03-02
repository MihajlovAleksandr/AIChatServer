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
        public KnownUser(User user, Connection connection)
        {
            this.user = user;
            AddConnection(connection);
            base.GotCommand += OnGotCommand;
            Disconnected += OnDisconnect;
        }
        private void OnGotCommand(object sender, Command command)
        {
            CommandGot.Invoke(sender, command);
        }
        private void OnDisconnect(object seder, EventArgs e)
        {

        }
    }
}
