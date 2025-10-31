using AIChatServer.Config.Data;
using AIChatServer.Entities.Chats;
using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Factories.Containers;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Factories.Interfaces
{
    public interface IManagerFactory
    {
        Task<ManagerContainer> CreateManagersAsync(ConfigData configData, MapperContainer mappers,
            Dictionary<ChatType, IChatMatchStrategy> strategies, TokenManagerContainer tokenManagers,
            ServiceContainer serviceContainer, UserFactoryContainer userFactoryContainer,
            IConnectionFactory connectionFactory, ChatControllerContainer chatControllerContainer);
    }
}
