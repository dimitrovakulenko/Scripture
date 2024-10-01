using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ScriptureCore
{
    internal class RuntimeCompiler: ICompiler
    {
        public (bool Success, List<string> Errors) TestCompile(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            var references = GetReferences();

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

        private List<MetadataReference> GetReferences()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            return loadedAssemblies
                .Where(assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
                .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
                .ToList<MetadataReference>();
        }

        public (bool Success, List<string> Errors) CompileTo(string code, string filepath)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);

            var references = GetReferences();

            var compilation = CSharpCompilation.Create(Path.GetFileNameWithoutExtension(filepath))
                .AddReferences(references)
                .AddSyntaxTrees(syntaxTree)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var fs = new FileStream(filepath, FileMode.Create))
            {
                var result = compilation.Emit(fs);

                var errors = result.Diagnostics
                    .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                    .Select(diagnostic => diagnostic.GetMessage())
                    .ToList();

                return (result.Success, errors);
            }
        }

        public string GetFullyQualifiedTypeName(string typeName, string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = syntaxTree.GetRoot();

            var compilation = CSharpCompilation.Create("TempCompilation")
                .AddSyntaxTrees(syntaxTree)
                .AddReferences(
                    GetReferences());
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var typeNodes = syntaxTree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>();
            foreach (var typeNode in typeNodes)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(typeNode);
                if (symbolInfo.Symbol != null && symbolInfo.Symbol.Name == typeName)
                {
                    var typeSymbol = symbolInfo.Symbol as ITypeSymbol;
                    if (typeSymbol != null)
                    {
                        return typeSymbol.ToDisplayString();
                    }
                }
            }
            return typeName;
        }
    }
}