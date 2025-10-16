using AIChatServer.Entities.User;
using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record UserBanResponse(
        [property: JsonProperty("reason_category")] BanReason ReasonCategory,
        [property: JsonProperty("banned_at")] DateTime BannedAt,
        [property: JsonProperty("banned_until")] DateTime BannedUntil
    );
}
