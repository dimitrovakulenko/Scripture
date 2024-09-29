using ScriptureCore;
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
            // Get user prompt from the input TextBox
            string userPrompt = ScriptDescriptionTextBox.Text;

            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                //MessageBox.Show("Please enter a description for the script.", "Input Error");
                return;
            }

            try
            {
                // Get the OpenAIService instance from the ServiceLocator
                var openAIService = ServiceLocator.GetService<OpenAIService>();

                // Generate the script using the user's prompt
                string generatedScript = await openAIService.GenerateInitialScriptAsync(userPrompt);

                // Display the generated script in the output TextBox
                ScriptEditor.Text = generatedScript;

                var res = RuntimeCompiler.TestCompile(generatedScript);

                ScriptStatusLabel.Content = res.Success ? "Successfully compiled" : "Compilation failed";
            }
            catch (Exception ex)
            {
                ScriptEditor.Text = ex.ToString();
            }
        }
    }
}
