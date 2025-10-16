using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Utils.Implementations.Mappers
{
    class UserBanResponseMapper : IResponseMapper<UserBanResponse, UserBan>
    {
        public UserBanResponse ToDTO(UserBan model)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(model.ReasonCategory);
            ArgumentNullException.ThrowIfNull(model.BannedUntil);
            ArgumentNullException.ThrowIfNull(model.BannedAt);

            return new UserBanResponse(model.ReasonCategory, model.BannedAt, model.BannedUntil);
        }
    }
}
