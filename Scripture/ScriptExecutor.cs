using Autodesk.AutoCAD.ApplicationServices;
using ScriptureCore;

namespace Scripture
{
    internal class ScriptExecutor : IScriptExecutor
    {
        public void Execute(string dllPath, string commandName)
        {
            var activeDocument = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

            // Set FILEDIA to 0 to suppress file dialog
            activeDocument.SendStringToExecute("FILEDIA 0\n", false, false, true);

            // Load the DLL using NETLOAD
            activeDocument.SendStringToExecute($"NETLOAD \"{dllPath}\"\n", false, false, true);

            // Restore FILEDIA to 1
            activeDocument.SendStringToExecute("FILEDIA 1\n", false, false, true);

            // Execute the command
            activeDocument.SendStringToExecute($"{commandName}\n", false, false, true);
        }
    }
}
