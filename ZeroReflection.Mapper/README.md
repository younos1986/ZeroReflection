# ZeroReflection.Mapper

ZeroReflection.Mapper is a .NET source generator for object mapping. It enables fast, compile-time mapping between different object types, reducing boilerplate and improving performance.

## Features
- Attribute-based mapping configuration
- Custom mapping profiles
- Ignore properties with attributes
- Fast dispatcher for arrays/lists/single objects (if/else or switch jump-table)
- Source-generated, reflection-free property and collection mapping

## Getting Started
Add the NuGet package to your project:

```
dotnet add package ZeroReflection.Mapper
```

Annotate your classes with mapping attributes and implement mapping profiles as needed.

## Using the mapper
- Single object: `var entity = model.MapToPersonEntity();`
- Collections: `var list = MapPersonModelToPersonEntity.MapListToPersonEntity(models);`
- Via `IMapper`: `mapper.Map<List<PersonEntity>>(models)` or `mapper.MapSingleObject<PersonModel, PersonEntity>(model)`.

## Configuration Flags
- `UseSwitchDispatcher` (default `true`): switch-based jump table vs chained type checks.
- `ThrowIfPropertyMissing` (default `false`): injects build-time `#error` for unmapped destination properties.
- `EnableProjectionFunctions` is disabled/ignored for AOT safety.

## Custom Mappings
Use static methods or `Func<TSource,TDestination>` delegates. Expression-based mappings are not supported.

## AOT / NativeAOT Support
- No runtime reflection for mapping (non-static custom mapping methods are rejected at generation time).
- No dynamic code generation (`Expression.Compile`).
- Projection members removed.
- Use only static custom mapping methods or delegate overload.

## License
MIT

## Repository
https://github.com/younos1986/ZeroReflection

