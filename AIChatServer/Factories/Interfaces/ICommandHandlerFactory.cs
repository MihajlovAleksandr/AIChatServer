using AIChatServer.Factories.Containers;
using AIChatServer.Managers.Interfaces;
using System.Collections.Concurrent;

namespace AIChatServer.Factories.Interfaces
{
    public interface ICommandHandlerFactory
    {
        ConcurrentDictionary<string, ICommandHandler> CreateCommandHandlers(ServiceContainer services, TokenManagerContainer tokenManagers,
                   ManagerContainer managers, MapperContainer mappers, ChatControllerContainer chatControllerContainer);
    }
}
