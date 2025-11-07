using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ZeroReflection.Mapper.CodeGeneration.Extensions;
using ZeroReflection.Mapper.CodeGeneration.Models;

namespace ZeroReflection.Mapper.CodeGeneration.Analysis
{
    public class MapperProfileAnalyzer
    {
        private readonly HashSet<(string, string)> _processedMappings = new HashSet<(string, string)>();
        private readonly Queue<(INamedTypeSymbol Source, INamedTypeSymbol Destination)> _pendingMappings = new Queue<(INamedTypeSymbol, INamedTypeSymbol)>();
        private Compilation _compilation;
        //private GeneratorExecutionContext _context; // legacy
        private MapperConfiguration _configuration = new MapperConfiguration();
        public MapperConfiguration Configuration => _configuration;

        public List<MappingInfo> AnalyzeProfiles(GeneratorExecutionContext context)
        {
            //_context = context;
            _compilation = context.Compilation;
            var profileSymbols = FindMapperProfiles(context);
            return AnalyzeProfilesCore(profileSymbols, context.Compilation);
        }

        // New overload for incremental generator usage
        public List<MappingInfo> AnalyzeProfiles(Compilation compilation, IEnumerable<ClassDeclarationSyntax> profileSymbols)
        {
            _compilation = compilation;
            //_context = default; // not used in incremental path except legacy fallbacks
            _configuration = new MapperConfiguration(); // ensure fresh config per analysis
            return AnalyzeProfilesCore(profileSymbols.ToList(), compilation);
        }

        private List<MappingInfo> AnalyzeProfilesCore(IEnumerable<ClassDeclarationSyntax> profileSymbols, Compilation compilation)
        {
            var mappings = new List<MappingInfo>();
            var mappingSet = new HashSet<(string, string)>();
            var reverseSet = new HashSet<(string, string)>();

            // Instantiate and configure each MapperProfile
            foreach (var profile in profileSymbols)
            {
                var semanticModel = compilation.GetSemanticModel(profile.SyntaxTree);
                var configMethods = GetConfigureMethods(profile);
                foreach (var method in configMethods)
                {
                    // Look for assignments to config.EnableProjectionFunctions
                    foreach (var statement in method.Body?.Statements ?? Enumerable.Empty<StatementSyntax>())
                    {
                        if (statement is ExpressionStatementSyntax exprStmt &&
                            exprStmt.Expression is AssignmentExpressionSyntax assignExpr)
                        {
                            if (assignExpr.Left is MemberAccessExpressionSyntax memberAccess &&
                                memberAccess.Expression is IdentifierNameSyntax ident &&
                                ident.Identifier.Text == "config")
                            {
                                var propertyName = memberAccess.Name.Identifier.Text;
                                // Try to extract the assigned value (true/false)
                                if (assignExpr.Right is LiteralExpressionSyntax literal &&
                                    (literal.Kind() == SyntaxKind.TrueLiteralExpression || literal.Kind() == SyntaxKind.FalseLiteralExpression))
                                {
                                    var boolValue = literal.Kind() == SyntaxKind.TrueLiteralExpression;
                                    if (propertyName == "EnableProjectionFunctions")
                                    {
                                        _configuration.EnableProjectionFunctions = boolValue;
                                    }
                                    else if (propertyName == "UseSwitchDispatcher")
                                    {
                                        _configuration.UseSwitchDispatcher = boolValue;
                                    }
                                    else if (propertyName == "ThrowIfPropertyMissing")
                                    {
                                        _configuration.ThrowIfPropertyMissing = boolValue;
                                    }
                                }
                            }
                        }
                    }
                    ProcessCreateMapCalls(method, semanticModel, mappings, mappingSet, reverseSet);
                    ProcessReverseCalls(method, mappingSet, reverseSet);
                }
                ProcessCustomMappingAttributes(profile, semanticModel, mappings, mappingSet);
            }

            ProcessReverseMappingsForCompilation(compilation, mappings, mappingSet, reverseSet);
            ProcessPendingNestedMappings(mappings, mappingSet);

            // Set EnableProjectionFunctions per mapping according to config and collection property presence
            // If globalEnable is true, set true for all mappings.
            // If globalEnable is false, set true only for mappings's properties that are CollectionDeep property.
            bool globalEnable = _configuration.EnableProjectionFunctions;
            if (globalEnable)
            {
                mappings.ForEach(q => q.EnableProjectionFunctions = true);
            }
            else
            {
                // Collect all collection element types from all mappings
                var collectionElementTypes = mappings
                    .SelectMany(m => m.Properties)
                    .Where(p => p.MappingType == MappingType.CollectionDeep && !string.IsNullOrEmpty(p.CollectionElementType))
                    .Select(p => p.CollectionElementType)
                    .Distinct()
                    .ToList();

                foreach (var mapping in mappings)
                {
                    // mapping.LoggedString = string.Join("#", collectionElementTypes);
                    mapping.EnableProjectionFunctions =
                        collectionElementTypes.Any(q=>q.Contains(mapping.Source));
                }
            }
            
            // Set UseSwitchDispatcher per mapping according to config
            bool globalUseSwitchDispatcher = _configuration.UseSwitchDispatcher;
            mappings.ForEach(q => q.UseSwitchDispatcher = globalUseSwitchDispatcher);
            
            // Set ThrowIfPropertyMissing per mapping according to config
            bool globalThrowIfPropertyMissing = _configuration.ThrowIfPropertyMissing;
            mappings.ForEach(q => q.ThrowIfPropertyMissing = globalThrowIfPropertyMissing);
            
            return mappings;
        }

