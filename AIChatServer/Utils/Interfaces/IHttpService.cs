namespace AIChatServer.Utils.Interfaces
{
    public interface IHttpService
    {
        Task<string?> PostAsync(string url, string jsonBody, Dictionary<string, string> headers);
    }
}
