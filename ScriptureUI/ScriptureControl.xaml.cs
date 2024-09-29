using ScriptureCore;
using System.CodeDom.Compiler;
using System.Windows.Controls;

namespace ScriptureUI
{
    /// <summary>
    /// Interaction logic for ScriptureControl.xaml
    /// </summary>
    public partial class ScriptureControl : UserControl
    {
        public ScriptureControl()
        {
            InitializeComponent();
        }

        private async void OnGenerateScriptClick(object sender, System.Windows.RoutedEventArgs e)
        {
            string userPrompt = ScriptDescriptionTextBox.Text;

            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                return;
            }

            try
            {
                var lLMServices = ServiceLocator.GetService<ILLMServices>();
                var compiler = ServiceLocator.GetService<ICompiler>();

                // Generate the script using the user's prompt
                string generatedScript = await lLMServices.GenerateInitialScriptAsync(userPrompt);

                // Display the generated script in the output TextBox
                ScriptEditor.Text = generatedScript;

                var res = compiler.TestCompile(generatedScript);

                ScriptStatusLabel.Content = res.Success ? "Successfully compiled" : "Compilation failed";
            }
            catch (Exception ex)
            {
                ScriptEditor.Text = ex.ToString();
            }
        }
    }
}
