using AIChatServer.Entities.User;
using AIChatServer.Managers.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations.ChatStrategies.MatchStrategies
{
    public class RandomChatMatchStrategy : IStopableChatMatchStrategy
    {
        private readonly int _probabilityAIChat;
        private readonly Guid _aiId;
        private readonly Random _random = new();
        private readonly ILogger<RandomChatMatchStrategy> _logger;
        private readonly List<User> _humanChatStorage = new();
        private readonly List<(User user, CancellationTokenSource token)> _aiChatStorage = new();
        private readonly object _lock = new();
        private readonly (int min, int max) _delay;

        public RandomChatMatchStrategy(
            int probabilityAIChat,
            Guid aiId,
            (int, int) delay,
            ILogger<RandomChatMatchStrategy> logger)
        {
            if (probabilityAIChat < 0 || probabilityAIChat > 100)
                throw new ArgumentOutOfRangeException(nameof(probabilityAIChat), "Probability must be between 0 and 100.");
            _aiId = aiId;
            _delay = delay;
            _probabilityAIChat = probabilityAIChat;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IsSearching(Guid userId)
        {
            lock (_lock)
            {
                bool inHuman = _humanChatStorage.Any(u => u.Id == userId);
                bool inAi = _aiChatStorage.Any(a => a.user.Id == userId);
                return inHuman || inAi;
            }
        }

        public async Task MatchUserAsync(User user, IChatLifecycleManager chatCreator, string? predicate)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            double rnd = _random.NextDouble() * 100;

            if (rnd < _probabilityAIChat)
            {
                _logger.LogInformation("User {UserId} assigned to AI chat (rnd={Rnd:F2}).", user.Id, rnd);
                await StartAIChatAsync(user, chatCreator);
            }
            else
            {
                _logger.LogInformation("User {UserId} assigned to Human chat (rnd={Rnd:F2}).", user.Id, rnd);
                TryMatchHumanAsync(user, chatCreator);
            }
        }

        public void StopSearching(Guid userId)
        {
            lock (_lock)
            {
                var user = _humanChatStorage.FirstOrDefault(u => u.Id == userId);
                if (user != null)
                {
                    _humanChatStorage.Remove(user);
                    _logger.LogInformation("User {UserId} removed from human search queue.", userId);
                    return;
                }

                var userToken = _aiChatStorage.FirstOrDefault(a => a.user.Id == userId);
                if (userToken.user != null)
                {
                    userToken.token.Cancel();
                    _aiChatStorage.Remove(userToken);
                    _logger.LogInformation("User {UserId} canceled AI chat search.", userId);
                }
            }
        }

        private void TryMatchHumanAsync(User user, IChatLifecycleManager chatCreator)
        {
            User? partner = null;

            lock (_lock)
            {
                if (_humanChatStorage.Count > 0)
                {
                    partner = _humanChatStorage.First();
                    _humanChatStorage.RemoveAt(0);
                }
                else
                {
                    _humanChatStorage.Add(user);
                }
            }

            if (partner != null)
            {
                _logger.LogInformation("Matched {User1} with {User2} (human chat).", user.Id, partner.Id);
                chatCreator.CreateChat(new Guid[] { user.Id, partner.Id }, Entities.Chats.ChatType.Random);
            }
        }

        private async Task StartAIChatAsync(User user, IChatLifecycleManager chatCreator)
        {
            var cts = new CancellationTokenSource();
            lock (_lock)
            {
                _aiChatStorage.Add((user, cts));
            }

            try
            {
                await Task.Delay(_random.Next(_delay.min, _delay.max), cts.Token);
                chatCreator.CreateChat(new Guid[] { user.Id, _aiId }, Entities.Chats.ChatType.Random);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("AI chat creation canceled for user {UserId}.", user.Id);
            }
            finally
            {
                lock (_lock)
                {
                    _aiChatStorage.RemoveAll(a => a.user.Id == user.Id);
                }
            }
        }
    }
}
