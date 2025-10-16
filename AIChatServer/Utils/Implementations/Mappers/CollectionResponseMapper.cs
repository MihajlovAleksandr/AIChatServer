using AIChatServer.Utils.Interfaces.Mapper;

namespace AIChatServer.Utils.Implementations.Mappers
{
    public class CollectionResponseMapper<TResponse, TModel>(IResponseMapper<TResponse, TModel> mapper) : ICollectionResponseMapper<TResponse, TModel>
    {
        private readonly IResponseMapper<TResponse, TModel> mapper = mapper
            ?? throw new ArgumentNullException(nameof(mapper));

        public IReadOnlyCollection<TResponse> ToDTO(IEnumerable<TModel> models)
        {
            List<TResponse> responses = new List<TResponse>();

            foreach(TModel model in models)
            {
                responses.Add(mapper.ToDTO(model));
            }

            return responses;
        }
    }
}
