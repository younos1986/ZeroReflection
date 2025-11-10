using System.Collections.Generic;
using System.Linq;
using ZeroReflection.Mapper.CodeGeneration.Models;

namespace ZeroReflection.Mapper.CodeGeneration.Utils
{
    internal static class CodeGenUtils
    {
        public static string Qualify(string ns, string type)
            => string.IsNullOrWhiteSpace(ns) ? $"global::{type}" : $"global::{ns}.{type}";

        public static HashSet<string> GetRequiredNamespaces(List<MappingInfo> mappings)
        {
            var namespaces = new HashSet<string>();
            foreach (var mapping in mappings)
            {
                if (!string.IsNullOrEmpty(mapping.SourceNamespace))
                    namespaces.Add(mapping.SourceNamespace);
                if (!string.IsNullOrEmpty(mapping.DestinationNamespace))
                    namespaces.Add(mapping.DestinationNamespace);
            }
            return namespaces;
        }

        public static List<(string Source, string Destination)> GetUniquePairs(List<MappingInfo> mappings)
            => mappings.Select(m => (m.Source, m.Destination)).Distinct().ToList();

        public static string ExtractTypeName(string fullTypeName)
        {
            if (string.IsNullOrEmpty(fullTypeName))
                return string.Empty;
            var parts = fullTypeName.Split('.');
            var typeName = parts.Last();
            if (typeName.Contains('<'))
                typeName = typeName.Substring(0, typeName.IndexOf('<'));
            return typeName;
        }
    }
}
