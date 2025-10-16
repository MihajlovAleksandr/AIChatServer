using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record NotificationResponse(
        [property: JsonProperty("emailNotificationsEnabled")] bool EmailNotificationsEnabled
    );
}
