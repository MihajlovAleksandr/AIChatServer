using AIChatServer.Config.Data;
using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Factories.Containers;

namespace AIChatServer.Factories.Interfaces
{
    public interface IMainManagerFactory
    {
        Task<MainManagerDependencies> Create(ConfigData configData, IConnectionFactory connectionFactory);
    }
}