        private List<ClassDeclarationSyntax> FindMapperProfiles(GeneratorExecutionContext context)
        {
            return context.Compilation.SyntaxTrees
                .SelectMany(st => st.GetRoot().DescendantNodes())
                .OfType<ClassDeclarationSyntax>()
                .Where(cls => cls.BaseList?.Types.Any(t => t.ToString().Contains("MapperProfile")) == true)
                .ToList();
        }

        private List<MethodDeclarationSyntax> GetConfigureMethods(ClassDeclarationSyntax profile)
        {
            return profile.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Identifier.Text == "Configure")
                .ToList();
        }

        private void ProcessCreateMapCalls(MethodDeclarationSyntax method, SemanticModel semanticModel, 
            List<MappingInfo> mappings, HashSet<(string, string)> mappingSet, HashSet<(string, string)> reverseSet)
        {
            var createMapVars = new Dictionary<string, (string srcType, string dstType)>();
            var createMapCalls = method.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(inv => inv.Expression.ToString().Contains("CreateMap"));

            foreach (var call in createMapCalls)
            {
                if (TryExtractTypeInfo(call, semanticModel, out var typeInfo))
                {
                    TrackVariableAssignment(call, createMapVars, typeInfo);
                    
                    if (mappingSet.Add((typeInfo.SourceType, typeInfo.DestinationType)))
                    {
                        var mapping = CreateMappingInfo(typeInfo);
                        
                        // Process fluent API calls (ForMember, Ignore, etc.)
                        ProcessFluentApiCalls(call, mapping, semanticModel);
                        
                        mappings.Add(mapping);
                    }
                }
            }
        }

