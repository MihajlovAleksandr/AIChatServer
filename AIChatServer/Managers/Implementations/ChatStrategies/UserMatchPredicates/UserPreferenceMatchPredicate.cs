using AIChatServer.Entities.User;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.ChatMatchStrategies.UserMatchPredicates
{
    public class UserPreferenceMatchPredicate : IUserMatchPredicate
    {
        public string Predicate => "UserPreferenceMatch";

        public bool TryMatch(User main, User other)
        {
            return main.UserData.IsFits(other.Preference);
        }
    }
}
