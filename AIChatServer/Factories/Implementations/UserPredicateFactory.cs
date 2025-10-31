using AIChatServer.Factories.Containers;
using AIChatServer.Factories.Interfaces;
using AIChatServer.Managers.Implementations.ChatMatchStrategies.UserMatchPredicates;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Factories.Implementations
{
    public class UserPredicateFactory : IUserPredicateFactory
    {
        public UserPredicateContainer GetPredicates()
        {
            var allMatchPredicate = new AllMatchPredicate();
            return new UserPredicateContainer(new Dictionary<string, IUserMatchPredicate>
            {
                { "AllMatch",  allMatchPredicate},
                { "UserPreferenceMatch", new UserPreferenceMatchPredicate() }
            }, allMatchPredicate);
        }
    }
}
