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
            TokenManager tokenManager = new TokenManager();
            string token = tokenManager.GenerateToken(96453);
            tokenManager.DecodeToken(token);
            Console.WriteLine(tokenManager.ValidateTokenInternal(token, out int userId));
            Console.WriteLine(userId);
        }
    }

}