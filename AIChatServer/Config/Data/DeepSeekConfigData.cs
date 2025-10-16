namespace AIChatServer.Config.Data
{
    public record DeepSeekConfigData
    (
        string ApiKey,
        string ApiUrl,
        int MaxTokenCount
    );
}
