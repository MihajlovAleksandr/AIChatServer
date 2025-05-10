using AIChatServer.Entities.Connection;

namespace AIChatServer.Entities.User.ServerUsers
{
    public class KnownUser : ServerUser
    {
        public event EventHandler<Command> CommandGot;
        public KnownUser(User user, Connection.Connection connection): base(connection)
        {
            User = user;
            GotCommand += OnGotCommand;
        }
        public KnownUser(UnknownUser unknownUser): base(unknownUser)
        {
            GotCommand += OnGotCommand;
        }

        private void OnGotCommand(object sender, Command command)
        {
            CommandGot.Invoke(this, command);
        }
    }
}
