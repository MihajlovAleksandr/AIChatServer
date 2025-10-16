using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record EntryTokenResponse(
       [property: JsonProperty("token")] string Token
   )
    {
        public override string ToString() => "EntryTokenResponse {}";
    }
}
