using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record SettingsInfoResponse(
       [property: JsonProperty("email")] string Email,
       [property: JsonProperty("userData")] UserDataResponse UserData,
       [property: JsonProperty("preference")] PreferenceResponse Preference,
       [property: JsonProperty("connectionCount")] int[] ConnectionCount,
       [property: JsonProperty("notifications")] NotificationResponse Notifications
   );
}
