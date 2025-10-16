using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Request
{
    public record VerificationCodeRequest(
        [property: JsonProperty("code")] int Code
    )
    {
        public override string ToString() => "VerificationCodeRequest {}";
    }
}
