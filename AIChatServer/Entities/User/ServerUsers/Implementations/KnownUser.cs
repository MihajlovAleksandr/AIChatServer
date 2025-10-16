using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Utils.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Entities.User.ServerUsers.Implementations
{
    public class KnownUser : ServerUser
    {
        public KnownUser(User user, IConnection connection, ISerializer serializer,
            IUserConnection userConnection, IConnectionNotifier connectionNotifier, ILogger<ServerUser> logger) :
            base(connection, serializer, userConnection, connectionNotifier, logger)
        {
            User = user;
        }

        public KnownUser(User user, IReadOnlyCollection<IConnection> connections,
            ISerializer serializer,
            IUserConnection userConnection, IConnectionNotifier connectionNotifier, ILogger<ServerUser> logger
            ) : base(user, connections, serializer, userConnection, connectionNotifier, logger)
        {

        }

        protected override bool IsKnownUser()
        {
            return true;
        }
    }
}
