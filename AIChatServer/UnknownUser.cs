using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer
{
    public class UnknownUser : ServerUser
    {
        public event EventHandler<EventArgs> UserChanged;
        public UnknownUser()
        {
            CommandGot += (command) =>
            {
                if (command.Operation == "LoginIn")
                {
                    
                }
                else if(command.Operation == "Registration")
                {

                }
            };
        }
    }
}
