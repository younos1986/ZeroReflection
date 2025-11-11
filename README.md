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

Annotate your classes with mapping attributes and implement mapping profiles as needed. See the documentation for details.

## Defining a Mapper Profile
Inherit from `MapperProfile` and override `Configure`:

```csharp
public class ZeroReflectionMapperProfile : MapperProfile
{
    public override void Configure(MapperConfiguration config)
    {
        config.UseSwitchDispatcher = true; // jump-table dispatcher
        config.ThrowIfPropertyMissing = false;

        config.CreateMap<PersonModel, PersonEntity>().Reverse();

        config.CreateMap<ProductModel, ProductEntity>()
            .ForMember(dest => dest.Id, src => Guid.NewGuid().ToString())
            .Ignore(dest => dest.Manufacturer);
        config.CreateMap<ProductEntity, ProductModel>(); // reverse of custom mapping

        config.CreateMap<BalanceEntity, BalanceModel>()
            .WithCustomMapping(StaticMappers.MapBalanceModelToBalanceEntity);
        config.CreateMap<BalanceModel, BalanceEntity>();
    }
}
```

### Key Points
- Reverse Mapping: `.Reverse()` allowed only for simple mappings (no custom/ignore/member configs).
- Custom Member Mapping: `.ForMember()` to provide a value for a destination property.
- Ignore Properties: `.Ignore()` to skip mapping a destination property.
- Custom Mapping Functions: `.WithCustomMapping(Func<TSource,TDest>)` for full control (must be static for AOT).

## Dependency Injection Registration
```csharp
var services = new ServiceCollection();
services.RegisterZeroReflectionMapping();
var sp = services.BuildServiceProvider();
var mapper = sp.GetRequiredService<IMapper>();
```

## Using the mapper
- Single object: `var entity = model.MapToPersonEntity();`
- Collections: `var list = MapPersonModelToPersonEntity.MapListToPersonEntity(models);`
- Via `IMapper`: `mapper.Map<List<PersonEntity>>(models)` or `mapper.MapSingleObject<PersonModel, PersonEntity>(model)`.

## Generated Output Summary
1. DI registration extension (registers `IMapper`, dispatcher, and mapping classes).
2. Per-map static class: single object + list/array conversion helpers.
3. Dispatcher: routes object/list/array mappings using either chained type checks or a switch jump-table.

## Configuration Flags
- `UseSwitchDispatcher` (default `true`): switch-based jump table vs chained type checks.
- `ThrowIfPropertyMissing` (default `false`): injects build-time `#error` for unmapped destination properties.
- `EnableProjectionFunctions` is currently disabled and ignored (projection expressions & compiled delegates removed for AOT safety).

## Custom Mappings
Provide a static method or a `Func<TSource,TDestination>` delegate. Expression-based (`Expression<Func<...>>`) custom mappings are not supported (no `Compile()` usage).

## AOT / NativeAOT Support
ZeroReflection.Mapper is trimmed/AOT-friendly:
- No runtime reflection for mappings (non-static custom mapping methods are rejected at generation).
- No dynamic code generation (`Expression.Compile` removed).
- Projection members (Expression/compiled delegates) are stripped.
- Use only static custom mapping methods or delegate overload.
- `EnableProjectionFunctions` always treated as `false`.
If you previously relied on projection expressions, replace them with standard LINQ using the generated list/array methods.

## Benchmarks
Benchmarks (in `ZeroReflection.Benchmarks/`) compare single-object and collection mapping performance against other mappers.

## License
MIT

## Repository
https://github.com/younos1986/ZeroReflection
