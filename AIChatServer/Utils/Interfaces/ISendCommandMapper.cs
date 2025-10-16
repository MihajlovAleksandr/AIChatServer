using AIChatServer.Entities.DTO.Response;

namespace AIChatServer.Utils.Interfaces
{
    public interface ISendCommandMapper
    {
        Dictionary<Guid, CommandResponse> MapToSendCommand(IReadOnlyCollection<Guid> users, CommandResponse command);
    }
}
