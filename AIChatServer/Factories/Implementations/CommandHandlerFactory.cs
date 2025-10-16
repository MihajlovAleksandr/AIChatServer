using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Factories.Containers;
using AIChatServer.Factories.Interfaces;
using AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Utils.Implementations.Mappers;
using AIChatServer.Utils.Interfaces;
using System.Collections.Concurrent;

namespace AIChatServer.Factories.Implementations
{
    public class CommandHandlerFactory(ISerializer serializer) : ICommandHandlerFactory
    {
        private readonly ISerializer _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

        public ConcurrentDictionary<string, ICommandHandler> CreateCommandHandlers(ServiceContainer services, TokenManagerContainer tokenManagers,
            ManagerContainer managers, MapperContainer mappers)
        {
            var handlers = new ConcurrentDictionary<string, ICommandHandler>();

            handlers.TryAdd("SendMessage", new SendMessageCommandHandler(services.ChatService, managers.ChatManager, managers.UserManager,
                managers.AIManager,
                services.NotificationService, managers.NotificationManager, _serializer, mappers.MessageMapper, mappers.SendCommandMapper));

            handlers.TryAdd("SearchChat", new SearchChatCommandHandler(managers.ChatManager));
            handlers.TryAdd("EndChat", new EndChatCommandHandler(managers.ChatManager));
            handlers.TryAdd("StopSearchingChat", new StopSearchingChatCommandHandler(managers.ChatManager));
            handlers.TryAdd("SyncDB", new SyncDBCommandHandler(managers.SyncService, _serializer));
            handlers.TryAdd("LoadUsersInChat", new LoadUsersInChatCommandHandler(services.ChatService, managers.ChatManager,
                managers.AIManager, _serializer, mappers.UserDataMapper));
            handlers.TryAdd("GetSettingsInfo", new GetSettingsInfoCommandHandler(services.ConnectionService, services.NotificationService,
               _serializer, mappers.UserDataMapper, mappers.PreferenceMapper));
            handlers.TryAdd("UpdateUserData", new UpdateUserDataCommandHandler(services.UserService, mappers.UserDataMapper));
            handlers.TryAdd("UpdatePreference", new UpdatePreferenceCommandHandler(services.UserService, mappers.PreferenceMapper));
            handlers.TryAdd("DeleteConnection", new DeleteConnectionCommandHandler(services.ConnectionService,
                managers.UserManager, mappers.ConnectionInfoMapper));
            handlers.TryAdd("ChangePassword", new ChangePasswordCommandHandler(services.AuthService, _serializer));
            handlers.TryAdd("GetDevices", new GetDevicesCommandHandler(services.ConnectionService,
                _serializer, new CollectionResponseMapper<ConnectionInfoResponse, ConnectionInfo>(mappers.ConnectionInfoMapper)));
            handlers.TryAdd("EntryTokenRead", new EntryTokenReadCommandHandler(tokenManagers.EntryTokenManager, managers.UserManager));
            handlers.TryAdd("UpdateNotifications", new UpdateNotificationsCommandHandler(services.NotificationService));
            handlers.TryAdd("UpdateChatName", new UpdateChatNameCommandHandler(services.ChatService));
            handlers.TryAdd("UpdateNotificationToken", new UpdateNotificationTokenCommandHandler(services.NotificationService));

            return handlers;
        }
    }
}
