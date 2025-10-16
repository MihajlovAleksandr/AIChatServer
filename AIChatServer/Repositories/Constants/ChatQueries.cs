namespace AIChatServer.Repositories.Constants
{
    public static class ChatQueries
    {
        public const string GetMessagesByChatId =
            "SELECT * FROM messages WHERE chat_id = @ChatId AND deleted_status = false";

        public const string SendMessage = @"
            INSERT INTO messages (chat_id, user_id, text) 
            VALUES (@ChatId, @UserId, @Text)
            RETURNING id;";

        public const string EndChat =
            "UPDATE chats SET end_time = CURRENT_TIMESTAMP WHERE id = @Id";

        public const string UpdateChatName = @"
            UPDATE users_chats
            SET name = @Name
            WHERE user_id = @UserId AND chat_id = @ChatId";

        public const string GetNewChats = @"
            SELECT c.id, c.creation_time, c.end_time, uc.name
            FROM chats c
            JOIN users_chats uc ON c.id = uc.chat_id
            WHERE uc.user_id = @UserId 
            AND (uc.last_update > @LastOnline OR 
                 (c.end_time IS NOT NULL AND c.end_time > @LastOnline));";

        public const string GetNewMessages = @"
            SELECT m.*
            FROM messages m
            JOIN users_chats uc ON m.chat_id = uc.chat_id
            WHERE uc.user_id = @UserId
            AND (m.last_update > @LastOnline)
            ORDER BY m.chat_id;";

        public const string LoadUsers = @"
            SELECT 
                ud.user_id,
                ud.name,
                ud.gender,
                ud.age,
                CASE WHEN EXISTS (
                    SELECT 1 FROM connections 
                    WHERE user_id = ud.user_id AND last_connection IS NULL
                ) THEN true ELSE false END AS is_online
            FROM user_data ud
            WHERE ud.user_id IN (
                SELECT user_id FROM users_chats WHERE chat_id = @ChatId
            )";

        public const string GetChats =
            "SELECT * FROM chats WHERE end_time IS NULL AND deleted_status = false";

        public const string GetUserChats =
            "SELECT user_id, name FROM users_chats WHERE chat_id = @ChatId";

        public const string AddChatTokenUsage =
            "INSERT INTO chat_token_usage (chat_id, tokens_count) VALUES (@ChatId, 0)";

        public const string UseToken = @"
            UPDATE chat_token_usage
            SET tokens_count = tokens_count + @TokensCount
            WHERE chat_id = @ChatId";

        public const string CreateChat =
            "INSERT INTO chats (type) VALUES (@Type) RETURNING *;";

        public const string AddUserToChat =
            "INSERT INTO users_chats (user_id, chat_id, name) VALUES (@UserId, @ChatId, '') RETURNING name;";

        public const string GetMessageById =
            "SELECT id, chat_id, user_id, text, time, last_update FROM messages WHERE id = @Id AND deleted_status = false";

        public const string GetChatEndTime =
            "SELECT end_time FROM chats WHERE id = @Id";
    }
}