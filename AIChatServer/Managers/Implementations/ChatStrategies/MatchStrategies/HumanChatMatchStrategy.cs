using AIChatServer.Entities.User;
using AIChatServer.Managers.Interfaces;
using Microsoft.Extensions.Logging;
using AIChatServer.Entities.Chats;
using AIChatServer.Managers.Implementations.ChatMatchStrategies;

namespace AIChatServer.Managers.Implementations.ChatStrategies.MatchStrategies
{
    public class HumanChatMatchStrategy(Dictionary<string, IUserMatchPredicate> predicates, IUserMatchPredicate defaultPredicate,
        ILogger<HumanChatMatchStrategy> logger) : ChatPredicateHandler(predicates, defaultPredicate), IStopableChatMatchStrategy
    {
        private readonly List<(User user, IUserMatchPredicate predicate)> _usersWithoutChat = new();
        private readonly object _syncObj = new();
        private readonly ILogger<HumanChatMatchStrategy> _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));

        public Task MatchUserAsync(User user, IChatLifecycleManager chatCreator, string? predicateName)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            IUserMatchPredicate predicate = GetPredicate(predicateName);

            lock (_syncObj)
            {
             
                for(int i = 0; i<_usersWithoutChat.Count;i++) {
                    var userPredicatePair = _usersWithoutChat[i];
                    User candidate = userPredicatePair.user;
                    IUserMatchPredicate candidatePredicate = userPredicatePair.predicate;
                    if (predicate.TryMatch(user, candidate) && candidatePredicate.TryMatch(candidate, user))
                    {
                        chatCreator.CreateChat(new[] { user.Id, candidate.Id }, ChatType.Human);
                        _usersWithoutChat.RemoveAt(i);
                        _logger.LogInformation(
                            "Matched user {UserId} with user {CandidateId} into a Human chat.",
                            user.Id, candidate.Id
                        );
                        return Task.CompletedTask;
                    }
                }

                _usersWithoutChat.Add((user, predicate));
                _logger.LogInformation("User {UserId} added to waiting list for Human chat.", user.Id);
            }

            return Task.CompletedTask;
        }

        public void StopSearching(Guid userId)
        {
            lock (_syncObj)
            {
                int removedCount = _usersWithoutChat.RemoveAll(u => u.user.Id == userId);
                if (removedCount > 0)
                {
                    _logger.LogInformation("User {UserId} stopped searching for a Human chat.", userId);
                }
            }
        }

        public bool IsSearching(Guid userId)
        {
            lock (_syncObj)
            {
                bool isSearching = _usersWithoutChat.Exists(u => u.user.Id == userId);
                _logger.LogDebug("Checked if user {UserId} is searching: {IsSearching}.", userId, isSearching);
                return isSearching;
            }
        }
    }
}
