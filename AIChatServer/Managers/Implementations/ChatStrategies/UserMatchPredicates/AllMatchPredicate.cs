using AIChatServer.Entities.User;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers.Implementations.ChatMatchStrategies.UserMatchPredicates
{
    public class AllMatchPredicate : IUserMatchPredicate
    {
        public string Predicate => "AllMatch";

        public bool TryMatch(User first, User second)
        {
            return true;
        }
    }
}
