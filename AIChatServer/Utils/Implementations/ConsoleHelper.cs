using AIChatServer.Utils.Interfaces;

namespace AIChatServer.Utils.Implementations
{
    public class ConsoleHelper : IConsoleHelper
    {
        public void Write(
            string text,
            ConsoleColor foreground = ConsoleColor.White,
            ConsoleColor background = ConsoleColor.Black)
        {
            var originalForeground = Console.ForegroundColor;
            var originalBackground = Console.BackgroundColor;

            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;

            Console.WriteLine(text);

            Console.ForegroundColor = originalForeground;
            Console.BackgroundColor = originalBackground;
        }
    }
}
