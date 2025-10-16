using AIChatServer.Config.Data;
using AIChatServer.Factories.Containers;
using AIChatServer.Factories.Interfaces;
using AIChatServer.Utils.Implementations;

namespace AIChatServer.Factories.Implementations
{
    public class TokenManagerFactory : ITokenManagerFactory
    {
        public TokenManagerContainer CreateTokenManagers(ConfigData configData)
        {
            var connectionTokenManager = new ConnectionTokenManager(
                configData.TokenConfigData.SecretKey,
                configData.TokenConfigData.Issuer,
                configData.TokenConfigData.Audience,
                configData.TokenConfigData.ExpireDays);
            var entryTokenManager = new EntryTokenManager(
                configData.TokenConfigData.SecretKey,
                configData.TokenConfigData.Issuer,
                configData.TokenConfigData.Audience,
                configData.TokenConfigData.ExpireMinutes);

            return new TokenManagerContainer(connectionTokenManager, entryTokenManager);
        }
    }
}
