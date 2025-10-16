using AIChatServer.Entities.Connection;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Utils.Implementations.Mappers
{
    public class ConnectionInfoResponseMapper : IResponseMapper<ConnectionInfoResponse, ConnectionInfo>
    {
        public ConnectionInfoResponse ToDTO(ConnectionInfo model)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(model.Id);
            ArgumentNullException.ThrowIfNull(model.UserId);
            ArgumentNullException.ThrowIfNull(model.Device);

            return new ConnectionInfoResponse(model.Id, model.UserId, model.Device, model.LastOnline);
        }
    }
}
