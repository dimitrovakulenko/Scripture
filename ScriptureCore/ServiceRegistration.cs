using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ScriptureCore
{
    public static class ServiceRegistration
    {
        public static void RegisterServices(IServiceCollection services, IScriptExecutor? scriptExecutor)
        {
            // Register ConfigurationService as a singleton
            services.AddSingleton<ConfigurationService>();

            // Register OpenAIService as a singleton
            services.AddSingleton<ILLMServices, OpenAIService>(provider =>
            {
                var configService = provider.GetRequiredService<ConfigurationService>();
                return new OpenAIService(configService.GetConfiguration());
            });

            services.AddSingleton<ICompiler, RuntimeCompiler>();

            if(scriptExecutor != null)
                services.AddSingleton<IScriptExecutor>(provide =>
                {
                    return scriptExecutor;
                });
        }
    }
}
