using AIChatServer.Config.Data;
using AIChatServer.Factories.Containers;

namespace AIChatServer.Factories.Interfaces
{
    public interface IUserFactoryFactory
    {
        UserFactoryContainer CreateUserFactories(ServiceContainer services, TokenManagerContainer tokenManagers,
             MapperContainer mappers, ConfigData configData);
    }
}
