using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using ZeroReflection.Mapper.CodeGeneration.Analysis;

namespace ZeroReflection.Mapper.CodeGeneration
{
    [Generator]
    public class MapperGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Select candidate class declarations that might derive from MapperProfile
            var profileClasses = context.SyntaxProvider
                .CreateSyntaxProvider(static (node, _) => node is ClassDeclarationSyntax cds && cds.BaseList != null,
                    static (ctx, _) =>
                    {
                        var cds = (ClassDeclarationSyntax)ctx.Node;
                        if (cds.BaseList?.Types.Any(t => t.ToString().Contains("MapperProfile")) == true)
                            return cds;
                        return null;
                    })
                .Where(static c => c is not null)!;

            var collectedProfiles = profileClasses.Collect();

            // Combine compilation with collected profile class declarations
            var combined = context.CompilationProvider.Combine(collectedProfiles);

            context.RegisterSourceOutput(combined, static (spc, source) =>
            {
                var compilation = source.Left;
                var profiles = source.Right!; // ImmutableArray<ClassDeclarationSyntax?>
                if (profiles.Length == 0) return;

                var nonNullProfiles = profiles.Where(p => p is not null)!.Cast<ClassDeclarationSyntax>();
                var analyzer = new MapperProfileAnalyzer();
                var mappings = analyzer.AnalyzeProfiles(compilation, nonNullProfiles);
                if (mappings.Count == 0) return;
                var generator = new MapperCodeGenerator();
                generator.Generate(spc, compilation, mappings);
            });
        }
    }
}