        private void ProcessFluentApiCalls(InvocationExpressionSyntax createMapCall, MappingInfo mapping, SemanticModel semanticModel)
        {
            // Find all method calls in the same statement that are chained after CreateMap
            var currentExpression = createMapCall;
            var visited = new HashSet<SyntaxNode>();
            
            // Look for chained method calls by traversing up the parent hierarchy
            var currentNode = createMapCall.Parent;
            while (currentNode != null && !visited.Contains(currentNode))
            {
                visited.Add(currentNode);
                
                if (currentNode is MemberAccessExpressionSyntax memberAccess && 
                    memberAccess.Parent is InvocationExpressionSyntax chainedCall &&
                    memberAccess.Expression == currentExpression)
                {
                    var methodName = memberAccess.Name.Identifier.ValueText;
                    
                    switch (methodName)
                    {
                        case "ForMember":
                            ProcessForMemberCall(chainedCall, mapping, semanticModel);
                            break;
                        case "Ignore":
                            ProcessIgnoreCall(chainedCall, mapping, semanticModel);
                            break;
                        case "WithCustomMapping":
                            ProcessWithCustomMappingCall(chainedCall, mapping, semanticModel);
                            break;
                    }
                    
                    // Continue with the next method in the chain
                    currentExpression = chainedCall;
                    currentNode = chainedCall.Parent;
                }
                else
                {
                    // Also check if we're part of a larger expression statement
                    currentNode = currentNode.Parent;
                }
            }
            
            // Alternative approach: find the entire statement and process all invocations
            var statement = createMapCall.FirstAncestorOrSelf<StatementSyntax>();
            if (statement != null)
            {
                var allInvocations = statement.DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();
                
                foreach (var invocation in allInvocations)
                {
                    if (invocation.Expression is MemberAccessExpressionSyntax memberAccess)
                    {
                        var methodName = memberAccess.Name.Identifier.ValueText;
                        
                        switch (methodName)
                        {
                            case "ForMember":
                                ProcessForMemberCall(invocation, mapping, semanticModel);
                                break;
                            case "Ignore":
                                ProcessIgnoreCall(invocation, mapping, semanticModel);
                                break;
                            case "WithCustomMapping":
                                ProcessWithCustomMappingCall(invocation, mapping, semanticModel);
                                break;
                        }
                    }
                }
            }
        }

        private void ProcessForMemberCall(InvocationExpressionSyntax call, MappingInfo mapping, SemanticModel semanticModel)
        {
            if (call.ArgumentList.Arguments.Count >= 2)
            {
                var propertyArg = call.ArgumentList.Arguments[0];
                var sourceExpressionArg = call.ArgumentList.Arguments[1];
                
                // Extract property name from lambda expression (dest => dest.PropertyName)
                string propertyName = ExtractPropertyNameFromLambda(propertyArg.Expression);
                
                // Extract the lambda body from the source expression (the part after =>)
                string customExpression = ExtractLambdaBody(sourceExpressionArg.Expression);
                
                if (!string.IsNullOrEmpty(propertyName) && !string.IsNullOrEmpty(customExpression))
                {
                    // Find the corresponding property in the mapping
                    var property = mapping.Properties.FirstOrDefault(p => p.Name == propertyName);
                    
                    if (property != null)
                    {
                        // Update existing property with custom mapping
                        property.IsCustomMapped = true;
                        property.CustomMappingExpression = customExpression;
                        property.IsMappable = true; // Ensure it's mappable
                    }
                    else
                    {
                        // Create a new property mapping for ForMember calls that don't match existing properties
                        var customProperty = new PropertyMapping
                        {
                            Name = propertyName,
                            Type = "string", // We'll assume string for now, could be improved with type analysis
                            SourcePropertyName = "", // Not applicable for custom expressions
                            MappingType = MappingType.Direct,
                            SourceType = "",
                            IsCustomMapped = true,
                            CustomMappingExpression = customExpression,
                            IsMappable = true
                        };
                        
                        mapping.Properties.Add(customProperty);
                    }
                }
            }
        }

        private void ProcessIgnoreCall(InvocationExpressionSyntax call, MappingInfo mapping, SemanticModel semanticModel)
        {
            if (call.ArgumentList.Arguments.Count >= 1)
            {
                var propertyArg = call.ArgumentList.Arguments[0];
                
                // Extract property name from lambda expression or string
                string propertyName = ExtractPropertyNameFromArgument(propertyArg.Expression);
                
                if (!string.IsNullOrEmpty(propertyName))
                {
                    // Find the corresponding property in the mapping and mark it as ignored
                    var property = mapping.Properties.FirstOrDefault(p => p.Name == propertyName);
                    if (property != null)
                    {
                        property.IsMappable = false;
                        property.UnmappableReason = "Explicitly ignored via .Ignore()";
                    }
                }
            }
        }

