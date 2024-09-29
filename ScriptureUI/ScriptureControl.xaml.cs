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
                // Disable the button during processing
                GenerateScriptButton.IsEnabled = false;

                // Show progress bar and status
                UpdateProgress("Generating initial script...", 20);

                var llmServices = ServiceLocator.GetService<ILLMServices>();
                var compiler = ServiceLocator.GetService<ICompiler>();

                // Generate the script using the user's prompt
                string generatedScript = await llmServices.GenerateInitialScriptAsync(userPrompt);

                // Display the generated script in the output TextBox
                ScriptEditor.Text = generatedScript;

                // Update progress
                UpdateProgress("Compiling script...", 60);

                var res = compiler.TestCompile(generatedScript);

                ScriptStatusLabel.Content = res.Success 
                    ? "Successfully compiled" 
                    : $"Compilation failed : {string.Join(';', res.Errors)}";
                UpdateProgress(res.Success ? "Compilation succeeded" : "Compilation failed", 100);
            }
            catch (Exception ex)
            {
                ScriptEditor.Text = ex.ToString();
                ScriptStatusLabel.Content = "An error occurred";
                UpdateProgress("Error occurred", 100);
            }
            finally
            {
                GenerateScriptButton.IsEnabled = true;
            }
        }

        private void UpdateProgress(string status, int progressValue)
        {
            if (MainProgressBar.Value == 0)
                MainProgressBar.Visibility = System.Windows.Visibility.Visible;
            ProgressStatusLabel.Text = status;
            MainProgressBar.Value = progressValue;
            if(MainProgressBar.Value == 100)
                MainProgressBar.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void OnRecompileScriptClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var compiler = ServiceLocator.GetService<ICompiler>();

            var res = compiler.TestCompile(ScriptEditor.Text);
            ScriptStatusLabel.Content = res.Success
                ? "Successfully compiled"
                : $"Compilation failed : {string.Join(';', res.Errors)}";
        }
    }
}
