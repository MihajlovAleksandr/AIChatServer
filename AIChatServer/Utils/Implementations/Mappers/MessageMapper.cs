using AIChatServer.Entities.Chats;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Utils.Implementations.Mappers
{
    class MessageMapper : IMapper<MessageRequest, Message, MessageResponse>
    {
        public MessageResponse ToDTO(Message model)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(model.Id);
            ArgumentNullException.ThrowIfNull(model.Chat);
            ArgumentNullException.ThrowIfNull(model.Sender);
            ArgumentNullException.ThrowIfNull(model.Text);
            ArgumentNullException.ThrowIfNull(model.Time);
            ArgumentNullException.ThrowIfNull(model.LastUpdate);

            return new MessageResponse((Guid)model.Id, model.Chat, model.Sender, model.Text, (DateTime)model.Time, (DateTime)model.LastUpdate);
        }

        public Message ToModel(MessageRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Chat);
            ArgumentNullException.ThrowIfNull(request.Sender);
            ArgumentNullException.ThrowIfNull(request.Text);

            return new Message() { Id = request.Id, Chat = request.Chat, Sender = request.Sender, Text = request.Text };
        }
    }
}
