using AIChatServer.Config.Data;
using AIChatServer.Factories.Containers;

namespace AIChatServer.Factories.Interfaces
{
    public interface IRepositoryFactory
    {
        RepositoryContainer CreateRepositories(ConfigData configData);
    }
}
