using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record ChangePasswordRequest(
        [property: JsonProperty("currentPassword")] string CurrentPassword,
        [property: JsonProperty("newPassword")] string NewPassword
    )
    {
        public override string ToString() => "ChangePasswordRequest {}";
    }
}
