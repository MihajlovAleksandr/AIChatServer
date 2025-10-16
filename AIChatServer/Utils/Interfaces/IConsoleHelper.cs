namespace AIChatServer.Utils.Interfaces
{
    public interface IConsoleHelper
    {
        public void Write(string text, ConsoleColor foreground = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black);
    }
}
