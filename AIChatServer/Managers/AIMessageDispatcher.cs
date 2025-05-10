using AIChatServer.Utils;

namespace AIChatServer.Entities.AI
{
    public class AIMessageDispatcher
    {
        public event Func<List<AIMessage>, Task<string>> SendMessage;
        private readonly AIMessageGroup mainMessageGroup;
        private readonly AIMessageGroup compressedMessageGroup;
        private readonly SemaphoreSlim processingSemaphore = new SemaphoreSlim(1, 1);

        public AIMessageDispatcher()
        {
            mainMessageGroup = new AIMessageGroup(8, 4);
            compressedMessageGroup = new AIMessageGroup(5, 0);
            mainMessageGroup.OnBufferOverflowing += OnGroupOverflowing;
            compressedMessageGroup.OnBufferOverflowing += OnGroupOverflowing;
        }

        public async Task SetMainMessages(List<AIMessage> messages)
        {
            await processingSemaphore.WaitAsync();
            try
            {
                mainMessageGroup.SetMessages(messages);
            }
            finally
            {
                processingSemaphore.Release();
            }
        }

        public async Task SetCompressedMessages(List<AIMessage> messages)
        {
            await processingSemaphore.WaitAsync();
            try
            {
                compressedMessageGroup.SetMessages(messages);
            }
            finally
            {
                processingSemaphore.Release();
            }
        }

        private async Task OnGroupOverflowing(List<AIMessage> messages)
        {
            try
            {
                var systemMessage = new AIMessage(-1, messages[0].ChatId, "system",
                    "Сожми следующие сообщения в краткое содержание, сохраняя ключевую информацию. " +
                    "Ответь на том же языке, что и сообщения. " +
                    "Не добавляй комментарии, только сжатое содержание.");

                messages.Insert(0, systemMessage);
                string response = await SendMessage.Invoke(messages);
                DB.DeleteAIMessages(messages);
                var compressedMessage = DB.AddAIMessage(new AIMessage(0, messages[0].ChatId, "system", response), "compressedMessage");
                    await compressedMessageGroup.AddMessage(compressedMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnMainGroupOverflowing: {ex.Message}");
                throw;
            }
        }

        public async Task<List<AIMessage>> GetContext()
        {
            await processingSemaphore.WaitAsync();
            try
            {
                var mainMessages = mainMessageGroup.GetAIMessages();
                var compressedMessages = compressedMessageGroup.GetAIMessages();
                var allMessages = new List<AIMessage>();
                allMessages.AddRange(compressedMessages);
                allMessages.AddRange(mainMessages);
                return allMessages;
            }
            finally
            {
                processingSemaphore.Release();
            }
        }

        public async Task AddMessage(AIMessage aIMessage)
        {
            await processingSemaphore.WaitAsync();
            try
            {
                await mainMessageGroup.AddMessage(aIMessage);
            }
            finally
            {
                processingSemaphore.Release();
            }
        }
    }
}