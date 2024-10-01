using ScriptureCore;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace ScriptureUI
{
    public partial class ScriptureControl : UserControl, INotifyPropertyChanged
    {
        public ScriptureControl()
        {
            InitializeComponent();

            ScriptEditor.Options.IndentationSize = 4;
            ScriptEditor.Options.ConvertTabsToSpaces = false;

            _dllPath = ConfigurationManager.AppSettings["DllPath"];
            _dllPath = !string.IsNullOrWhiteSpace(_dllPath) 
                ? _dllPath
                : System.IO.Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDocuments), "AutoCADPlugins", "scripturePlugin.dll");


            DataContext = this;
        }

        public string? _dllPath;
        public string? DllPath
        {
            get => _dllPath;
            set
            {
                if (_dllPath != value)
                {
                    _dllPath = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _generatingScript = false;
        public bool GeneratingScript
        {
            get => _generatingScript;
            set
            {
                if (_generatingScript != value)
                {
                    _generatingScript = value;
                    OnPropertyChanged();

                    if (!_generatingScript)
                    {
                        ProgressStatusText = "";
                        _scriptCompiledWithoutErrors = false;
                    }
                }
            }
        }

        public bool _scriptCompiledWithoutErrors = false;
        public bool ScriptCompiledWithoutErrors
        {
            get => _scriptCompiledWithoutErrors;
            set
            {
                if (_scriptCompiledWithoutErrors != value)
                {
                    _scriptCompiledWithoutErrors = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool _executingScript = false;
        public bool ExecutingScript
        {
            get => _executingScript;
            set
            {
                if (_executingScript != value)
                {
                    _executingScript = value;
                    OnPropertyChanged();
                }
            }
        }

        private void SaveDllPathToConfig(string dllPath)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("DllPath");
            config.AppSettings.Settings.Add("DllPath", dllPath);
            config.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }

        private static int _fixErrorAttemptsNumber = 5;
        private async void OnGenerateScriptClick(object sender, RoutedEventArgs e)
        {
            string userPrompt = ScriptDescriptionTextBox.Text;

            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                ScriptDescriptionTextBox.Text = "Please enter script descripion here";
                return;
            }

            try
            {
                GeneratingScript = true;

                var llmServices = ServiceLocator.GetService<ILLMServices>();

                ProgressStatusText = "Generating initial script";
                var generatedScript = await llmServices.GenerateInitialScriptAsync(userPrompt);

                int attemptIndex = 0;
                while (true)
                {
                    ScriptEditor.Text = generatedScript;

                    Recompile();

                    if (attemptIndex == _fixErrorAttemptsNumber
                        || LastCompilationStatus.Success)
                    {
                        if (!LastCompilationStatus.Success)
                            MainTabControl.SelectedIndex = 1;

                        break;

                    }

                    attemptIndex++;
                    ProgressStatusText = $"Fixing generated script (attempt {attemptIndex} out of {_fixErrorAttemptsNumber})";

                    generatedScript = await llmServices.TryFixScriptAsync(generatedScript, LastCompilationStatus.Errors, false);                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
            finally
            {
                GeneratingScript = false;
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

                var commandName = ExtractCommandName(ScriptEditor.Text);
                if (commandName != null)
                {
                    CommandNameTextBox.Text = commandName;
                }
            }

            ScriptCompiledWithoutErrors = LastCompilationStatus.Success;
        }

        private void OnRecompileScriptClick(object sender, RoutedEventArgs e)
        {
            Recompile();
        }

        private async void OnTryFixClick(object sender, RoutedEventArgs e)
        {
            try
            {
                GeneratingScript = true;

                var llmServices = ServiceLocator.GetService<ILLMServices>();

                string script = ScriptEditor.Text;

                if (LastCompilationStatus.Errors.Count == 0)
                    return;

                ProgressStatusText = "Fixing generated script";
                var fixedScript = await llmServices.TryFixScriptAsync(script, LastCompilationStatus.Errors, true);

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
                MessageBox.Show(ex.ToString(), "Error");
            }
            finally
            {
                GeneratingScript = false;
            }
        }

        private void OnExecutionModeChanged(object sender, RoutedEventArgs e)
        {
            if (PluginOptionsPanel is null)
                return;

            if (((RadioButton)sender).IsChecked ?? false)
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

                if (CreatePluginRadioButton.IsChecked ?? false)
                {
                    if (string.IsNullOrWhiteSpace(DllPath))
                    {
                        var res = MessageBox.Show("Enter dll name please", "Error");
                        return;
                    }

                    if (File.Exists(DllPath))
                    {
                        var res = MessageBox.Show("The dll already exists, overwrite?", "Warning", MessageBoxButton.YesNo);
                        if (res == MessageBoxResult.No)
                            return;
                    }

                    SaveDllPathToConfig(DllPath);

                    var customCommandName = CommandNameTextBox.Text;
                    if (!string.IsNullOrWhiteSpace(customCommandName))
                    {
                        script = ReplaceCommandName(script, commandName, customCommandName);
                        commandName = customCommandName;
                    }

                    var (success, errors) = compiler.CompileTo(script, DllPath);

                    MessageBox.Show(success ? "Succeeded" : $"Failed:\n {string.Join(',', errors)}", "Result");
                }
                else
                {
                    var tempFileName = Path.Combine(Path.GetTempPath(), $"GeneratedScript_{Guid.NewGuid()}.dll");

                    var (success, errors) = compiler.CompileTo(script, tempFileName);

                    if (!success)
                    {
                        MessageBox.Show($"Failed to compile:#\n{string.Join(',', errors)}", "Result");
                        return;
                    }

                    var scriptExecutor = ServiceLocator.GetService<IScriptExecutor>();
                    scriptExecutor.Execute(tempFileName, commandName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private string _progressStatusText = "";
        public string ProgressStatusText
        {
            get => _progressStatusText;
            set
            {
                if (_progressStatusText != value)
                {
                    _progressStatusText = value;
                    OnPropertyChanged();
                }
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
