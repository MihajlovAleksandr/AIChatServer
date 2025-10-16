namespace AIChatServer.Repositories.Constants
{
    public static class UserQueries
    {
        public const string GetUserById = @"
            SELECT 
                uc.id, uc.email, uc.password,
                up.end_at AS premium_until,
                uc.google_id,
                ud.id AS user_data_id, ud.name, ud.gender, ud.age,
                p.id AS preference_id, p.min_age, p.max_age, p.preferred_gender
            FROM users_credentials uc
            LEFT JOIN user_data ud ON uc.id = ud.user_id
            LEFT JOIN preferences p ON uc.id = p.user_id
            LEFT JOIN users_premium up ON uc.id = up.user_id AND up.end_at > CURRENT_TIMESTAMP
            WHERE uc.id = @userId AND uc.delete_status = false";

        public const string GetUserByEmail = @"
            SELECT 
                uc.id, uc.email, uc.password,
                up.end_at AS premium_until,
                uc.google_id,
                ud.id AS user_data_id, ud.name, ud.gender, ud.age,
                p.id AS preference_id, p.min_age, p.max_age, p.preferred_gender
            FROM users_credentials uc
            LEFT JOIN user_data ud ON uc.id = ud.user_id
            LEFT JOIN preferences p ON uc.id = p.user_id
            LEFT JOIN users_premium up ON uc.id = up.user_id AND up.end_at > CURRENT_TIMESTAMP
            WHERE uc.email = @Email AND uc.delete_status = false";

        public const string IsEmailFree =
            "SELECT COUNT(*) FROM users_credentials WHERE email = @Email AND delete_status = false";

        public const string InsertUserWithPassword =
            "INSERT INTO users_credentials (email, password) VALUES (@Email, @Password) RETURNING id;";

        public const string InsertUserWithGoogle =
            "INSERT INTO users_credentials (email, google_id) VALUES (@Email, @GoogleId) RETURNING id;";

        public const string InsertUserData =
            "INSERT INTO user_data (user_id, name, gender, age) VALUES (@UserId, @Name, @Gender, @Age)";

        public const string InsertPreference =
            "INSERT INTO preferences (user_id, min_age, max_age, preferred_gender) VALUES (@UserId, @MinAge, @MaxAge, @PreferredGender)";

        public const string InsertNotifications =
            "INSERT INTO user_notification_settings(user_id, enabled_email_notifications) VALUES (@UserId, true)";

        public const string UpdateConnection =
            "UPDATE connections SET user_id = @UserId WHERE id = @Id";

        public const string UpdateUserData =
            "UPDATE user_data SET name = @Name, gender = @Gender, age = @Age WHERE user_id = @UserId";

        public const string UpdatePreference =
            "UPDATE preferences SET min_age = @MinAge, max_age = @MaxAge, preferred_gender = @PreferredGender WHERE user_id = @UserId";

        public const string IsUserPremium =
            "SELECT COUNT(*) FROM users_premium WHERE user_id = @UserId AND end_at > CURRENT_TIMESTAMP";

        public const string GetUsersInSameChats = @"
            SELECT DISTINCT uc1.user_id
            FROM users_chats uc1
            JOIN users_chats uc2 ON uc1.chat_id = uc2.chat_id
            WHERE uc2.user_id = @targetUserId AND uc1.user_id != @targetUserId";
    }
}