using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Factories.Containers
{
    public record UserPredicateContainer
    (
        Dictionary<string, IUserMatchPredicate> Predicates,
        IUserMatchPredicate DefaultPredicate
    );
}
