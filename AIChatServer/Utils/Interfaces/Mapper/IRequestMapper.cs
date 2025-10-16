namespace AIChatServer.Utils.Interfaces.Mapper
{
    public interface IRequestMapper<TRequest, TModel>
    {
        TModel ToModel(TRequest request);
    }
}
