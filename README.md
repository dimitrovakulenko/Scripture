# SCRIPTURE - LLM-Based Automation Plugin for AutoCAD

SCRIPTURE is a powerful automation tool for AutoCAD that allows users to automate their workflows by simply describing the actions to be automated with text. 

This tool is currently a prototype and is designed for research and experimentation.

## Example

![Demo of the Plugin: create random entities](.assets/readme_example_1_create_ents.gif)

![Demo of the Plugin: move all rects to a new layer](.assets/readme_example_2_move_to_layer.gif)

## Prerequisites

- **AutoCAD** (or similar): Make sure AutoCAD or similar application supporting running .net8 objectARX plugins is installed on your machine
- **Access to an LLM**: Requires an API key and endpoint for either:
  - **Microsoft Azure OpenAI API**
  - **OpenAI API** (not tested yet)

## How to Try It

### Install or Compile

You have two options to get started with SCRIPTURE:

1. **Compile Locally**  
   Clone the repository and build the solution locally using dotnet tool.

2. **Install via MSI or Bundle**
   
   - **MSI Installer**: Use the MSI installer to install the plugin. You need to ensure .NET 8 runtime is already installed.
   - **Bundle Installer**: Use the bundle (`bundle.exe`) to install both the .NET 8 runtime (if not installed) and the plugin.

Please use next link: https://github.com/dimitrovakulenko/Scripture/releases to download an installer.

### Configure `appSettings.json`

You can find 'appSettings.json'  file in %PROGRAMFILES% folder in case of using installer and in the bin folder of scripture project in case of local build.
To use SCRIPTURE, you need to configure the `appSettings.json` file.

- **Azure OpenAI API Configuration**:
  
  ```json
  {
    "InitialScriptModel": {
      "ApiKey": "YOUR_AZURE_OPENAI_API_KEY",
      "Endpoint": "https://YOUR_AZURE_ENDPOINT.openai.azure.com/",
      "ModelName": "YOUR_MODEL_DEPLOYMENT_NAME"
    }
  }
  ```
  
  - Set ApiKey to your Azure OpenAI API key.
  - Set Endpoint to your Azure OpenAI resource endpoint URL.
  - Set ModelName to your Azure deployment name.

- **OpenAI API Configuration**:
  
  ```json
  {
    "InitialScriptModel": {
      "ApiKey": "YOUR_OPENAI_API_KEY",
      "Endpoint": "",
      "ModelName": "CHOSEN_MODEL_NAME"
    }
  }
  ```
  
  - Set ApiKey to your OpenAI API key.
  - Set ModelName to the desired model (e.g., gpt-4o).
  - Keep Endpoint empty.
