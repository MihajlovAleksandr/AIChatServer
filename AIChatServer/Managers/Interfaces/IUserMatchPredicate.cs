using AIChatServer.Entities.User;

namespace AIChatServer.Managers.Interfaces
{
    public interface IUserMatchPredicate
    {
        string Predicate { get; }
        bool TryMatch(User main, User other);
    }
}
