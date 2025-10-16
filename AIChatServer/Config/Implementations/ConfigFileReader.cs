using AIChatServer.Config.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AIChatServer.Config.Implementations
{
    public class ConfigFileReader(string jsonFile) : IConfigReader
    {
        private readonly IConfigurationRoot configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(jsonFile)
                .Build();

        public string? GetDataFromPath(string path)
        {
            string[] paths = path.Split('.');
            var selection = configurationRoot.GetSection(paths[0]);
            for (int i = 1; i < paths.Length - 1; i++)
            {
                selection = selection.GetSection(paths[i]);
            }
            return selection[paths[^1]];
        }
    }
}
