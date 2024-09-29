namespace ScriptureCore
{
    public interface ICompiler
    {
        (bool Success, List<string> Errors) TestCompile(string code);
    }
}
