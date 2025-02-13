using System.Net;
using System.Text;
using System.Net.WebSockets;

namespace AIChatServer
{
    using System;
    using System.Net;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    class Server
    {
        public static void Main()
        {
            UserManager userManager = new UserManager();
            Console.ReadKey();
        }
    }

}