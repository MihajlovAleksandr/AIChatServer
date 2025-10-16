using AIChatServer.Entities.DTO.Response;
using AIChatServer.Utils.Interfaces;

namespace AIChatServer.Utils.Implementations
{
    public class SendCommandMapper : ISendCommandMapper
    {
        public Dictionary<Guid, CommandResponse> MapToSendCommand(IReadOnlyCollection<Guid> users, CommandResponse command)
        {
            return users.ToDictionary(userId => userId, userId => command);
        }
    }
}
