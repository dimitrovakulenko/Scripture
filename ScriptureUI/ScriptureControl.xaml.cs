using ScriptureCore;
using System.CodeDom.Compiler;
using System.Text.RegularExpressions;
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

            ScriptEditor.Options.IndentationSize = 4;
            ScriptEditor.Options.ConvertTabsToSpaces = false;
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
            if (MainProgressBar.Value == 100)
            {
                MainProgressBar.Visibility = System.Windows.Visibility.Collapsed;
                MainProgressBar.Value = 0;
            }
        }

        private void OnRecompileScriptClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var compiler = ServiceLocator.GetService<ICompiler>();

            var res = compiler.TestCompile(ScriptEditor.Text);
            ScriptStatusLabel.Content = res.Success
                ? "Successfully compiled"
                : $"Compilation failed : {string.Join(';', res.Errors)}";
        }

        private async void OnExecuteScriptClick(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                string script = ScriptEditor.Text;
                var commandName = ExtractCommandName(script);
                if (commandName is null)
                    throw new Exception("Command name not found");

                var compiler = ServiceLocator.GetService<ICompiler>();
                var (success, errors, dllPath) = compiler.CompileToTemporaryFile(script);

                if (!success)
                {
                    ScriptStatusLabel.Content = "Compilation failed";
                    ScriptEditor.Text = string.Join(Environment.NewLine, errors);
                    return;
                }

                var scriptExecutor = ServiceLocator.GetService<IScriptExecutor>();
                scriptExecutor.Execute(dllPath, commandName);

                ScriptStatusLabel.Content = "Script executed successfully";
            }
            catch (Exception ex)
            {
                ScriptStatusLabel.Content = "Execution failed";
                ScriptEditor.Text = ex.ToString();
            }
        }

        private string ExtractCommandName(string script)
        {
            // Regular expression to find the command name in [CommandMethod("CommandName")]
            var match = Regex.Match(script, @"\[CommandMethod\(""([^""]+)""\)\]");

            // If a match is found, return the command name, otherwise return null
            return match.Success ? match.Groups[1].Value : null;
        }

    }
}
