using AIChatServer.Entities.AI;
using AIChatServer.Entities.Chats;
using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User;
using AIChatServer.Integrations.AI.DTO;
using AIChatServer.Utils.Interfaces.Mapper;
using AIChatServer.Utils.Interfaces;

namespace AIChatServer.Factories.Containers
{
    public record MapperContainer(
        ISendCommandMapper SendCommandMapper,
        IResponseMapper<AIMessageRequest, AIMessage> AIMessageMapper,
        IResponseMapper<ConnectionInfoResponse, ConnectionInfo> ConnectionInfoMapper,
        IResponseMapper<ChatResponse, ChatWithUserContext> ChatResponseMapper,
        IResponseMapper<UserBanResponse, UserBan> UserBanResponseMapper,
        IMapper<MessageRequest, Message, MessageResponse> MessageMapper,
        IMapper<UserDataRequest, UserData, UserDataResponse> UserDataMapper,
        IMapper<PreferenceRequest, Preference, PreferenceResponse> PreferenceMapper,
        IRequestMapper<PreferenceRequest, Preference> PreferenceRequestMapper,
        IRequestMapper<UserDataRequest, UserData> UserDataRequestMapper
    );
}
