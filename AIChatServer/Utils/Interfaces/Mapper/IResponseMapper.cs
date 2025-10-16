namespace AIChatServer.Utils.Interfaces.Mapper
{
    public interface IResponseMapper <TResponse, TModel>
    {
        TResponse ToDTO(TModel model);
    }
}
