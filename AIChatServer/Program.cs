namespace AIChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            EmailManager emailManager = new EmailManager();
            emailManager.SendVerificationCode("", 957156);
            UserManager userManager = new UserManager();
            Console.ReadKey();
        }
    }
}
