using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record AuthRequest(
        [property: JsonProperty("email")] string Email,
        [property: JsonProperty("password")] string Password
    )
    {
        public override string ToString() => $"AuthRequest email = {{{Email}}}";
    }
}
