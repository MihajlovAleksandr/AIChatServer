using AIChatServer.Entities.Chats;
using AIChatServer.Entities.User;
using AIChatServer.Repositories.Constants;
using AIChatServer.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace AIChatServer.Repositories.Implementations
{
    public class ChatRepository(string connectionString, 
        ILogger<ChatRepository> logger) : BaseRepository(connectionString), IChatRepository
    {
        private readonly ILogger<ChatRepository> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public Chat CreateChat(Guid[] users, ChatType type)
        {
            try
            {
                _logger.LogInformation("Creating new chat with type {ChatType} for {UserCount} users", type, users.Length);
                var chat = CreateChat(type);
                foreach (Guid userId in users)
                {
                    chat.UsersNames.Add(userId, AddUserToChat(userId, chat.Id));
                }
                _logger.LogInformation("Chat created with Id={ChatId} and {UserCount} users", chat.Id, users.Length);
                return chat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create chat");
                throw;
            }
        }

        public DateTime EndChat(Guid chatId)
        {

            try
            {
                _logger.LogInformation("Ending chat with ChatId={ChatId}", chatId);

                using (var connection = GetConnection())
                using (var command = new NpgsqlCommand(ChatQueries.EndChat, connection))
                {
                    command.Parameters.AddWithValue("@Id", chatId);
                    command.ExecuteNonQuery();
                }

                var endTime = GetChatEndTime(chatId);
                _logger.LogInformation("Chat with ChatId={ChatId} ended at {EndTime}", chatId, endTime);
                return endTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to end chat with ChatId={ChatId}", chatId);
                throw;
            }
        }

        public bool UpdateChatName(Guid chatId, Guid userId, string name)
        {
            try
            {
                _logger.LogInformation("Updating chat name for ChatId={ChatId} by UserId={UserId} to {Name}", chatId, userId, name);

                using (var connection = GetConnection())
                using (var command = new NpgsqlCommand(ChatQueries.UpdateChatName, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ChatId", chatId);

                    bool success = command.ExecuteNonQuery() > 0;
                    if (success)
                        _logger.LogInformation("Chat name updated successfully for ChatId={ChatId}", chatId);
                    else
                        _logger.LogWarning("Failed to update chat name for ChatId={ChatId}", chatId);

                    return success;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update chat name for ChatId={ChatId} by UserId={UserId}", chatId, userId);
                throw;
            }
        }

        public (List<Guid>, List<UserData>, List<bool>) LoadUsers(Guid chatId)
        {
            var userIds = new List<Guid>();
            var userDataList = new List<UserData>();
            var isOnlineList = new List<bool>();

            try
            {
                _logger.LogInformation("Loading users for ChatId={ChatId}", chatId);

                using (var connection = GetConnection())
                using (var command = new NpgsqlCommand(ChatQueries. LoadUsers, connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userIds.Add(reader.GetGuid("user_id"));
                            userDataList.Add(new UserData
                            {
                                Id = reader.GetGuid("user_id"),
                                Name = reader.GetString("name"),
                                Gender = (Gender)Enum.Parse(typeof(Gender), reader.GetString("gender")),
                                Age = reader.GetInt32("age")
                            });
                            isOnlineList.Add(reader.GetBoolean("is_online"));
                        }
                    }
                }

                _logger.LogInformation("Loaded {UserCount} users for ChatId={ChatId}", userIds.Count, chatId);
                return (userIds, userDataList, isOnlineList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load users for ChatId={ChatId}", chatId);
                throw;
            }
        }

        public Dictionary<Guid, Chat> GetChats()
        {
            var chats = new Dictionary<Guid, Chat>();

            try
            {
                _logger.LogInformation("Retrieving all active chats");

                using (var connection = GetConnection())
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var getChatsCommand = new NpgsqlCommand(ChatQueries.GetChats, connection, transaction))
                        using (var reader = getChatsCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var chat = new Chat
                                {
                                    Id = reader.GetGuid("id"),
                                    CreationTime = reader.GetDateTime("creation_time"),
                                    Type = (ChatType)Enum.Parse(typeof(ChatType), reader.GetString("type"), ignoreCase: true),
                                    UsersNames = new Dictionary<Guid, string>()
                                };
                                chats.Add(chat.Id, chat);
                            }
                        }

                        using (var getUserChatsCommand = new NpgsqlCommand(ChatQueries.GetUserChats, connection, transaction))
                        {
                            getUserChatsCommand.Parameters.Add("@ChatId", NpgsqlTypes.NpgsqlDbType.Uuid);

                            foreach (var chat in chats.Values)
                            {
                                getUserChatsCommand.Parameters["@ChatId"].Value = chat.Id;

                                using (var reader = getUserChatsCommand.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        chat.UsersNames.Add(reader.GetGuid("user_id"), reader.GetString("name"));
                                    }
                                }
                            }
                        }

                        transaction.Commit();
                        _logger.LogInformation("Retrieved {ChatCount} active chats", chats.Count);
                        return chats;
                    }
                    catch
                    {
                        transaction.Rollback();
                        _logger.LogError("Failed to retrieve chats, transaction rolled back");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve chats");
                throw;
            }
        }

        public bool AddChatTokenUsage(Guid chatId)
        {
            try
            {
                _logger.LogInformation("Adding token usage record for ChatId={ChatId}", chatId);
                using (var connection = GetConnection())
                using (var command = new NpgsqlCommand(ChatQueries.AddChatTokenUsage, connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    bool success = command.ExecuteNonQuery() > 0;
                    if (success)
                        _logger.LogInformation("Token usage record added for ChatId={ChatId}", chatId);
                    else
                        _logger.LogWarning("Failed to add token usage record for ChatId={ChatId}", chatId);
                    return success;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add token usage for ChatId={ChatId}", chatId);
                throw;
            }
        }

        public bool UseToken(Guid chatId, int tokenCount)
        {
            try
            {
                _logger.LogInformation("Updating token usage for ChatId={ChatId}, adding {TokenCount} tokens", chatId, tokenCount);
                using (var connection = GetConnection())
                using (var command = new NpgsqlCommand(ChatQueries.UseToken, connection))
                {
                    command.Parameters.AddWithValue("@ChatId", chatId);
                    command.Parameters.AddWithValue("@TokensCount", tokenCount);
                    bool success = command.ExecuteNonQuery() > 0;
                    if (success)
                        _logger.LogInformation("Token usage updated successfully for ChatId={ChatId}", chatId);
                    else
                        _logger.LogWarning("Failed to update token usage for ChatId={ChatId}", chatId);
                    return success;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update token usage for ChatId={ChatId}", chatId);
                throw;
            }
        }

        private Chat CreateChat(ChatType type)
        {
            try
            {
                using (var connection = GetConnection())
                using (var command = new NpgsqlCommand(ChatQueries.CreateChat, connection))
                {
                    command.Parameters.AddWithValue("@Type", type.ToString());

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var chat = new Chat
                            {
                                Id = reader.GetGuid("id"),
                                CreationTime = reader.GetDateTime("creation_time"),
                                Type = (ChatType)Enum.Parse(typeof(ChatType), reader.GetString("type"), ignoreCase: true)
                            };
                            _logger.LogInformation("Created chat with Id={ChatId}", chat.Id);
                            return chat;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create chat in DB");
                throw;
            }

            throw new Exception("Can't read chat from DB");
        }

        private string AddUserToChat(Guid userId, Guid chatId)
        {
            try
            {
                using (var connection = GetConnection())
                using (var command = new NpgsqlCommand(ChatQueries.AddUserToChat, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@ChatId", chatId);

                    var result = command.ExecuteScalar();
                    _logger.LogInformation("Added UserId={UserId} to ChatId={ChatId}", userId, chatId);
                    return result?.ToString() ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add UserId={UserId} to ChatId={ChatId}", userId, chatId);
                throw;
            }
        }
        public (List<Chat>, List<Chat>) GetNewChats(Guid userId, DateTime lastOnline)
        {
            var chats = (new List<Chat>(), new List<Chat>());

            try
            {
                _logger.LogInformation("Retrieving new chats for UserId={UserId} since {LastOnline}", userId, lastOnline);

                using (var connection = GetConnection())
                using (var command = new NpgsqlCommand(ChatQueries.GetNewChats, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@LastOnline", lastOnline);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var chat = new Chat
                            {
                                Id = reader.GetGuid("id"),
                                CreationTime = reader.GetDateTime("creation_time"),
                                EndTime = reader.IsDBNull("end_time") ? null : reader.GetDateTime("end_time"),
                                UsersNames = new Dictionary<Guid, string>() { { userId, reader.GetString("name") } }
                            };

                            if (chat.CreationTime > lastOnline)
                                chats.Item1.Add(chat);
                            else
                                chats.Item2.Add(chat);
                        }
                    }
                }

                _logger.LogInformation("Retrieved {NewChatsCount} new chats and {OldChatsCount} updated chats for UserId={UserId}",
                    chats.Item1.Count, chats.Item2.Count, userId);

                return chats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve new chats for UserId={UserId}", userId);
                throw;
            }
        }

        private DateTime GetChatEndTime(Guid chatId)
        {
            try
            {
                using (var connection = GetConnection())
                using (var command = new NpgsqlCommand(ChatQueries.GetChatEndTime, connection))
                {
                    command.Parameters.AddWithValue("@Id", chatId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var endTime = reader.GetDateTime("end_time");
                            _logger.LogDebug("Retrieved end time for ChatId={ChatId}: {EndTime}", chatId, endTime);
                            return endTime;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve end time for ChatId={ChatId}", chatId);
                throw;
            }

            throw new Exception($"Chat with ID {chatId} not found");
        }
    }
}
