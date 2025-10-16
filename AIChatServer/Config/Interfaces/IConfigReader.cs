namespace AIChatServer.Config.Interfaces
{
    public interface IConfigReader
    {
        string? GetDataFromPath(string path);
    }
}
