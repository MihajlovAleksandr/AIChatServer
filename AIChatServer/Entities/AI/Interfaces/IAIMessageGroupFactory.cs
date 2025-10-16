using AIChatServer.Entities.AI.Implementations;

namespace AIChatServer.Entities.AI.Interfaces
{
    public interface IAIMessageGroupFactory
    {
        AIMessageGroup Create((int, int) messageGroupSize);
    }
}
