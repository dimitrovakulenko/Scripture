namespace ScriptureCore
{
    public interface ILLMServices
    {
        Task<string> GenerateInitialScriptAsync(string prompt);

        Task<string> TryFixScriptAsync(string script, List<string> errorMessages);
    }
}
