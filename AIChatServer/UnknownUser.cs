using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1;

namespace AIChatServer
{
    public class UnknownUser : ServerUser
    {
        public event EventHandler<User> UserChanged;
        public UnknownUser()
        {
            CommandGot += (command) =>
            {
                if (command.Operation == "LoginIn")
                {
                    string username = command.GetData<string>("username");
                    string password = command.GetData<string>("password");
                    User user = DB.LoginIn(username, password);
                    if (user != null)
                    {
                        DeleteSocketFromList(command.Sender);
                        UserChanged.Invoke(command.Sender, user);
                    }
                }
                else if(command.Operation == "Registration")
                {

                }
            };
        }
    }
}
