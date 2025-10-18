using AIChatServer.Entities.Chats;
using AIChatServer.Repositories.Interfaces;
using AIChatServer.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Service.Implementations
{
    public class MessageService(IMessageRepository messageRepository, ILogger<MessageService> logger) : IMessageService
    {
        private readonly IMessageRepository _messageRepository = messageRepository
            ?? throw new ArgumentNullException(nameof(messageRepository));
        private readonly ILogger<MessageService> _logger = logger ??
            throw new ArgumentNullException(nameof(logger));

        public List<Message> GetMessagesByChatId(Guid chatId)
        {
            var messages = _messageRepository.GetMessagesByChatId(chatId);
            _logger.LogInformation("Retrieved {MessageCount} messages for Chat {ChatId}.", messages.Count, chatId);
            return messages;
        }

        public Message SendMessage(Message message)
        {
            ArgumentNullException.ThrowIfNull(message);

            if (string.IsNullOrWhiteSpace(message.Text))
            {
                _logger.LogWarning("Attempt to send empty message in Chat {ChatId} by User {UserId}.", message.Chat, message.Sender);
                throw new ArgumentException("Message text cannot be empty", nameof(message));
            }
            Guid messageId = message.Id ?? Guid.NewGuid();
            while (_messageRepository.GetMessageById(messageId) != null)
            {
                messageId = Guid.NewGuid();
            }


            var result = _messageRepository.SendMessage(messageId, message.Chat, message.Sender, message.Text);
            _logger.LogInformation("Message {MessageId} sent in Chat {ChatId} by User {UserId}.", result.Id, message.Chat, message.Sender);
            return result;
        }
        public (List<Message>, List<Message>) GetNewMessages(Guid userId, DateTime lastOnline)
        {
            var messages = _messageRepository.GetNewMessages(userId, lastOnline);
            _logger.LogInformation("Retrieved {NewMessages} new and {UpdatedMessages} updated messages for User {UserId}.", messages.Item1.Count, messages.Item2.Count, userId);
            return messages;
        }

        public Message? GetMessageById(Guid id)
        {
            ArgumentNullException.ThrowIfNull(id);
            var message = _messageRepository.GetMessageById(id);
            _logger.LogInformation("Retrieved {message} by id = {Id}", message, id);

            return message;
        }
    }
}
