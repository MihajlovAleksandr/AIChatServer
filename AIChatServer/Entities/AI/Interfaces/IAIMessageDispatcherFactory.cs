using AIChatServer.Entities.AI.Implementations;

namespace AIChatServer.Entities.AI.Interfaces
{
    public interface IAIMessageDispatcherFactory
    {
        AIMessageDispatcher Create();
    }
}
