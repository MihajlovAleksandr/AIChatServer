using AIChatServer.Entities.AI;
using AIChatServer.Entities.Chats;
using AIChatServer.Utils;
using AIChatServer.Utils.AI;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Operators;
using System.Collections.Specialized;
using System.Text;

namespace AIChatServer.Managers
{
    public class AIManager
    {
        private readonly IAIController aiController;
        private readonly int aIId;
        private readonly Dictionary<int, AIMessageDispatcher> dialogs;
        public event Action<Message> OnSendMessage;
        public int AIId
        {
            get { return aIId; }
        }

        public AIManager(int aiId, IAIController aiController, Dictionary<int, AIMessageDispatcher> dialogs)
        {
            this.dialogs = dialogs;
            this.aiController = aiController;
            foreach(KeyValuePair<int, AIMessageDispatcher> pair in this.dialogs)
            {
                pair.Value.SendMessage += SendMessageAsync;
            }
            this.aIId = aiId;
        }

        public void CreateDialog(int chatId)
        {
            if (!dialogs.ContainsKey(chatId))
            {
                AIMessageDispatcher dispatcher = new AIMessageDispatcher();
                dispatcher.SendMessage += SendMessageAsync;
                dialogs.Add(chatId, dispatcher);
                DB.AddChatTokenUsage(chatId);
            }
        }

        public void EndDialog(int chatId)
        {
            if(dialogs.TryGetValue(chatId, out AIMessageDispatcher? dispatcher))
            {
                dispatcher.SendMessage -= SendMessageAsync;
                dialogs.Remove(chatId);
            }
        } 

        public async Task SendMessageAsync(int chatId, string message)
        {
            AIMessage messageFromUser = DB.AddAIMessage(new AIMessage(0, chatId, "user", message),"message");
            await dialogs[chatId].AddMessage(messageFromUser);
            List<AIMessage> messages = await dialogs[chatId].GetContext();
            messages.Insert(0, new AIMessage(0, chatId, "system", "Ты - человек ведущий непринужденную беседу. Твоя задача не раскрыть, что ты программа. Отвечай дружелюбно и небольшим текстом, без смайликов и других лишних символов"));
            AIMessage responseFromAI = DB.AddAIMessage(new AIMessage(0, chatId, "assistant", await SendMessageAsync(messages)), "message");
            await dialogs[chatId].AddMessage(responseFromAI);
            OnSendMessage.Invoke(new Message() { Chat = responseFromAI.ChatId, Text = responseFromAI.Content, Sender = aIId });
        }
        private async Task<string> SendMessageAsync(List<AIMessage> aiMessages)
        {
            AIResponseInfo info = await SendMessageWithRetryAsync(aiMessages);
            DB.UseToken(aiMessages[0].ChatId, info.TotalTokensUsed);
            return info.Answer;
        }
        private async Task<AIResponseInfo> SendMessageWithRetryAsync(List<AIMessage> messages)
        {
            AIResponseInfo? response;
            do
            {
                response = await aiController.SendMessageAsync(messages);
            } while (response == null);

            return response;
        }
    }
}