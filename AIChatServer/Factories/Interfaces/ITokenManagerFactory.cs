using AIChatServer.Config.Data;
using AIChatServer.Factories.Containers;

namespace AIChatServer.Factories.Interfaces
{
    public interface ITokenManagerFactory
    {
        TokenManagerContainer CreateTokenManagers(ConfigData configData);
    }
}