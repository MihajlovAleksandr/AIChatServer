namespace AIChatServer.Repositories.Constants
{
    public static class AIMessageQueries
    {
        public const string AddAIMessage = @"
            INSERT INTO ai_messages (chat_id, text, role, type) 
            VALUES (@ChatId, @Text, @Role, @Type)
            RETURNING id;";

        public const string GetAIMessagesByChat = @"
            SELECT id, chat_id, text, role, type 
            FROM ai_messages 
            WHERE chat_id = ANY(@ChatIds)
            ORDER BY chat_id, id ASC";

        public const string DeleteAIMessages = "DELETE FROM ai_messages WHERE id = ANY(@Ids)";
    }
}