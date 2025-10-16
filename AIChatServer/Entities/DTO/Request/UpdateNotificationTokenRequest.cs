using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record UpdateNotificationTokenRequest(
         [property: JsonProperty("notificationToken")] string NotificationToken
     )
    {
        public override string ToString() => "UpdateNotificationTokenRequest {}";
    }
}