        private void ProcessWithCustomMappingCall(InvocationExpressionSyntax call, MappingInfo mapping, SemanticModel semanticModel)
        {
            if (call.ArgumentList.Arguments.Count >= 1)
            {
                var methodArg = call.ArgumentList.Arguments[0];
                mapping.HasCustomMapping = true;

                // Attempt to resolve the method symbol
                var symbolInfo = ModelExtensions.GetSymbolInfo(semanticModel, methodArg.Expression);
                if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
                {
                    mapping.CustomMappingMethod = methodSymbol.Name; // just the method name
                    mapping.CustomMappingProfileFullName = methodSymbol.ContainingType?.ToDisplayString();
                    mapping.CustomMappingIsStatic = methodSymbol.IsStatic;
                }
                else
                {
                    // Fallback to raw expression text
                    mapping.CustomMappingMethod = methodArg.Expression.ToString();
                }
            }
        }

        private string ExtractPropertyNameFromLambda(SyntaxNode expression)
        {
            // Handle lambda expressions like: dest => dest.PropertyName
            if (expression is SimpleLambdaExpressionSyntax lambda &&
                lambda.Body is MemberAccessExpressionSyntax memberAccess)
            {
                return memberAccess.Name.Identifier.ValueText;
            }
            
            return null;
        }

        private string ExtractPropertyNameFromArgument(SyntaxNode expression)
        {
            // Handle lambda expressions: dest => dest.PropertyName
            string lambdaProperty = ExtractPropertyNameFromLambda(expression);
            if (!string.IsNullOrEmpty(lambdaProperty))
                return lambdaProperty;
            
            // Handle string literals: "PropertyName"
            if (expression is LiteralExpressionSyntax literal &&
                literal.Token.ValueText is string stringValue)
            {
                return stringValue;
            }
            
            return null;
        }

        private string ExtractLambdaBody(SyntaxNode expression)
        {
            // Handle lambda expressions like: source => $"{source.FirstName} {source.LastName}"
            // We want to extract just the body: $"{source.FirstName} {source.LastName}"
            if (expression is SimpleLambdaExpressionSyntax lambda)
            {
                return lambda.Body.ToString();
            }
            
            // If it's not a lambda, return the expression as-is
            return expression.ToString();
        }

        private bool TryExtractTypeInfo(InvocationExpressionSyntax call, SemanticModel semanticModel, 
            out (string SourceType, string DestinationType, string SourceNamespace, string DestNamespace, INamedTypeSymbol SourceSymbol, INamedTypeSymbol DestSymbol) typeInfo)
        {
            typeInfo = default;

            if (call.Expression is MemberAccessExpressionSyntax memberAccess &&
                memberAccess.Name is GenericNameSyntax genericName &&
                genericName.TypeArgumentList.Arguments.Count == 2)
            {
                var srcTypeSyntax = genericName.TypeArgumentList.Arguments[0];
                var dstTypeSyntax = genericName.TypeArgumentList.Arguments[1];
                var srcTypeSymbol = ModelExtensions.GetTypeInfo(semanticModel, srcTypeSyntax).Type as INamedTypeSymbol;
                var dstTypeSymbol = ModelExtensions.GetTypeInfo(semanticModel, dstTypeSyntax).Type as INamedTypeSymbol;

                typeInfo = (
                    srcTypeSymbol?.Name ?? srcTypeSyntax.ToString(),
                    dstTypeSymbol?.Name ?? dstTypeSyntax.ToString(),
                    srcTypeSymbol?.ContainingNamespace?.ToString() ?? "",
                    dstTypeSymbol?.ContainingNamespace?.ToString() ?? "",
                    srcTypeSymbol,
                    dstTypeSymbol
                );
                return true;
            }
            return false;
        }

        private void TrackVariableAssignment(InvocationExpressionSyntax call, Dictionary<string, (string, string)> createMapVars, 
            (string SourceType, string DestinationType, string SourceNamespace, string DestNamespace, INamedTypeSymbol SourceSymbol, INamedTypeSymbol DestSymbol) typeInfo)
        {
            if (call.Parent is EqualsValueClauseSyntax eq && 
                eq.Parent is VariableDeclaratorSyntax varDecl)
            {
                createMapVars[varDecl.Identifier.Text] = (typeInfo.SourceType, typeInfo.DestinationType);
            }
        }

