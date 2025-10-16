using AIChatServer.Utils.Implementations.Logger;

namespace AIChatServer.Managers.Interfaces
{
    public interface ICompositeLoggerFactory
    {
        CompositeLogger<T> Create<T>();
    }
}
