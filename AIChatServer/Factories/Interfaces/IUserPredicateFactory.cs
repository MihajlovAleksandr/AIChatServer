using AIChatServer.Factories.Containers;

namespace AIChatServer.Factories.Interfaces
{
    public interface IUserPredicateFactory
    {
        UserPredicateContainer GetPredicates();
    }
}
