using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Request;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;

namespace AIChatServer.Managers.Implementations.CommandHandlers.MainManagerCommands
{
    public class UpdateNotificationTokenCommandHandler(INotificationService notificationService) : ICommandHandler
    {
        private readonly INotificationService notificationService = notificationService
            ?? throw new ArgumentNullException(nameof(notificationService));

        public string Operation => "UpdateNotificationToken";

        public Task HandleAsync(object sender, Command command)
        {
            UpdateNotificationTokenRequest updateNotificationTokenRequest = command.GetData<UpdateNotificationTokenRequest>() ?? throw new ArgumentNullException("UpdateNotificationToken");
            notificationService.UpdateNotificationToken(command.Sender.Id, updateNotificationTokenRequest.NotificationToken);
            return Task.CompletedTask;
        }
    }
}