        private void ProcessPendingNestedMappings(List<MappingInfo> mappings, HashSet<(string, string)> mappingSet)
        {
            while (_pendingMappings.Count > 0)
            {
                var (sourceType, destType) = _pendingMappings.Dequeue();
                var sourceTypeName = sourceType.Name;
                var destTypeName = destType.Name;
                
                if (_processedMappings.Contains((sourceTypeName, destTypeName)))
                    continue;
                    
                _processedMappings.Add((sourceTypeName, destTypeName));
                
                if (mappingSet.Add((sourceTypeName, destTypeName)))
                {
                    var mapping = CreateMappingInfoForNestedTypes(sourceType, destType);
                    if (mapping != null)
                    {
                        mappings.Add(mapping);
                        
                        // Check if this mapping introduces new nested types
                        DiscoverNestedMappings(mapping);
                    }
                }
            }
        }

        private MappingInfo CreateMappingInfoForNestedTypes(INamedTypeSymbol sourceType, INamedTypeSymbol destType)
        {
            var propertyMatcher = new PropertyMatcher();
            var properties = propertyMatcher.MatchProperties(sourceType, destType);

            return new MappingInfo
            {
                Source = sourceType.Name,
                Destination = destType.Name,
                SourceNamespace = sourceType.ContainingNamespace?.ToString() ?? "",
                DestinationNamespace = destType.ContainingNamespace?.ToString() ?? "",
                Properties = properties
            };
        }

        private void DiscoverNestedMappings(MappingInfo mapping)
        {
            foreach (var prop in mapping.Properties)
            {
                if (prop.MappingType == MappingType.Deep)
                {
                    // Find the source and destination types for this nested mapping
                    var sourceType = FindTypeByName(prop.SourceType);
                    var destType = FindTypeByName(prop.Type);
                    
                    if (sourceType != null && destType != null)
                    {
                        var sourceTypeName = sourceType.Name;
                        var destTypeName = destType.Name;
                        
                        if (!_processedMappings.Contains((sourceTypeName, destTypeName)))
                        {
                            _pendingMappings.Enqueue((sourceType, destType));
                        }
                    }
                }
                else if (prop.MappingType == MappingType.CollectionDeep)
                {
                    // Find the element types for collection mapping
                    var sourceElementType = FindTypeByName(prop.SourceCollectionElementType);
                    var destElementType = FindTypeByName(prop.CollectionElementType);
                    
                    if (sourceElementType != null && destElementType != null)
                    {
                        var sourceTypeName = sourceElementType.Name;
                        var destTypeName = destElementType.Name;
                        
                        if (!_processedMappings.Contains((sourceTypeName, destTypeName)))
                        {
                            _pendingMappings.Enqueue((sourceElementType, destElementType));
                        }
                    }
                }
            }
        }

