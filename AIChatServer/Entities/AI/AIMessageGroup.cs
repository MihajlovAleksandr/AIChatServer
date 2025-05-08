using AIChatServer.Entities.AI;

namespace AIChatServer.Entities.AI
{
    public class AIMessageGroup
    {
        private readonly int maxBufferSize;
        private readonly int minBufferSize;
        private List<AIMessage> messages;
        public event Func<List<AIMessage>, Task> OnBufferOverflowing;

        public AIMessageGroup(int maxBufferSize, int minBufferSize)
        {
            if (minBufferSize >= maxBufferSize)
                throw new ArgumentException("minBufferSize must be less than maxBufferSize");

            messages = new List<AIMessage>();
            this.maxBufferSize = maxBufferSize;
            this.minBufferSize = minBufferSize;
        }

        public async Task AddMessage(AIMessage message)
        {
            messages.Add(message);

            if (messages.Count > maxBufferSize)
            {
                int messagesToRemove = messages.Count - minBufferSize;
                var overflowingMessages = messages.Take(messagesToRemove).ToList();
                messages = messages.Skip(messagesToRemove).ToList();
                await OnBufferOverflowing.Invoke(overflowingMessages);

            }
        }
        public void SetMessages(List<AIMessage> messages)
        {
            this.messages = messages;
        }

        public List<AIMessage> GetAIMessages()
        {
            return messages;
        }
    }
}