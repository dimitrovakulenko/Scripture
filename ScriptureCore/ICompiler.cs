namespace ScriptureCore
{
    public interface ICompiler
    {
        (bool Success, List<string> Errors) TestCompile(string code);

        (bool Success, List<string> Errors, string DllPath) CompileToTemporaryFile(string code);
    }
}
