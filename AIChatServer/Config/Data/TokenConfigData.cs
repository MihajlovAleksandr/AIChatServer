namespace AIChatServer.Config.Data
{
    public record TokenConfigData
    (
        string SecretKey,
        string Issuer,
        string Audience,
        int ExpireDays,
        int ExpireMinutes
    );
}
