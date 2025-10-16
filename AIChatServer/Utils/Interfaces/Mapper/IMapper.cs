namespace AIChatServer.Utils.Interfaces.Mapper
{
    public interface IMapper<TRequest, TModel, TResponse> : IRequestMapper<TRequest, TModel>, IResponseMapper<TResponse, TModel>
    {
    }
}
