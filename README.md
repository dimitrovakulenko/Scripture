# SCRIPTURE - LLM-Based Automation Plugin for AutoCAD

SCRIPTURE is a powerful automation tool for AutoCAD that allows users to automate their workflows by simply describing the actions to be automated with text. 
This tool is currently a prototype and is designed for research and experimentation.

## How to use it?

Either compile the code or use M
SCRIPTURE follows a straightforward workflow to automate AutoCAD actions from text input:

1. **Generate Script from Text**  
   Provide a textual description of what you want to automate. SCRIPTURE will use Azure OpenAI or OpenAI's GPT-4 to generate the corresponding automation script.

2. **Automatic Error Fixing**  
   If the generated script contains errors, SCRIPTURE iteratively refines and fixes the issues, ensuring successful compilation. Reflection is used to make necessary adjustments.

3. **Run or Create Plugin**  
   The generated automation can either be executed directly within AutoCAD or compiled into a plugin for repeated use. This enables both on-the-fly automation and reusable solutions.

## Getting Started

### Prerequisites

- **Visual Studio** (with C# and WPF support)
- **AutoCAD** with ObjectARX.NET API
- **API Key and Endpoint** for **OpenAI** or **Azure OpenAI**

### License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

