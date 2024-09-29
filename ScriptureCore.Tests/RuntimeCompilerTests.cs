using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Microsoft.Extensions.DependencyInjection;

namespace ScriptureCore.Tests
{
    public class RuntimeCompilerTests
    {
        public RuntimeCompilerTests()
        {
            // hacky load of autocad dlls
            try
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Editor ed = doc.Editor;
                ObjectId objectId = new ObjectId();
                Point3d point3D = new Point3d();
                if (objectId.IsErased) point3D = point3D * 2;
            }
            catch { }

            // register services
            var serviceCollection = new ServiceCollection();
            ServiceRegistration.RegisterServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            ServiceLocator.SetServiceProvider(serviceProvider);
        }

        [Fact]
        public void TestAutocadDllsFound()
        {
            var code = @"
        using Autodesk.AutoCAD.ApplicationServices;
        using Autodesk.AutoCAD.DatabaseServices;
        using Autodesk.AutoCAD.EditorInput;
        using Autodesk.AutoCAD.Geometry;

        namespace AutoCADPlugin
        {
            public class PolygonSelector
            {
                public void SelectSmallPolygons()
                {
                }
            }
        }";

            var compiler = ServiceLocator.GetService<ICompiler>();

            var (success, errors) = compiler.TestCompile(code);

            Assert.True(success, $"Compilation failed with errors: {string.Join(Environment.NewLine, errors)}");
        }
    }
}