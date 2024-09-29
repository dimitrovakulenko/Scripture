using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;

namespace ScriptureCore
{
    internal class RuntimeCompiler: ICompiler
    {
        public (bool Success, List<string> Errors) TestCompile(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            var references = loadedAssemblies
                .Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
                .ToList();

            var compilation = CSharpCompilation.Create("TestCompilation")
                .AddReferences(references)
                .AddSyntaxTrees(syntaxTree)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var result = compilation.Emit(Stream.Null);

            var errors = result.Diagnostics
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                .Select(diagnostic => diagnostic.GetMessage())
                .ToList();

            return (result.Success, errors);
        }
    }
}