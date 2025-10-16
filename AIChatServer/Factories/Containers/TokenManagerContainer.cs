using AIChatServer.Utils.Interfaces;

namespace AIChatServer.Factories.Containers
{
    public record TokenManagerContainer
    (
        IConnectionTokenManager ConnectionTokenManager,
        IEntryTokenManager EntryTokenManager
    );
}
