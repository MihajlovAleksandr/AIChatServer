using System;
using System.Collections.Generic;
using System.Linq;
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
            base.CommandGot += (command) =>
            {
                CommandGot?.Invoke(user, command);
            };
        }

    }
}
