# ZeroReflection.MapperGenerator

This package contains the source generator for ZeroReflection.Mapper. It analyzes `MapperProfile` classes at compile time and generates efficient, reflection-free mapping code using Roslyn code analysis.

## Installation

This package is typically referenced automatically when you install `ZeroReflection.Mapper`. You don't need to install it separately unless you're building custom tooling.

For normal usage, simply install the main package:

```bash
dotnet add package ZeroReflection.Mapper
```

The generator will automatically run during compilation and generate mapping code based on your `MapperProfile` configurations.

## How It Works

The source generator:

1. **Scans for MapperProfile classes** - Analyzes all classes that inherit from `MapperProfile` in your project
2. **Parses mapping configurations** - Extracts mapping pairs, custom mappings, property mappings, and ignored properties from your `Configure()` method
3. **Generates mapping code** - Creates static extension methods and helper classes for each mapping pair
4. **Builds a dispatcher** - Generates a type-safe dispatcher for runtime type resolution (switch-based or if/else)
5. **Creates DI extensions** - Generates service registration methods for dependency injection

## What It Generates

For each `MapperProfile` in your project, the generator creates:

### 1. Extension Methods for Object Mapping
```csharp
public static PersonEntity MapToPersonEntity(this PersonModel source)
public static List<PersonEntity> MapListToPersonEntity(List<PersonModel> source)
public static PersonEntity[] MapArrayToPersonEntity(PersonModel[] source)
```

### 2. Generated Mapping Dispatcher
A high-performance dispatcher class that routes mapping requests based on source/destination types:
- **Switch mode** (default): Uses dictionary lookup + switch statement for O(1) dispatch
- **If/else mode**: Sequential type comparisons for smaller code size

### 3. Service Registration Extension
```csharp
services.RegisterZeroReflectionMapping();
```
Automatically registers `IMapper` and `IGeneratedMappingDispatcher` with your DI container.

### 4. Static Mapping Classes
For each mapping pair (e.g., `PersonModel` → `PersonEntity`), generates a class like:
```csharp
public static class MapPersonModelToPersonEntity
{
    public static PersonEntity MapToPersonEntity(this PersonModel source) { ... }
    public static List<PersonEntity> MapListToPersonEntity(List<PersonModel> source) { ... }
    public static PersonEntity[] MapArrayToPersonEntity(PersonModel[] source) { ... }
}
```

## Configuration Options

The generator respects these configuration properties set in your `MapperProfile`:

- **`UseSwitchDispatcher`** (default: `true`) - Use switch-based dispatcher vs if/else chains
- **`ThrowIfPropertyMissing`** (default: `false`) - Generate build-time errors for unmapped destination properties
- **`EnableProjectionFunctions`** - Disabled/ignored for AOT safety

## Custom Mapping Support

The generator handles:
- **Property-level custom mappings** - Via `ForMember()` configuration
- **Full custom mappings** - Via `WithCustomMapping()` with static methods
- **Property ignoring** - Via `Ignore()` or `[IgnoreMap]` attribute
- **Property renaming** - Via `[MapTo("DestinationProperty")]` attribute

## AOT Compatibility

The generator produces AOT-friendly code:
- No runtime reflection
- No `Expression.Compile()`
- All mapping logic generated at compile time
- Only static custom mapping methods are supported

## Example Profile

```csharp
public class MyMapperProfile : MapperProfile
{
    public override void Configure(MapperConfiguration config)
    {
        config.UseSwitchDispatcher = true;
        config.ThrowIfPropertyMissing = false;
        
        config.CreateMap<PersonModel, PersonEntity>()
            .ForMember(dest => dest.Id, src => Guid.NewGuid())
            .Ignore(dest => dest.InternalField);
        
        config.CreateMap<ProductEntity, ProductModel>()
            .WithCustomMapping(StaticMappers.MapProduct);
    }
}
```

## Requirements

- .NET Standard 2.0 or higher
- Microsoft.CodeAnalysis.CSharp 4.0.1+
- Roslyn source generator support

## License
MIT

## Repository
[GitHub](https://github.com/younos1986/ZeroReflection)

