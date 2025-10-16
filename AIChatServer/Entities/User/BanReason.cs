using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AIChatServer.Entities.User
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BanReason
    {
        Undefined,
        CommunityRulesViolation,
        SpamOrAdvertising,
        OffensiveLanguage,
        SecurityViolation,
        UnauthorizedToolsUsage,
        HarassmentOrDiscrimination,
        MultipleUserReports,
        AdministrativeDecision,
    }
}
