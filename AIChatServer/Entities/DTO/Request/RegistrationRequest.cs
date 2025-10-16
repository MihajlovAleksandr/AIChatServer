using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record RegistrationRequest(
        [property: JsonProperty("email")] string Email,
        [property: JsonProperty("password")] string Password,
        [property: JsonProperty("localization")] string Localization
    )
    {
        public override string ToString() => $"RegistrationRequest email = {{{Email}}}, localization = {{{Localization}}}";
    }
}
