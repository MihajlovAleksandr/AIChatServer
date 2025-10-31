using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Factories.Containers
{
    public record ChatControllerContainer
    (
        IChatMatchStrategiesHandler ChatMatcher,
        IAddUserStrategiesHandler ChatUserAdder
    );
}
