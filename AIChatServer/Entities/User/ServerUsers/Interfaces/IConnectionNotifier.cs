using AIChatServer.Entities.Connection.Interfaces;

namespace AIChatServer.Entities.User.ServerUsers.Interfaces
{
    public interface IConnectionNotifier : IConnectionEventHandler
    {
        void HandleConnectionAdded(IConnection connection, User user, bool isKnownUser);
        public void HandleConnectionRemoved(IConnection connection, User user);
    }
}
