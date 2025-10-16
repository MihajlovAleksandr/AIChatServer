using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Utils.Implementations.Mappers
{
    public class UserDataMapper : IMapper<UserDataRequest, UserData, UserDataResponse>
    {
        public UserDataResponse ToDTO(UserData model)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(model.Name);
            ArgumentNullException.ThrowIfNull(model.Age);
            ArgumentNullException.ThrowIfNull(model.Gender);

            return new UserDataResponse(model.Name, model.Age, model.Gender);
        }

        public UserData ToModel(UserDataRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Name);
            ArgumentNullException.ThrowIfNull(request.Age);
            ArgumentNullException.ThrowIfNull(request.Gender);

            return new UserData() { Name = request.Name, Age = request.Age, Gender = request.Gender };
        }
    }
}
