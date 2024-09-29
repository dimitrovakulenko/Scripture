using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

namespace ScriptureCore.Tests
{
    public class RuntimeCompilerTests
    {
        public RuntimeCompilerTests()
        {
            try
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
            }
            catch { }
            try
            {
                Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            }
            catch { }
            ObjectId objectId = new ObjectId();
            Point3d point3D = new Point3d();
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

            var (success, errors) = RuntimeCompiler.TestCompile(code);

            Assert.True(success, $"Compilation failed with errors: {string.Join(Environment.NewLine, errors)}");
        }
    }
}