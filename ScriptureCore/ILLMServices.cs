namespace ScriptureCore
{
    public interface ILLMServices
    {
        Task<string> GenerateInitialScriptAsync(string prompt);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="script"></param>
        /// <param name="errorMessages"></param>
        /// <param name="provideAdditionalMetadata">additional data about available properties/methods is provided in some cases</param>
        /// <returns></returns>
        Task<string> TryFixScriptAsync(string script, List<string> errorMessages, bool provideAdditionalMetadata);
    }
}
