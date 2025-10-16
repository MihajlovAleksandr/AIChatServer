using AIChatServer.Factories.Containers;

namespace AIChatServer.Factories.Interfaces
{
    public interface IMapperFactory
    {
        MapperContainer CreateMappers();
    }
}
