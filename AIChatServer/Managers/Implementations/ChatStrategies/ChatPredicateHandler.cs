using AIChatServer.Entities.User;
using AIChatServer.Managers.Interfaces;
using System.Threading.Tasks;

namespace AIChatServer.Managers.Implementations.ChatMatchStrategies
{
    public abstract class ChatPredicateHandler(Dictionary<string, IUserMatchPredicate> predicates, IUserMatchPredicate defaultPredicate)
    {
        private readonly Dictionary<string, IUserMatchPredicate> _predicates = predicates ?? throw new ArgumentNullException(nameof(predicates));
        private readonly IUserMatchPredicate _defaultPredicate = defaultPredicate ?? throw new ArgumentNullException(nameof(defaultPredicate));

        protected IUserMatchPredicate GetPredicate(string? predicateName)
        {
            if (predicateName == null || !_predicates.TryGetValue(predicateName, out IUserMatchPredicate? matchPredicate)) return _defaultPredicate;
            else return matchPredicate;
        }

    }
}
