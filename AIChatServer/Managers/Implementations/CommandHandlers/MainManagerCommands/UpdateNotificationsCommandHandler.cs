using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class UpdateNotificationsCommandHandler(INotificationService notificationService) : ICommandHandler
    {
        private readonly INotificationService _notificationService = notificationService
            ?? throw new ArgumentNullException(nameof(notificationService));

        public string Operation => "UpdateNotifications";

        public async Task HandleAsync(object sender, Command command)
        {
            var knownUser = (IServerUser)sender;

            SetNotificationRequest setNotificationsRequest = command.GetData<SetNotificationRequest>() 
                ?? throw new ArgumentNullException("setNotificationsRequest");

            _notificationService.UpdateNotifications(knownUser.User.Id, setNotificationsRequest.EmailNotificationsEnabled);

            CommandResponse emailNotificationsCommand = new CommandResponse("UpdateNotifications",
                new NotificationResponse(setNotificationsRequest.EmailNotificationsEnabled));
            await knownUser.SendCommandAsync(emailNotificationsCommand);
        }
    }

}
