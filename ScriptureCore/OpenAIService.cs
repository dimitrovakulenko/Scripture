using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text;
using System.Text.RegularExpressions;

namespace ScriptureCore
{
    internal class OpenAIService : ILLMServices
    {
        private readonly ChatClient _initialScriptClient;

        public OpenAIService(IConfiguration configuration)
        {
            string? apiKey = configuration["InitialScriptModel:ApiKey"];
            string? endpoint = configuration["InitialScriptModel:Endpoint"];
            string? modelName = configuration["InitialScriptModel:ModelName"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentNullException(nameof(apiKey), "API key must not be null or empty.");
            }

            // Check if the endpoint is specified to determine if it's Azure or OpenAI
            if (string.IsNullOrEmpty(endpoint))
            {
                // OpenAI directly
                var openAiClient = new OpenAIClient(apiKey);
                _initialScriptClient = openAiClient.GetChatClient(modelName);
            }
            else
            {
                // Azure OpenAI
                if (string.IsNullOrEmpty(modelName))
                {
                    throw new ArgumentNullException(nameof(modelName), "Deployment name must not be null or empty for Azure.");
                }

                var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
                _initialScriptClient = azureClient.GetChatClient(modelName);
            }
        }

        public async Task<string> GenerateInitialScriptAsync(string prompt)
        {
            ChatCompletionOptions options = new()
            {
                MaxOutputTokenCount = 1024,
                Temperature = 0.3f,
            };

            var messages = new List<ChatMessage>()
            {
                new SystemChatMessage(GenerateInitialScriptSystemMessage()),
                new UserChatMessage(prompt),
            };

            var completeAnswer = new StringBuilder();
            ClientResult<ChatCompletion> completionsResponse;

            do
            {
                completionsResponse = await _initialScriptClient.CompleteChatAsync(messages, options);

                // Add the generated response to the complete answer
                completeAnswer.Append(
                    RemoveCodeFence(
                        string.Join(
                            "", 
                            completionsResponse.Value.Content.Select(c => c.Text))));

                // Add the new response to the message list to continue the conversation contextually
                messages.Add(new AssistantChatMessage(string.Join("", completionsResponse.Value.Content.Select(c => c.Text))));
            }
            while (completionsResponse.Value.FinishReason == ChatFinishReason.Length);

            return completeAnswer.ToString();
        }

        public async Task<string> TryFixScriptAsync(string script, List<string> errorMessages, bool provideAdditionalMetadata)
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(@"You are an expert C# developer specializing in AutoCAD ObjectARX.NET API. 
                    Your task is to fix the provided script based on the error messages below.
                    Please ensure:
                    1. The corrected version is complete, with all necessary 'using' statements, namespaces, and class definitions.
                    2. Return only the corrected C# code, without any extra explanations or comments."),

                new UserChatMessage($"Here is the script that needs fixing:\n```csharp\n{script}\n```"),
                new UserChatMessage($"Here are the error messages:\n{string.Join("\n", errorMessages)}")
            };

            if (provideAdditionalMetadata)
            {
                messages.AddRange(CollectExtraMessagesForErrors(script, errorMessages));
            }

            ChatCompletionOptions options = new()
            {
                MaxOutputTokenCount = 1024,
                Temperature = 0.3f,
            };

            var completeAnswer = new StringBuilder();
            ClientResult<ChatCompletion> completionsResponse;

            do
            {
                completionsResponse = await _initialScriptClient.CompleteChatAsync(messages, options);

                // Add the generated response to the complete answer
                completeAnswer.Append(
                    RemoveCodeFence(
                        string.Join(
                            "",
                            completionsResponse.Value.Content.Select(c => c.Text))));

                // Add the new response to the message list to continue the conversation contextually
                messages.Add(new AssistantChatMessage(string.Join("", completionsResponse.Value.Content.Select(c => c.Text))));
            }
            while (completionsResponse.Value.FinishReason == ChatFinishReason.Length);

            return completeAnswer.ToString();
        }

        private List<ChatMessage> CollectExtraMessagesForErrors(string script, List<string> errorMessages)
        {
            var result = new List<ChatMessage>();

            var typeNames = new List<string>();
            foreach (var errorMessage in errorMessages)
            {
                if (Regex.IsMatch(errorMessage, @"'([^']+)' does not contain a definition for '([^']+)'"))
                {
                    var typeName = Regex.Match(errorMessage, @"'([^']+)' does not contain a definition for").Groups[1].Value;
                    typeNames.Add(typeName);
                }
                else if (Regex.IsMatch(errorMessage, @"Non-invocable member '([^']+)' cannot be used like a method"))
                {
                    var typeName = Regex.Match(errorMessage, @"Non-invocable member '([^\.]+)\.").Groups[1].Value;
                    typeNames.Add(typeName);
                }
            }

            if (typeNames.Any())
            {
                var compiler = ServiceLocator.GetService<ICompiler>();
                foreach (var typeName in typeNames)
                {
                    var reflectionInfo = GetReflectionInfo(
                        compiler.GetFullyQualifiedTypeName(
                            typeName, script));
                    if (!string.IsNullOrEmpty(reflectionInfo))
                    {
                        result.Add(new UserChatMessage($"Here is the information about the type '{typeName}':\n{reflectionInfo}"));
                    }
                }
            }

            return result;
        }

        private string GetReflectionInfo(string typeName)
        {
            try
            {
                var type = Type.GetType(typeName);
                if (type == null)
                {
                    return string.Empty;
                }

                var properties = type.GetProperties();
                var methods = type.GetMethods();

                var propertyInfo = string.Join(", ", properties.Select(p => $"{p.PropertyType.Name} {p.Name}"));
                var methodInfo = string.Join(", ", methods.Select(m => $"{m.ReturnType.Name} {m.Name}()"));

                return $"Properties: {propertyInfo}\nMethods: {methodInfo}";
            }
            catch
            {
                return string.Empty;
            }
        }


        private static string RemoveCodeFence(string code)
        {
            if (code.StartsWith("```csharp", StringComparison.OrdinalIgnoreCase))
            {
                code = code.Substring(9).TrimStart(); // Remove ```csharp and any extra whitespace/new lines
            }

            if (code.EndsWith("```", StringComparison.OrdinalIgnoreCase))
            {
                code = code.Substring(0, code.Length - 3).TrimEnd(); // Remove ``` and any extra whitespace/new lines
            }

            return code;
        }

        private string GenerateInitialScriptSystemMessage()
        {
            return @"You are an expert C# developer specializing in AutoCAD ObjectARX.NET API.
            Your task is to generate a C# function to solve the user's problem based on the given description.
            The function must use ObjectARX.NET classes and methods to accomplish the user's requirements.
            Please ensure:
            1. The function is complete, with all necessary 'using' statements, namespaces, and class definitions.
            2. Decompose the problem into multiple sub-functions if needed.
            3. Use only .NET classes related to ObjectARX.
            4. Return only the C# code, without any extra explanations or comments.

            The user's request is: ";
        }
    }
}
