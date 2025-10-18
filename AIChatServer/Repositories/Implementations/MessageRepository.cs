using AIChatServer.Entities.Chats;
using AIChatServer.Repositories.Constants;
using AIChatServer.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace AIChatServer.Repositories.Implementations
{
    public class MessageRepository(string connectionString, ILogger<MessageRepository> logger) : BaseRepository(connectionString), IMessageRepository
    {
        private readonly ILogger<MessageRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger)); 

        public List<Message> GetMessagesByChatId(Guid chatId)
        {
            var messages = new List<Message>();

            try
            {
                _logger.LogInformation("Retrieving messages for ChatId={ChatId}", chatId);

                using (var connection = GetConnection())
                using (var command = new NpgsqlCommand(MessageQueries.GetMessagesByChatId, connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            messages.Add(new Message
                            {
                                Id = reader.GetGuid("id"),
                                Chat = reader.GetGuid("chat_id"),
                                Sender = reader.GetGuid("user_id"),
                                Text = reader.GetString("text"),
                                Time = reader.GetDateTime("time"),
                                LastUpdate = reader.GetDateTime("last_update")
                            });
                        }
                    }
                }

                _logger.LogInformation("Retrieved {MessageCount} messages for ChatId={ChatId}", messages.Count, chatId);
                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve messages for ChatId={ChatId}", chatId);
                throw;
            }
        }

        public Message SendMessage(Guid id, Guid chatId, Guid userId, string text)
        {

            try
            {
                _logger.LogInformation("Sending message with Id = {Id} for ChatId={ChatId} by UserId={UserId}", id, chatId, userId);

                using (var connection = GetConnection())
                using (var command = new NpgsqlCommand(MessageQueries.SendMessage, connection))
                {
                    command.Parameters.AddWithValue("Id", id);
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Text", text);

                    var messageId = (Guid?)command.ExecuteScalar();
                    ArgumentNullException.ThrowIfNull(messageId);

                    var message = GetMessageById(id);
                    ArgumentNullException.ThrowIfNull(message);

                    _logger.LogInformation("Message sent with Id={MessageId} for ChatId={ChatId}", id, chatId);
                    return message;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send message for ChatId={ChatId} by UserId={UserId}", chatId, userId);
                throw;
            }
        }
        public (List<Message>, List<Message>) GetNewMessages(Guid userId, DateTime lastOnline)
        {
            var messages = (new List<Message>(), new List<Message>());

            try
            {
                _logger.LogInformation("Retrieving new messages for UserId={UserId} since {LastOnline}", userId, lastOnline);

                using (var connection = GetConnection())
                using (var command = new NpgsqlCommand(MessageQueries.GetNewMessages, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@LastOnline", lastOnline);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var message = new Message
                            {
                                Id = reader.GetGuid("id"),
                                Chat = reader.GetGuid("chat_id"),
                                Sender = reader.GetGuid("user_id"),
                                Text = reader.GetString("text"),
                                Time = reader.GetDateTime("time"),
                                LastUpdate = reader.GetDateTime("last_update")
                            };

                            if (message.Time > lastOnline)
                                messages.Item1.Add(message);
                            else
                                messages.Item2.Add(message);
                        }
                    }
                }

                _logger.LogInformation("Retrieved {NewMessagesCount} new messages and {OldMessagesCount} updated messages for UserId={UserId}",
                    messages.Item1.Count, messages.Item2.Count, userId);

                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve new messages for UserId={UserId}", userId);
                throw;
            }
        }
        public Message? GetMessageById(Guid messageId)
        {
            try
            {
                using (var connection = GetConnection())
                using (var command = new NpgsqlCommand(MessageQueries.GetMessageById, connection))
                {
                    command.Parameters.AddWithValue("@Id", messageId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var message = new Message
                            {
                                Id = reader.GetGuid("id"),
                                Chat = reader.GetGuid("chat_id"),
                                Sender = reader.GetGuid("user_id"),
                                Text = reader.GetString("text"),
                                Time = reader.GetDateTime("time"),
                                LastUpdate = reader.GetDateTime("last_update")
                            };
                            _logger.LogDebug("Retrieved message Id={MessageId}", messageId);
                            return message;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve message Id={MessageId}", messageId);
                throw;
            }

            return null;
        }
    }
}
