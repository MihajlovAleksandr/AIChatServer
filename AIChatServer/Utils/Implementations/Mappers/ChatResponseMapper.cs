using AIChatServer.Entities.Chats;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Utils.Implementations.Mappers
{
    public class ChatResponseMapper : IResponseMapper<ChatResponse, ChatWithUserContext>
    {
        public ChatResponse ToDTO(ChatWithUserContext? model)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(model.Chat);
            ArgumentNullException.ThrowIfNull(model.UserId);
            ArgumentNullException.ThrowIfNull(model.Chat.Id);
            ArgumentNullException.ThrowIfNull(model.Chat.UsersWithData);
            ArgumentNullException.ThrowIfNull(model.Chat.CreationTime);

            if (model.Chat.UsersWithData.TryGetValue(model.UserId, out UserInChatData? value))
                return new ChatResponse(model.Chat.Id, value.Name, value.JoinTime, model.Chat.EndTime);
            else
                throw new KeyNotFoundException($"User with ID '{model.UserId}' not found in chat users list.");
        }
    }
}
