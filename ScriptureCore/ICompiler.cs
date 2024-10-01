namespace ScriptureCore
{
    public interface ICompiler
    {
        (bool Success, List<string> Errors) TestCompile(string code);

        (bool Success, List<string> Errors) CompileTo(string code, string filepath);

        string GetFullyQualifiedTypeName(string typeName, string code);

    }
}
