namespace AIChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(DB.LoginIn("oleg228@gmail.com", "Qwerty123"));
            Console.ReadKey();
        }
    }
}
