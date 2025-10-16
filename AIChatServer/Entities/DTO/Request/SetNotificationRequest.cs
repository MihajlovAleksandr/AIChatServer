using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record SetNotificationRequest(
        [property: JsonProperty("emailNotificationsEnabled")] bool EmailNotificationsEnabled
    );
}
