namespace ScriptureCore
{
    public interface ILLMServices
    {
        Task<string> GenerateInitialScriptAsync(string prompt);
    }
}
