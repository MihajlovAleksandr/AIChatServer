using AIChatServer.Entities.DTO.Response;

namespace AIChatServer.Entities.User.ServerUsers.Interfaces
{
    public interface IUnknownUser
    {
        Guid Id { get; }
        User User { get; }
        void SetUser(User user);
        void KnowUser();
        Task SendCommandAsync(CommandResponse command);
        void StoreTempData(string key, object value);
        T? GetTempData<T>(string key);
    }
}
