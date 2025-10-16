using AIChatServer.Entities.AI;
using AIChatServer.Integrations.AI.DTO;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Utils.Implementations.Mappers
{
    public class AIMessageResponseMapper : IResponseMapper<AIMessageRequest, AIMessage>
    {
        public AIMessageRequest ToDTO(AIMessage model)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(model.Content);
            ArgumentNullException.ThrowIfNull(model.Role);

            return new AIMessageRequest(model.Content, model.Role);
        }
    }
}
