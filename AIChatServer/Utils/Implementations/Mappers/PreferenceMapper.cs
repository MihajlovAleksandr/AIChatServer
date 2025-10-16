using AIChatServer.Entities.DTO.Request;
using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User;
using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Utils.Implementations.Mappers
{
    class PreferenceMapper : IMapper<PreferenceRequest, Preference, PreferenceResponse>
    {
        public PreferenceResponse ToDTO(Preference model)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(model.MinAge);
            ArgumentNullException.ThrowIfNull(model.MaxAge);
            ArgumentNullException.ThrowIfNull(model.Gender);

            return new PreferenceResponse(model.MinAge, model.MaxAge, model.Gender);
        }

        public Preference ToModel(PreferenceRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.MaxAge);
            ArgumentNullException.ThrowIfNull(request.MinAge);
            ArgumentNullException.ThrowIfNull(request.Gender);

            return new Preference() { MaxAge = request.MaxAge, MinAge = request.MinAge, Gender = request.Gender };
        }
    }
}
