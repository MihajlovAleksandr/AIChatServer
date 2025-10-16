using Newtonsoft.Json;

namespace AIChatServer.Entities.DTO.Response
{
    public record UseOtherLoginInServiceResponse(
        [property: JsonProperty("service")] string Service
    );
}
