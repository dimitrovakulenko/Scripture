using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Microsoft.Extensions.DependencyInjection;
using ScriptureCore;

namespace ScriptureHarnessApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public App()
        {
            string[] args = Environment.GetCommandLineArgs();
            bool loadAutocadDlls = !args.Contains("--noAutoCADDlls"); // Default to true unless explicitly disabled

            // Load AutoCAD DLLs if required
            if (loadAutocadDlls)
            {
                LoadAutoCADDlls();
            }

            var serviceCollection = new ServiceCollection();
            ServiceRegistration.RegisterServices(serviceCollection);

            // Build the service provider and set it in ServiceLocator
            var serviceProvider = serviceCollection.BuildServiceProvider();
            ServiceLocator.SetServiceProvider(serviceProvider);
        }

        private void LoadAutoCADDlls()
        {
            try
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                ObjectId objectId = new ObjectId();
                Point3d point3D = new Point3d();
                if (objectId.IsErased) point3D = point3D * 2;
            }
            catch { }
        }
    }

}
