using AIChatServer.Entities.Chats;
using AIChatServer.Factories.Containers;
using AIChatServer.Factories.Interfaces;
using AIChatServer.Managers.Implementations;
using AIChatServer.Managers.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Factories.Implementations
{
    public class ChatControllerFactory(ICompositeLoggerFactory loggerFactory) : IChatControllerFactory
    {
        private readonly ICompositeLoggerFactory _loggerFactory = loggerFactory ??
            throw new ArgumentNullException(nameof(loggerFactory));

        public ChatControllerContainer Create(Dictionary<ChatType, IChatMatchStrategy> chatMatchStrategies,
            Dictionary<ChatType, IAddUserStrategy> addUserStrategies)
        {
            return new ChatControllerContainer(
                new ChatMatchStrategiesHandler(chatMatchStrategies, _loggerFactory.Create<ChatMatchStrategiesHandler>()),
                new AddUserStrategiesHandler(addUserStrategies, _loggerFactory.Create<AddUserStrategiesHandler>()));
        }
    }
}
