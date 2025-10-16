using AIChatServer.Entities.DTO.Response;
using AIChatServer.Entities.User.ServerUsers.Implementations;
using AIChatServer.Managers.Interfaces;
using AIChatServer.Service.Interfaces;
using AIChatServer.Utils.Interfaces;
using Microsoft.Extensions.Logging;

namespace AIChatServer.Managers.Implementations
{
    public class ConnectionEventHandler : IEventHandler
    {
        private readonly IUserManager _userManager;
        private readonly IUserService _userService;
        private readonly ISendCommandMapper _commandMapper;
        private readonly ILogger<ConnectionEventHandler> _logger;

        public ConnectionEventHandler(
            IUserManager userManager,
            IUserService userService,
            ISendCommandMapper commandMapper,
            ILogger<ConnectionEventHandler> logger)
        {
            _userManager = userManager;
            _userService = userService;
            _commandMapper = commandMapper;
            _logger = logger;
        }

        public async Task HandleAsync(object? sender, object? args)
        {
            if (sender is not ServerUser user || args is not bool isOnline)
                throw new ArgumentException("Invalid connection event arguments");

            var response = new CommandResponse("UserOnlineChanges",
                new UserOnlineChangesResponse(user.User.Id, isOnline));

            var usersToNotify = _userService.GetUsersInSameChats(user.User.Id);
            await _userManager.SendCommandAsync(_commandMapper.MapToSendCommand(usersToNotify, response));

            _logger.LogInformation("User {UserId} online status changed: {IsOnline}", user.User.Id, isOnline);
        }

        public void Subscribe() { }
    }
}
