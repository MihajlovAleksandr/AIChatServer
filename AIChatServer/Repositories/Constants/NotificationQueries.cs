namespace AIChatServer.Repositories.Constants
{
    public static class NotificationQueries
    {
        public const string UpdateNotifications = @"
            UPDATE user_notification_settings 
            SET enabled_email_notifications = @EmailNotifications
            WHERE user_id = @UserId";

        public const string GetNotifications =
            "SELECT enabled_email_notifications FROM user_notification_settings WHERE user_id = @UserId";

        public const string UpdateNotificationToken = @"
            UPDATE connections
            SET notification_token = @NotificationToken
            WHERE id = @Id";

        public const string GetNotificationTokens = @"
            SELECT user_id, notification_token
        FROM connections
        WHERE user_id = ANY(@ids)";
    }
}