using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record VerificationCodeResponse(
        [property: JsonProperty("answer")] int Answer
    )
    {
        public override string ToString() => "VerificationCodeResponse {}";
    }
}
