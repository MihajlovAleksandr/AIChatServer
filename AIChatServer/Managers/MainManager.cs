using AIChatServer.Managers.Implementations;
using AIChatServer.Managers.Interfaces;

namespace AIChatServer.Managers
{
    public class MainManager
    {
        private readonly IConnectionListener _connectionListener;
        private readonly IUserEvents _userEvents;

        private readonly IEventHandler _connectionEventHandler;
        private readonly IEventHandler _commandEventHandler;
        private readonly IEventHandler _chatEventHandler;
        private readonly IEventHandler _aiMessageEventHandler;

        public MainManager(
            IConnectionListener connectionListener,
            IUserEvents userEvents,
            ConnectionEventHandler connectionEventHandler,
            CommandEventHandler commandEventHandler,
            ChatEventHandler chatEventHandler,
            AIMessageEventHandler aiMessageEventHandler)
        {
            _connectionListener = connectionListener ?? throw new ArgumentNullException(nameof(connectionListener));
            _userEvents = userEvents ?? throw new ArgumentNullException(nameof(userEvents));

            _connectionEventHandler = connectionEventHandler;
            _commandEventHandler = commandEventHandler;
            _chatEventHandler = chatEventHandler;
            _aiMessageEventHandler = aiMessageEventHandler;

            SubscribeToEvents();
            _connectionListener.Start();
        }

        private void SubscribeToEvents()
        {
            _userEvents.ConnectionChanged += (s, e) => _connectionEventHandler.HandleAsync(s, e);
            _userEvents.CommandGot += (s, e) => _commandEventHandler.HandleAsync(s, e);
            _chatEventHandler.Subscribe();
            _aiMessageEventHandler.Subscribe();
        }
    }
}
