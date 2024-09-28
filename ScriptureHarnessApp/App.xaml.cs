using Microsoft.Extensions.DependencyInjection;
using ScriptureCore;
using System.Configuration;
using System.Data;
using System.Windows;

namespace ScriptureHarnessApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var serviceCollection = new ServiceCollection();
            ServiceRegistration.RegisterServices(serviceCollection);

            // Build the service provider and set it in ServiceLocator
            var serviceProvider = serviceCollection.BuildServiceProvider();
            ServiceLocator.SetServiceProvider(serviceProvider);
        }
    }

}
