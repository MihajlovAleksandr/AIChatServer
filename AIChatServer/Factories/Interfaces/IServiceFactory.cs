using AIChatServer.Config.Data;
using AIChatServer.Factories.Containers;

namespace AIChatServer.Factories.Interfaces
{
    public interface IServiceFactory
    {
        ServiceContainer CreateServices(ConfigData configData, RepositoryContainer repos);
    }
    
}
