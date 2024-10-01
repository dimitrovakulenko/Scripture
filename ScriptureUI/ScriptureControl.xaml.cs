using ScriptureCore;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace ScriptureUI
{
    public partial class ScriptureControl : UserControl
    {
        public ScriptureControl()
        {
            InitializeComponent();

            ScriptEditor.Options.IndentationSize = 4;
            ScriptEditor.Options.ConvertTabsToSpaces = false;

            var dllPath = ConfigurationManager.AppSettings["DllPath"];
            DllPathTextBox.Text = !string.IsNullOrWhiteSpace(dllPath) 
                ? dllPath 
                : System.IO.Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDocuments), "AutoCADPlugins"); ;
        }

        private void SaveDllPathToConfig(string dllPath)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("DllPath");
            config.AppSettings.Settings.Add("DllPath", dllPath);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private async void OnGenerateScriptClick(object sender, RoutedEventArgs e)
        {
            string userPrompt = ScriptDescriptionTextBox.Text;

            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                return;
            }

            try
            {
                GenerateScriptButton.IsEnabled = false;

                var llmServices = ServiceLocator.GetService<ILLMServices>();

                var generatedScript = await llmServices.GenerateInitialScriptAsync(userPrompt);

                ScriptEditor.Text = generatedScript;

                Recompile();

                if (!LastCompilationStatus.Success)
                {
                    MainTabControl.SelectedIndex = 1;
                }
                ((TabItem)MainTabControl.Items[1]).IsEnabled = true;
            }
            catch (Exception ex)
            {
                ScriptStatusTextBox.Text = "An error occurred: " + ex.ToString();
            }
            finally
            {
                GenerateScriptButton.IsEnabled = true;
            }
        }

        private (bool Success, List<string> Errors) LastCompilationStatus { get; set; }

        private void Recompile()
        {
            var compiler = ServiceLocator.GetService<ICompiler>();

            LastCompilationStatus = compiler.TestCompile(ScriptEditor.Text);
            ScriptStatusTextBox.Text = LastCompilationStatus.Success
                ? "Successfully compiled"
                : $"Compilation failed:{Environment.NewLine} " +
                $"{string.Join(Environment.NewLine, LastCompilationStatus.Errors)}";

            if (LastCompilationStatus.Success)
            {
                MainTabControl.SelectedIndex = 2;
                ((TabItem)MainTabControl.Items[2]).IsEnabled = true;

                var commandName = ExtractCommandName(ScriptEditor.Text);
                if (commandName != null)
                {
                    CommandNameTextBox.Text = commandName;
                }
            }
            else
            {
                ((TabItem)MainTabControl.Items[2]).IsEnabled = false;
            }
        }

        private void OnRecompileScriptClick(object sender, RoutedEventArgs e)
        {
            Recompile();
        }

        private async void OnTryFixClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var llmServices = ServiceLocator.GetService<ILLMServices>();

                string script = ScriptEditor.Text;

                if (LastCompilationStatus.Errors.Count == 0)
                {
                    return;
                }

                var fixedScript = await llmServices.TryFixScriptAsync(script, LastCompilationStatus.Errors);

                if (!string.IsNullOrEmpty(fixedScript))
                {
                    ScriptEditor.Text = fixedScript;

                    Recompile();
                }
                else
                    throw new Exception("No reply");
            }
            catch (Exception ex)
            {
                // TODO: handle error
            }
        }

        private void OnExecutionModeChanged(object sender, RoutedEventArgs e)
        {
            if ((bool)((RadioButton)sender).IsChecked && PluginOptionsPanel != null)
            {
                var radioButton = sender as RadioButton;

                if (radioButton?.Content.ToString() == "Create Plugin")
                {
                    PluginOptionsPanel.Visibility = Visibility.Visible;
                    ExecuteScriptButton.Content = "Create Plugin";
                }
                else
                {
                    PluginOptionsPanel.Visibility = Visibility.Collapsed;
                    ExecuteScriptButton.Content = "Execute Script";
                }
            }
        }

        private void OnExecuteScriptClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string script = ScriptEditor.Text;
                var commandName = ExtractCommandName(script);
                if (commandName is null)
                    throw new Exception("Command name not found");

                var compiler = ServiceLocator.GetService<ICompiler>();

                if ((bool)CreatePluginRadioButton.IsChecked)
                {
                    var dllPath = DllPathTextBox.Text;
                    if (string.IsNullOrWhiteSpace(dllPath))
                    {
                        return;
                    }

                    SaveDllPathToConfig(dllPath);

                    var customCommandName = CommandNameTextBox.Text;
                    if (!string.IsNullOrWhiteSpace(customCommandName))
                    {
                        script = ReplaceCommandName(script, commandName, customCommandName);
                        commandName = customCommandName;
                    }

                    // TODO:
                    // var (success, errors, dllFilePath) = compiler.CompileToCustomPath(script, dllPath);
                    // Handle success and errors...

                    ScriptStatusTextBox.Text = "Plugin created successfully";
                }
                else
                {
                    var (success, errors, dllFilePath) = compiler.CompileToTemporaryFile(script);

                    if (!success)
                        return;

                    var scriptExecutor = ServiceLocator.GetService<IScriptExecutor>();
                    scriptExecutor.Execute(dllFilePath, commandName);

                    ScriptStatusTextBox.Text = "Script executed successfully";
                }
            }
            catch (Exception ex)
            {
                ScriptStatusTextBox.Text = "Execution failed";
            }
        }

        private string? ExtractCommandName(string script)
        {
            var match = Regex.Match(script, @"\[CommandMethod\(""([^""]+)""\)\]");
            return match.Success ? match.Groups[1].Value : null;
        }

        private string ReplaceCommandName(string script, string oldCommandName, string newCommandName)
        {
            return script.Replace($"[CommandMethod(\"{oldCommandName}\")]", $"[CommandMethod(\"{newCommandName}\")]");
        }
    }
}
