namespace AIChatServer.Repositories.Constants
{
    public static class AuthQueries
    {
        public const string VerifyGoogleId =
            "SELECT COUNT(*) FROM users_credentials WHERE email = @Email AND google_id = @GoogleId";

        public const string ChangePassword =
            "UPDATE users_credentials SET password = @Password WHERE id = @Id";

        public const string GetUserBanByEmail = @"
            SELECT ub.id, user_id, ub.reason, ub.reason_category, ub.banned_at, ub.banned_until, uc.id AS user_id
            FROM users_bans ub
            JOIN users_credentials uc ON ub.user_id = uc.id
            WHERE uc.email = @Email
            AND ub.banned_until > CURRENT_TIMESTAMP
            LIMIT 1";

        public const string GetUserBanById = @"
            SELECT ub.id, ub.user_id, ub.reason, ub.reason_category, ub.banned_at, ub.banned_until
            FROM users_bans ub
            WHERE ub.user_id = @Id
            AND ub.banned_until > CURRENT_TIMESTAMP
            LIMIT 1";
    }
}
