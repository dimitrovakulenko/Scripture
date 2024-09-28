using Microsoft.Extensions.DependencyInjection;

namespace ScriptureCore
{
    public static class ServiceRegistration
    {
        public static void RegisterServices(IServiceCollection services)
        {
            // Register ConfigurationService as a singleton
            services.AddSingleton<ConfigurationService>();

            // Register OpenAIService as a singleton
            services.AddSingleton<OpenAIService>(provider =>
            {
                var configService = provider.GetRequiredService<ConfigurationService>();
                return new OpenAIService(configService.GetConfiguration());
            });
        }
    }
}
