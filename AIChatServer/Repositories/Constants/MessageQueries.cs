namespace AIChatServer.Repositories.Constants
{
    public static class MessageQueries
    {
        public const string GetNewMessages = @"
            SELECT m.*
            FROM messages m
            JOIN users_chats uc ON m.chat_id = uc.chat_id
            WHERE uc.user_id = @UserId
            AND (m.last_update > @LastOnline)
            ORDER BY m.chat_id;";

        public const string SendMessage = @"
            INSERT INTO messages (id, chat_id, user_id, text) 
            VALUES (@Id, @ChatId, @UserId, @Text)
            RETURNING id;";

        public const string GetMessageById =
            "SELECT id, chat_id, user_id, text, time, last_update FROM messages WHERE id = @Id AND deleted_status = false";

        public const string GetMessagesByChatId =
            "SELECT * FROM messages WHERE chat_id = @ChatId AND deleted_status = false";
    }
}
