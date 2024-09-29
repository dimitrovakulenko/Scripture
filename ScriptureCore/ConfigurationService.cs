using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace ScriptureCore
{
    public class ConfigurationService
    {
        private readonly IConfiguration _configuration;

        public ConfigurationService()
        {
            var pluginLocation = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);
            if (string.IsNullOrEmpty(pluginLocation))
                throw new Exception("Executing dll doesn't exist?");

            _configuration = new ConfigurationBuilder()
                .SetBasePath(pluginLocation)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public IConfiguration GetConfiguration() => _configuration;
    }
}
