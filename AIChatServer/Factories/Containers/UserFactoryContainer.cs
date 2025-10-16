using AIChatServer.Entities.User.ServerUsers.Interfaces;

namespace AIChatServer.Factories.Containers
{
    public record UserFactoryContainer(
        IKnownUserFactory KnownUserFactory,
        IUnknownUserFactory UnknownUserFactory
    );
}
