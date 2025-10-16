using AIChatServer.Config.Data;
using AIChatServer.Config.Interfaces;
using AIChatServer.Config.Paths;

namespace AIChatServer.Config.Implementations
{
    public class AppConfigManager(IConfigReader configReader) : IAppConfigManager
    {
        private readonly IConfigReader _configReader = configReader
            ?? throw new ArgumentNullException(nameof(configReader));
        public ConfigData GetConfigData()
        {
            ServerConfigData serverConfigData = new(
                _configReader.GetDataFromPath($"{ServerConfigPaths.BasePath}.{ServerConfigPaths.ServerURL}")
                    ?? throw new ArgumentNullException(nameof(ServerConfigPaths.ServerURL))
            );
            TokenConfigData tokenConfigData = new TokenConfigData(
                _configReader.GetDataFromPath($"{TokenConfigPaths.BasePath}.{TokenConfigPaths.Key}")
                    ?? throw new ArgumentNullException(nameof(TokenConfigPaths.Key)),
                _configReader.GetDataFromPath($"{TokenConfigPaths.BasePath}.{TokenConfigPaths.Issuer}")
                    ?? throw new ArgumentNullException(nameof(TokenConfigPaths.Issuer)),
                _configReader.GetDataFromPath($"{TokenConfigPaths.BasePath}.{TokenConfigPaths.Audience}")
                    ?? throw new ArgumentNullException(nameof(TokenConfigPaths.Audience)),
                int.Parse(_configReader.GetDataFromPath($"{TokenConfigPaths.BasePath}.{TokenConfigPaths.ExpireDays}")
                    ?? throw new ArgumentNullException(nameof(TokenConfigPaths.ExpireDays))),
                int.Parse(_configReader.GetDataFromPath($"{TokenConfigPaths.BasePath}.{TokenConfigPaths.ExpireMinutes}")
                    ?? throw new ArgumentNullException(nameof(TokenConfigPaths.ExpireMinutes)))
            );
            AIConfigData aIConfigData = new AIConfigData(
                int.Parse(_configReader.GetDataFromPath($"{AIConfigPaths.BasePath}.{AIConfigPaths.ProbabilityAIChat}")
                    ?? throw new ArgumentNullException(nameof(AIConfigPaths.ProbabilityAIChat))),
                Guid.Parse(_configReader.GetDataFromPath($"{AIConfigPaths.BasePath}.{AIConfigPaths.AIId}")
                    ?? throw new ArgumentNullException(nameof(AIConfigPaths.AIId)))
            );
            AIMessageBufferData aIMessageBufferData = new AIMessageBufferData(
                new AIMessageBufferSize(
                    int.Parse(_configReader.GetDataFromPath($"{AIMessageBufferConfigPaths.BasePath}.{AIMessageBufferConfigPaths.MessageMin}")
                        ?? throw new ArgumentNullException(nameof(AIMessageBufferConfigPaths.MessageMin))),
                    int.Parse(_configReader.GetDataFromPath($"{AIMessageBufferConfigPaths.BasePath}.{AIMessageBufferConfigPaths.MessageMax}")
                        ?? throw new ArgumentNullException(nameof(AIMessageBufferConfigPaths.MessageMax)))
                ),
                new AIMessageBufferSize(
                    int.Parse(_configReader.GetDataFromPath($"{AIMessageBufferConfigPaths.BasePath}.{AIMessageBufferConfigPaths.CompressedMessageMin}")
                        ?? throw new ArgumentNullException(nameof(AIMessageBufferConfigPaths.CompressedMessageMin))),
                    int.Parse(_configReader.GetDataFromPath($"{AIMessageBufferConfigPaths.BasePath}.{AIMessageBufferConfigPaths.CompressedMessageMax}")
                        ?? throw new ArgumentNullException(nameof(AIMessageBufferConfigPaths.CompressedMessageMax)))
                )
            );
            DeepSeekConfigData deepSeekConfigData = new DeepSeekConfigData(
                _configReader.GetDataFromPath($"{DeepSeekConfigPaths.BasePath}.{DeepSeekConfigPaths.Key}")
                    ?? throw new ArgumentNullException(nameof(DeepSeekConfigPaths.Key)),
                _configReader.GetDataFromPath($"{DeepSeekConfigPaths.BasePath}.{DeepSeekConfigPaths.Url}")
                    ?? throw new ArgumentNullException(nameof(DeepSeekConfigPaths.Url)),
                int.Parse(_configReader.GetDataFromPath($"{DeepSeekConfigPaths.BasePath}.{DeepSeekConfigPaths.MaxTokenCount}")
                    ?? throw new ArgumentNullException(nameof(DeepSeekConfigPaths.MaxTokenCount)))
            );
            EmailConfigData emailConfigData = new EmailConfigData(
                 _configReader.GetDataFromPath($"{EmailConfigPaths.BasePath}.{EmailConfigPaths.SmtpServer}")
                    ?? throw new ArgumentNullException(nameof(EmailConfigPaths.SmtpServer)),
                int.Parse(_configReader.GetDataFromPath($"{EmailConfigPaths.BasePath}.{EmailConfigPaths.SmtpPort}")
                    ?? throw new ArgumentNullException(nameof(EmailConfigPaths.SmtpPort))),
                _configReader.GetDataFromPath($"{EmailConfigPaths.BasePath}.{EmailConfigPaths.SenderEmail}")
                    ?? throw new ArgumentNullException(nameof(EmailConfigPaths.SenderEmail)),
                _configReader.GetDataFromPath($"{EmailConfigPaths.BasePath}.{EmailConfigPaths.Password}")
                    ?? throw new ArgumentNullException(nameof(EmailConfigPaths.Password))
            );
            GoogleConfigData googleConfigData = new GoogleConfigData(
                _configReader.GetDataFromPath($"{GoogleConfigPaths.BasePath}.{GoogleConfigPaths.ClientId}")
                    ?? throw new ArgumentNullException(nameof(GoogleConfigPaths.ClientId))
            );

            return new ConfigData(serverConfigData, tokenConfigData, aIConfigData, aIMessageBufferData, deepSeekConfigData, emailConfigData, googleConfigData);
        }
    }
}
