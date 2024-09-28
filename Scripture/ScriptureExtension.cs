using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using ScriptureUI;
using System.Windows.Forms.Integration;

namespace Scripture
{
    public class ScriptureExtension : IExtensionApplication
    {
        private PaletteSet _paletteSet;

        public void Initialize()
        {
            _paletteSet = new PaletteSet("Scripture Panel");
            _paletteSet.Size = new System.Drawing.Size(300, 500); // Set panel size
            _paletteSet.Visible = true;

            // Create an instance of the WPF UserControl
            var wpfControl = new ScriptureControl(); // Your WPF UserControl

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
