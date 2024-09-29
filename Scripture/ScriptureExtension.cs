using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Microsoft.Extensions.DependencyInjection;
using ScriptureCore;
using ScriptureUI;
using System.Runtime.Versioning;
using System.Windows.Forms.Integration;

namespace Scripture
{
    [SupportedOSPlatform("windows")]
    public class ScriptureExtension : IExtensionApplication
    {
        private PaletteSet? _paletteSet;

        public void Initialize()
        {
            System.Diagnostics.Debugger.Launch();

            var serviceCollection = new ServiceCollection();
            ServiceRegistration.RegisterServices(serviceCollection);

            // Build the service provider and set it in ServiceLocator
            var serviceProvider = serviceCollection.BuildServiceProvider();
            ServiceLocator.SetServiceProvider(serviceProvider);

            // build the palette
            _paletteSet = new PaletteSet("Scripture Panel");
            _paletteSet.Size = new System.Drawing.Size(600, 800);
            _paletteSet.Visible = true;

            // Create an instance of the WPF UserControl
            var wpfControl = new ScriptureControl();

            // Create an ElementHost to host the WPF control
            ElementHost elementHost = new ElementHost
            {
                Child = wpfControl, // Assign the WPF UserControl
                Dock = DockStyle.Fill // Make the WPF control fill the parent panel
            };

            // Add the ElementHost to a WinForms Panel
            Panel panel = new Panel
            {
                Dock = DockStyle.Fill
            };
            panel.Controls.Add(elementHost);

            // Add the panel to the PaletteSet
            _paletteSet.Add("Scripture UI", panel);
        }

        public void Terminate()
        {

        }

        [CommandMethod("ShowScripturePanel")]
        public void ShowScripturePanel()
        {
            if (_paletteSet != null)
            {
                _paletteSet.Visible = true; // Make the PaletteSet visible
            }
        }
    }
}
