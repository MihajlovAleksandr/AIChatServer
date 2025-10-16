using AIChatServer.Entities.Chats;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Managers;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils.Interfaces;
using AIChatServer.Utils.Interfaces.Mapper;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace AIChatServer.Factories.Containers
{
    public record MainManagerDependencies(
        IChatManager ChatManager,
        IUserManager UserManager,
        IAIManager AIManager,
        INotificationManager NotificationManager,
        IChatService ChatService,
        IUserService UserService,
        INotificationService NotificationService,
        ISendCommandMapper SendCommandMapper,
        IResponseMapper<ChatResponse, ChatWithUserContext> ChatResponseMapper,
        IMapper<MessageRequest, Message, MessageResponse> MessageMapper,
        ConcurrentDictionary<string, ICommandHandler> CommandHandlers,
        ILogger<MainManager> Logger,
        IUserEvents UserEvents,
        IConnectionListener ConnectionListener
    );
}