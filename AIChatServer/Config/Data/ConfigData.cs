namespace AIChatServer.Config.Data
{
    public record ConfigData(
        ServerConfigData ServerConfigData,
        TokenConfigData TokenConfigData,
        AIConfigData AIConfigData,
        AIMessageBufferData AIMessageBufferData,
        DeepSeekConfigData DeepSeekConfigData,
        EmailConfigData EmailConfigData,
        GoogleConfigData GoogleConfigData
    );
}
