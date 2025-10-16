using AIChatServer.Factories.Containers;
using AIChatServer.Factories.Interfaces;
using AIChatServer.Utils.Implementations;
using AIChatServer.Utils.Implementations.Mappers;

namespace AIChatServer.Factories.Implementations
{
    public class MapperFactory : IMapperFactory
    {
        public MapperContainer CreateMappers()
        {
            var sendCommandMapper = new SendCommandMapper();
            var aiMessageMapper = new AIMessageResponseMapper();
            var connectionInfoMapper = new ConnectionInfoResponseMapper();
            var chatResponseMapper = new ChatResponseMapper();
            var userBanResponseMapper = new UserBanResponseMapper();
            var messageMapper = new MessageMapper();
            var userDataMapper = new UserDataMapper();
            var preferenceMapper = new PreferenceMapper();
            var preferenceRequestMapper = new PreferenceMapper();
            var userDataRequestMapper = new UserDataMapper();

            return new MapperContainer(
                sendCommandMapper,
                aiMessageMapper,
                connectionInfoMapper,
                chatResponseMapper,
                userBanResponseMapper,
                messageMapper,
                userDataMapper,
                preferenceMapper,
                preferenceRequestMapper,
                userDataRequestMapper
            );
        }
    }
}
