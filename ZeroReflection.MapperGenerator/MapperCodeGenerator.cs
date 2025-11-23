using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using ZeroReflection.MapperGenerator.Emit;
using ZeroReflection.MapperGenerator.Models;
using ZeroReflection.MapperGenerator.Utils;

namespace ZeroReflection.MapperGenerator
{
    public class MapperCodeGenerator
    {
        public void Generate(SourceProductionContext spc,
            Compilation compilation,
            List<MappingInfo> mappings)
        {
            if (!mappings.Any()) return;

            // Service registration
            spc.AddSource("RegisterZeroReflectionMapping.g.cs", Microsoft.CodeAnalysis.Text.SourceText.From(ServiceRegistrationEmitter.Build(mappings), System.Text.Encoding.UTF8));

            // Mapping handlers
            var uniqueMappings = CodeGenUtils.GetUniquePairs(mappings);
            foreach (var (source, destination) in uniqueMappings)
            {
                var mapping = mappings.FirstOrDefault(m => m.Source == source && m.Destination == destination);
                var handlerSource = MappingHandlerEmitter.Build(source, destination, mapping, mappings);
                spc.AddSource($"Map{source}To{destination}.g.cs", Microsoft.CodeAnalysis.Text.SourceText.From(handlerSource, System.Text.Encoding.UTF8));
            }

            // Dispatcher - decide using configuration flag; fall back to simple chain if mixed
            bool useSwitch = mappings.Any() && mappings.First().UseSwitchDispatcher;
            spc.AddSource("GeneratedMappingDispatcher.g.cs", Microsoft.CodeAnalysis.Text.SourceText.From(DispatcherEmitter.Build(mappings), System.Text.Encoding.UTF8));
        }
    }
}
