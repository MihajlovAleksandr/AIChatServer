using AIChatServer.Entities.User;

namespace AIChatServer.Entities.Exceptions
{
    public class UserBannedException(UserBan userBan) : 
        Exception($"User {{{userBan.UserId}}} is banned until {userBan.BannedUntil}. Reason: {userBan.Reason}")
    {
        public UserBan UserBan { get; } = userBan ?? throw new ArgumentNullException(nameof(userBan));
    }
}
