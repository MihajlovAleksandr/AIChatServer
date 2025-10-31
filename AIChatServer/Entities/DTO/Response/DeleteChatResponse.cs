using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatServer.Entities.DTO.Response
{
    public record DeleteChatResponse
    (
        [property: JsonProperty("chatId")] Guid ChatId
    );
}
