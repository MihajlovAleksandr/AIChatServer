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
            ArgumentNullException.ThrowIfNull(model.Chat.UsersNames);
            ArgumentNullException.ThrowIfNull(model.Chat.CreationTime);

            if (model.Chat.UsersNames.TryGetValue(model.UserId, out string? value))
                return new ChatResponse(model.Chat.Id, value, model.Chat.CreationTime, model.Chat.EndTime);
            else
                throw new KeyNotFoundException($"User with ID '{model.UserId}' not found in chat users list.");
        }
    }
}
