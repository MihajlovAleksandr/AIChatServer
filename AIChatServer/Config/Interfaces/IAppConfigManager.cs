using AIChatServer.Config.Data;

namespace AIChatServer.Config.Interfaces
{
    public interface IAppConfigManager
    {
        ConfigData GetConfigData();
    }
}
