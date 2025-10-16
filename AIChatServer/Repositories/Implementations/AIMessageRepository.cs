using AIChatServer.Entities.AI;
using AIChatServer.Repositories.Constants;
using AIChatServer.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AIChatServer.Repositories.Implementations
{
    public class AIMessageRepository : BaseRepository, IAIMessageRepository
    {
        private readonly ILogger<AIMessageRepository> _logger;

        public AIMessageRepository(string connectionString, ILogger<AIMessageRepository> logger)
            : base(connectionString)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public AIMessage AddAIMessage(Guid chatId, string content, string role, string type)
        {

            try
            {
                _logger.LogInformation("Adding AI message for ChatId={ChatId}, Role={Role}, Type={Type}", chatId, role, type);

                using (var connection = GetConnection())
                using (var command = new NpgsqlCommand(AIMessageQueries.AddAIMessage, connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    command.Parameters.AddWithValue("@Text", content);
                    command.Parameters.AddWithValue("@Role", role);
                    command.Parameters.AddWithValue("@Type", type);

                    var messageId = (Guid)command.ExecuteScalar();

                    _logger.LogInformation("Successfully added AI message with Id={MessageId} for ChatId={ChatId}", messageId, chatId);

                    return new AIMessage(messageId, chatId, role, content);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add AI message for ChatId={ChatId}", chatId);
                throw;
            }
        }

        public IDictionary<Guid, (IReadOnlyCollection<AIMessage>, IReadOnlyCollection<AIMessage>)> GetAIMessagesByChat(IReadOnlyCollection<Guid> chatIds)
        {
            var idMessagesPairs = new Dictionary<Guid, (IReadOnlyCollection<AIMessage>, IReadOnlyCollection<AIMessage>)>();

            if (chatIds == null || chatIds.Count == 0)
            {
                _logger.LogWarning("GetAIMessagesByChat called with empty ChatIds collection");
                return idMessagesPairs;
            }

            _logger.LogInformation("Retrieving AI messages for {ChatCount} chats", chatIds.Count);

            try
            {
                var messagesByChat = new Dictionary<Guid, (List<AIMessage> MainMessages, List<AIMessage> CompressedMessages)>();

                using (var connection = GetConnection())
                using (var command = new NpgsqlCommand(AIMessageQueries.GetAIMessagesByChat, connection))
                {
                    command.Parameters.AddWithValue("@ChatIds", chatIds.ToArray());

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var chatId = reader.GetGuid(reader.GetOrdinal("chat_id"));
                            var message = new AIMessage(
                                reader.GetGuid(reader.GetOrdinal("id")),
                                chatId,
                                reader.GetString(reader.GetOrdinal("role")),
                                reader.GetString(reader.GetOrdinal("text"))
                            );

                            if (!messagesByChat.TryGetValue(chatId, out var messageLists))
                            {
                                messageLists = (new List<AIMessage>(), new List<AIMessage>());
                                messagesByChat[chatId] = messageLists;
                            }

                            if (reader.GetString(reader.GetOrdinal("type")) == "message")
                                messageLists.MainMessages.Add(message);
                            else
                                messageLists.CompressedMessages.Add(message);
                        }
                    }
                }

                foreach (var chatId in chatIds)
                {
                    if (messagesByChat.TryGetValue(chatId, out var messages))
                        idMessagesPairs.Add(chatId, messages);
                }

                _logger.LogInformation("Retrieved AI messages for {ChatCount} chats", idMessagesPairs.Count);

                return idMessagesPairs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve AI messages for multiple chats");
                throw;
            }
        }

        public bool DeleteAIMessages(Guid[] messages)
        {
            if (messages == null || messages.Length == 0)
            {
                _logger.LogWarning("DeleteAIMessages called with empty message Ids");
                return true;
            }

            try
            {
                _logger.LogInformation("Deleting {MessageCount} AI messages", messages.Length);

                using (var connection = GetConnection())
                using (var command = new NpgsqlCommand(AIMessageQueries.DeleteAIMessages, connection))
                {
                    command.Parameters.AddWithValue("@Ids", messages);
                    int rowsAffected = command.ExecuteNonQuery();

                    _logger.LogInformation("Deleted {DeletedCount} AI messages", rowsAffected);

                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete AI messages");
                throw;
            }
        }
    }
}
