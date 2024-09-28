using Microsoft.Extensions.DependencyInjection;
using System;

namespace ScriptureCore
{
    public static class ServiceLocator
    {
        public static IServiceProvider? ServiceProvider { get; private set; }

        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public static T GetService<T>() where T : class
        {
            if (ServiceProvider == null)
            {
                throw new InvalidOperationException("ServiceProvider is not set. Make sure to call SetServiceProvider during application initialization.");
            }

            return ServiceProvider.GetRequiredService<T>();
        }
    }
}
