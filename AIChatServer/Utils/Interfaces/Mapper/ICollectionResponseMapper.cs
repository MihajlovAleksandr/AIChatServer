namespace AIChatServer.Utils.Interfaces.Mapper
{
    public interface ICollectionResponseMapper<TResponse, TModel>
    {
        IReadOnlyCollection<TResponse> ToDTO(IEnumerable<TModel> models);
    }
}