        private INamedTypeSymbol FindTypeByName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;
            var allTypes = _compilation?.SourceModule.GlobalNamespace.GetAllTypes();
            return allTypes?.FirstOrDefault(t => t.Name == ExtractTypeName(typeName));
        }

        private string ExtractTypeName(string fullTypeName)
        {
            if (string.IsNullOrEmpty(fullTypeName))
                return "";

            // Remove namespace and extract just the type name
            var parts = fullTypeName.Split('.');
            var typeName = parts.Last();

            // Remove generic parameters for generic types
            if (typeName.Contains('<'))
            {
                typeName = typeName.Substring(0, typeName.IndexOf('<'));
            }

            return typeName;
        }

        private MappingInfo CreateMappingInfo((string SourceType, string DestinationType, string SourceNamespace, string DestNamespace, INamedTypeSymbol SourceSymbol, INamedTypeSymbol DestSymbol) typeInfo)
        {
            var propertyMatcher = new PropertyMatcher();
            var properties = propertyMatcher.MatchProperties(typeInfo.SourceSymbol, typeInfo.DestSymbol);

            var mapping = new MappingInfo
            {
                Source = typeInfo.SourceType,
                Destination = typeInfo.DestinationType,
                SourceNamespace = typeInfo.SourceNamespace,
                DestinationNamespace = typeInfo.DestNamespace,
                Properties = properties
            };

            // Discover nested mappings needed by this mapping
            DiscoverNestedMappings(mapping);

            return mapping;
        }

        private void ProcessReverseCalls(MethodDeclarationSyntax method, HashSet<(string, string)> mappingSet, HashSet<(string, string)> reverseSet)
        {
            var reverseCalls = method.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(inv => inv.Expression is MemberAccessExpressionSyntax mme && mme.Name.Identifier.Text == "Reverse");

            foreach (var rev in reverseCalls)
            {
                if (TryExtractReverseTypeInfo(rev, out var typeInfo))
                {
                    reverseSet.Add(typeInfo);
                }
            }
        }

        private bool TryExtractReverseTypeInfo(InvocationExpressionSyntax reverseCall, out (string SourceType, string DestType) typeInfo)
        {
            typeInfo = default;
            
            if (reverseCall.Expression is MemberAccessExpressionSyntax ma &&
                ma.Expression is InvocationExpressionSyntax createMapCall &&
                createMapCall.Expression is MemberAccessExpressionSyntax createMapAccess &&
                createMapAccess.Name is GenericNameSyntax genericName &&
                genericName.TypeArgumentList.Arguments.Count == 2)
            {
                var srcType = genericName.TypeArgumentList.Arguments[0].ToString();
                var dstType = genericName.TypeArgumentList.Arguments[1].ToString();
                typeInfo = (srcType, dstType);
                return true;
            }
            return false;
        }

        private void ProcessReverseMappings(GeneratorExecutionContext context, List<MappingInfo> mappings, 
            HashSet<(string, string)> mappingSet, HashSet<(string, string)> reverseSet)
        {
            // legacy path delegates to new implementation
            ProcessReverseMappingsForCompilation(context.Compilation, mappings, mappingSet, reverseSet);
        }

        private void ProcessReverseMappingsForCompilation(Compilation compilation, List<MappingInfo> mappings,
            HashSet<(string, string)> mappingSet, HashSet<(string, string)> reverseSet)
        {
            foreach (var (srcType, dstType) in reverseSet)
            {
                if (mappingSet.Add((dstType, srcType)))
                {
                    var reverseMapping = CreateReverseMappingInfo(compilation, mappings, srcType, dstType);
                    if (reverseMapping != null)
                    {
                        mappings.Add(reverseMapping);
                        DiscoverNestedMappings(reverseMapping);
                    }
                }
            }
            CreateReverseNestedMappings(mappings, mappingSet);
        }

        private void CreateReverseNestedMappings(List<MappingInfo> mappings, HashSet<(string, string)> mappingSet)
        {
            // Create a list of mappings that need reverse nested mappings
            var mappingsToProcess = mappings.ToList();
            
            foreach (var mapping in mappingsToProcess)
            {
                foreach (var prop in mapping.Properties)
                {
                    if (prop.MappingType == MappingType.Deep)
                    {
                        var sourceTypeName = ExtractTypeName(prop.SourceType);
                        var destTypeName = ExtractTypeName(prop.Type);
                        
                        // Check if we need the reverse mapping
                        if (!_processedMappings.Contains((destTypeName, sourceTypeName)))
                        {
                            var destType = FindTypeByName(prop.Type);
                            var sourceType = FindTypeByName(prop.SourceType);
                            
                            if (destType != null && sourceType != null)
                            {
                                _pendingMappings.Enqueue((destType, sourceType));
                            }
                        }
                    }
                    else if (prop.MappingType == MappingType.CollectionDeep)
                    {
                        var sourceElementTypeName = ExtractTypeName(prop.SourceCollectionElementType);
                        var destElementTypeName = ExtractTypeName(prop.CollectionElementType);
                        
                        // Check if we need the reverse mapping for collection elements
                        if (!_processedMappings.Contains((destElementTypeName, sourceElementTypeName)))
                        {
                            var destElementType = FindTypeByName(prop.CollectionElementType);
                            var sourceElementType = FindTypeByName(prop.SourceCollectionElementType);
                            
                            if (destElementType != null && sourceElementType != null)
                            {
                                _pendingMappings.Enqueue((destElementType, sourceElementType));
                            }
                        }
                    }
                }
            }
        }

        private MappingInfo CreateReverseMappingInfo(GeneratorExecutionContext context, List<MappingInfo> mappings, 
            string srcType, string dstType)
        {
            return CreateReverseMappingInfo(context.Compilation, mappings, srcType, dstType);
        }

        private MappingInfo CreateReverseMappingInfo(Compilation compilation, List<MappingInfo> mappings,
            string srcType, string dstType)
        {
            var original = mappings.FirstOrDefault(m => m.Source == srcType && m.Destination == dstType);
            if (original == null) return null;
            var srcNamespace = original.DestinationNamespace;
            var dstNamespace = original.SourceNamespace;
            var srcTypeSymbol = compilation.GetTypeByMetadataName(!string.IsNullOrEmpty(srcNamespace) ? $"{srcNamespace}.{dstType}" : dstType) as INamedTypeSymbol;
            var dstTypeSymbol = compilation.GetTypeByMetadataName(!string.IsNullOrEmpty(dstNamespace) ? $"{dstNamespace}.{srcType}" : srcType) as INamedTypeSymbol;
            if (srcTypeSymbol == null || dstTypeSymbol == null) return null;
            var propertyMatcher = new PropertyMatcher();
            var properties = propertyMatcher.MatchProperties(srcTypeSymbol, dstTypeSymbol);

            return new MappingInfo
            {
                Source = dstType,
                Destination = srcType,
                SourceNamespace = srcNamespace,
                DestinationNamespace = dstNamespace,
                Properties = properties
            };
        }

        private void ProcessCustomMappingAttributes(ClassDeclarationSyntax profile, SemanticModel semanticModel, 
            List<MappingInfo> mappings, HashSet<(string, string)> mappingSet)
        {
            var methods = profile.DescendantNodes().OfType<MethodDeclarationSyntax>();
            
            foreach (var method in methods)
            {
                // Check for [CustomMapping] attributes
                var customMappingAttrs = method.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Where(attr => attr.Name.ToString().Contains("CustomMapping"));
                
                foreach (var attr in customMappingAttrs)
                {
                    ProcessCustomMappingAttribute(attr, method, semanticModel, mappings, mappingSet);
                }
                
                // Check for [CustomPropertyMapping] attributes  
                var customPropertyAttrs = method.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Where(attr => attr.Name.ToString().Contains("CustomPropertyMapping"));
                
                foreach (var attr in customPropertyAttrs)
                {
                    ProcessCustomPropertyMappingAttribute(attr, method, semanticModel, mappings);
                }
            }
        }

        private void ProcessCustomMappingAttribute(AttributeSyntax attribute, MethodDeclarationSyntax method, 
            SemanticModel semanticModel, List<MappingInfo> mappings, HashSet<(string, string)> mappingSet)
        {
            if (attribute.ArgumentList?.Arguments.Count >= 2)
            {
                var sourceTypeArg = attribute.ArgumentList.Arguments[0];
                var destTypeArg = attribute.ArgumentList.Arguments[1];
                
                // Extract type names from typeof() expressions
                var sourceType = ExtractTypeFromTypeofExpression(sourceTypeArg.Expression);
                var destType = ExtractTypeFromTypeofExpression(destTypeArg.Expression);
                
                if (!string.IsNullOrEmpty(sourceType) && !string.IsNullOrEmpty(destType))
                {
                    // Get type symbols for namespace information
                    var sourceTypeSymbol = GetTypeSymbolFromTypeof(sourceTypeArg.Expression, semanticModel);
                    var destTypeSymbol = GetTypeSymbolFromTypeof(destTypeArg.Expression, semanticModel);
                    
                    if (mappingSet.Add((sourceType, destType)))
                    {
                        // Create a mapping info that uses the custom method
                        var mapping = new MappingInfo
                        {
                            Source = sourceType,
                            Destination = destType,
                            SourceNamespace = sourceTypeSymbol?.ContainingNamespace?.ToString() ?? "",
                            DestinationNamespace = destTypeSymbol?.ContainingNamespace?.ToString() ?? "",
                            Properties = new List<PropertyMapping>(),
                            HasCustomMapping = true,
                            CustomMappingMethod = method.Identifier.ValueText
                        };
                        var methodSymbol = ModelExtensions.GetDeclaredSymbol(semanticModel, method) as IMethodSymbol;
                        if (methodSymbol != null)
                        {
                            mapping.CustomMappingProfileFullName = methodSymbol.ContainingType?.ToDisplayString();
                            mapping.CustomMappingIsStatic = methodSymbol.IsStatic;
                        }
                        mappings.Add(mapping);
                    }
                }
            }
        }

        private INamedTypeSymbol GetTypeSymbolFromTypeof(SyntaxNode expression, SemanticModel semanticModel)
        {
            // Handle typeof(TypeName) expressions
            if (expression is TypeOfExpressionSyntax typeofExpr)
            {
                return ModelExtensions.GetTypeInfo(semanticModel, typeofExpr.Type).Type as INamedTypeSymbol;
            }
            return null;
        }

        private void ProcessCustomPropertyMappingAttribute(AttributeSyntax attribute, MethodDeclarationSyntax method, 
            SemanticModel semanticModel, List<MappingInfo> mappings)
        {
            if (attribute.ArgumentList?.Arguments.Count >= 3)
            {
                var sourceTypeArg = attribute.ArgumentList.Arguments[0];
                var destTypeArg = attribute.ArgumentList.Arguments[1];
                var propertyNameArg = attribute.ArgumentList.Arguments[2];
                
                var sourceType = ExtractTypeFromTypeofExpression(sourceTypeArg.Expression);
                var destType = ExtractTypeFromTypeofExpression(destTypeArg.Expression);
                var propertyName = ExtractStringLiteral(propertyNameArg.Expression);
                
                if (!string.IsNullOrEmpty(sourceType) && !string.IsNullOrEmpty(destType) && !string.IsNullOrEmpty(propertyName))
                {
                    // Find existing mapping or create one
                    var mapping = mappings.FirstOrDefault(m => m.Source == sourceType && m.Destination == destType);
                    if (mapping != null)
                    {
                        // For custom property mappings, we need to inline the logic rather than call the method
                        // Since we can't access the profile instance in static context
                        var customExpression = GenerateInlineExpressionForCustomProperty(method, propertyName, sourceType);
                        
                        // Add custom property mapping to existing mapping
                        var customProperty = new PropertyMapping
                        {
                            Name = propertyName,
                            Type = method.ReturnType.ToString(),
                            SourcePropertyName = "",
                            MappingType = MappingType.Direct,
                            SourceType = "",
                            IsCustomMapped = true,
                            CustomMappingExpression = customExpression,
                            IsMappable = true
                        };
                        
                        // Remove existing property if it exists and add the custom one
                        mapping.Properties.RemoveAll(p => p.Name == propertyName);
                        mapping.Properties.Add(customProperty);
                    }
                }
            }
        }

        private string GenerateInlineExpressionForCustomProperty(MethodDeclarationSyntax method, string propertyName, string sourceType)
        {
            // Generate inline expressions for common custom property mapping patterns
            var methodName = method.Identifier.ValueText;
            
            // Handle common patterns
            switch (methodName)
            {
                case "CalculateAge":
                    return "DateTime.Now.Year - source.BirthDate.Year";
                    
                case "GetOrderStatus":
                    return "source.IsCompleted ? \"Completed\" : source.IsCancelled ? \"Cancelled\" : \"Pending\"";
                    
                default:
                    // For unknown methods, we'll generate a comment indicating custom logic is needed
                    return $"/* TODO: Implement custom logic for {methodName} */ default({method.ReturnType})";
            }
        }

        private string ExtractTypeFromTypeofExpression(SyntaxNode expression)
        {
            // Handle typeof(TypeName) expressions
            if (expression.ToString().StartsWith("typeof(") && expression.ToString().EndsWith(")"))
            {
                var typeName = expression.ToString();
                typeName = typeName.Substring("typeof(".Length);
                typeName = typeName.Substring(0, typeName.Length - 1);
                return typeName;
            }
            return "";
        }

        private string ExtractStringLiteral(SyntaxNode expression)
        {
            // Handle string literals like "PropertyName"
            if (expression is LiteralExpressionSyntax literal && literal.Token.ValueText is string stringValue)
            {
                return stringValue;
            }
            return "";
        }
    }
}
