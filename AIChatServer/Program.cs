using System.Linq.Expressions;

namespace AIChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(DB.HashPassword("Qwerty123#"));
            Console.WriteLine(DB.LoginIn("bolnoystas@gmail.com", "Qwerty123"));
            UserManager userManager = new UserManager();
            Console.ReadLine();
            //Console.ReadLine();
        }
    }
}
