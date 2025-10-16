using AIChatServer.Entities.Connection;
using AIChatServer.Entities.Connection.Interfaces;
using AIChatServer.Entities.User.ServerUsers.Interfaces;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Entities.User.ServerUsers.Implementations
{
    public class UnknownUser : ServerUser, IUnknownUser
    {
        private readonly Dictionary<string, object> _tempData;
        private readonly IReadOnlyCollection<ICommandHandler> _commandHandlers;

        public event Action<UnknownUser> UserChanged;
        public Guid Id { get; private set; }

        public UnknownUser(Guid id, IReadOnlyCollection<ICommandHandler> commandHandlers,
            IConnection connection, IConnectionService connectionService,
            ISerializer serializer, IUserConnection userConnection,
            IConnectionNotifier connectionNotifier, ILogger<ServerUser> logger)
            : base(connection, serializer, userConnection, connectionNotifier, logger)
        {
            _commandHandlers = commandHandlers ?? throw new ArgumentNullException(nameof(commandHandlers));
            User.Id = id;
            Id = id;
            _tempData = new Dictionary<string, object>();
            GotCommand += (s, e) => GotCommandHandler(e);
            Disconnected += (s, e) => { connectionService.DeleteUnknownConnection(connection.Id); };
        }

        private async void GotCommandHandler(Command command)
        {
            ICommandHandler? handler = _commandHandlers.FirstOrDefault(c => c.Operation == command.Operation);
            if(handler!=null){
                await handler.HandleAsync(this, command);
            }
        }

        public void KnowUser()
        {
            UserChanged.Invoke(this);
        }

        public void SetUser(User user)
        {
            User = user;
        }

        protected override bool IsKnownUser()
        {
            return false;
        }

        public void StoreTempData(string key, object value)
        {
            _tempData[key] = value;
        }

        public T? GetTempData<T>(string key)
        {
            if (_tempData.TryGetValue(key, out var value) && value is T tValue)
                return tValue;
            return default;
        }
    }
}

