using AIChatServer.Entities.User;
using AIChatServer.Managers.Interfaces;
using Microsoft.Extensions.Logging;
using AIChatServer.Entities.Chats;

namespace AIChatServer.Managers.Implementations
{
    public class HumanChatMatchStrategy(ILogger<HumanChatMatchStrategy> logger) : IStopableChatMatchStrategy
    {
        private readonly List<User> _usersWithoutChat = new();
        private readonly object _syncObj = new();
        private readonly ILogger<HumanChatMatchStrategy> _logger = logger 
            ?? throw new ArgumentNullException(nameof(logger));

        public Task MatchUserAsync(User user, IChatCreator chatCreator, ChatType? chatType)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (chatCreator == null) throw new ArgumentNullException(nameof(chatCreator));
            ChatType currentChatType = chatType ?? ChatType.Human;

            lock (_syncObj)
            {
                for (int i = 0; i < _usersWithoutChat.Count; i++)
                {
                    var candidate = _usersWithoutChat[i];
                    if (user.UserData.IsFits(candidate.Preference) &&
                        candidate.UserData.IsFits(user.Preference))
                    {
                        chatCreator.CreateChat(new[] { user.Id, candidate.Id }, currentChatType);
                        _usersWithoutChat.RemoveAt(i);
                        _logger.LogInformation(
                            "Matched user {UserId} with user {CandidateId} into a Human chat.",
                            user.Id, candidate.Id
                        );
                        return Task.CompletedTask;
                    }
                }

                _usersWithoutChat.Add(user);
                _logger.LogInformation("User {UserId} added to waiting list for Human chat.", user.Id);
            }

            return Task.CompletedTask;
        }

        public void StopSearching(Guid userId)
        {
            lock (_syncObj)
            {
                int removedCount = _usersWithoutChat.RemoveAll(u => u.Id == userId);
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
                bool isSearching = _usersWithoutChat.Exists(u => u.Id == userId);
                _logger.LogDebug("Checked if user {UserId} is searching: {IsSearching}.", userId, isSearching);
                return isSearching;
            }
        }
    }
}
