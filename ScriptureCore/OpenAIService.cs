﻿using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace ScriptureCore
{
    public class OpenAIService
    {
        private readonly ChatClient _initialScriptClient;

        public OpenAIService(IConfiguration configuration)
        {
            string? apiKey = configuration["InitialScriptModel:ApiKey"];
            string? endpoint = configuration["InitialScriptModel:Endpoint"];
            string? deploymentName = configuration["InitialScriptModel:DeploymentName"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentNullException(nameof(apiKey), "API key must not be null or empty.");
            }
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new ArgumentNullException(nameof(endpoint), "Endpoint must not be null or empty.");
            }
            if (string.IsNullOrEmpty(deploymentName))
            {
                throw new ArgumentNullException(nameof(deploymentName), "Deployment name must not be null or empty.");
            }

            _initialScriptClient = new AzureOpenAIClient(
                new Uri(endpoint), 
                new AzureKeyCredential(apiKey)).GetChatClient(deploymentName);
        }

        public async Task<string> GenerateScriptAsync(string prompt)
        {
            ChatCompletionOptions options = new()
            {
                MaxOutputTokenCount = 1024,
                Temperature = 0.3f,
                //StopSequences = new[] { "###" }
            };

            var messages = new ChatMessage[]
            {
                new SystemChatMessage(GenerateSystemMessage()),
                new UserChatMessage(prompt),
            };

            var completionsResponse = await _initialScriptClient.CompleteChatAsync(messages, options);
            return string.Join("", completionsResponse.Value.Content.Select(c => c.Text));
        }

        private string GenerateSystemMessage()
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
