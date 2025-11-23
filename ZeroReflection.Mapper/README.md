﻿# ZeroReflection.Mapper

ZeroReflection.Mapper is a high-performance .NET source generator for object mapping. It generates compile-time mapping code, eliminating runtime reflection and providing blazing-fast object transformations.

## Features
- **Profile-based configuration** - Define mappings using `MapperProfile` classes
- **Extension methods** - Generated extension methods for fluent mapping syntax
- **Collection mapping** - Built-in support for `List<T>` and `T[]` collections
- **Custom mappings** - Per-property or full object custom mapping support
- **Property ignoring** - Ignore properties via configuration or attributes
- **Reverse mappings** - Bidirectional mapping support with `.Reverse()`
- **Fast dispatching** - Switch-based or if/else dispatcher for type resolution
- **Zero reflection** - All mapping code generated at compile time
- **AOT-compatible** - Full NativeAOT support with no runtime code generation

## Installation

Add the NuGet packages to your project:

```bash
dotnet add package ZeroReflection.Mapper
dotnet add package ZeroReflection.MapperGenerator
```

## Quick Start

### 1. Define Your Models

```csharp
public class PersonModel
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
}

public class PersonEntity
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
}
```

### 2. Create a Mapping Profile

```csharp
using ZeroReflection.Mapper;

public class MyMapperProfile : MapperProfile
{
    public override void Configure(MapperConfiguration config)
    {
        // Simple bidirectional mapping
        config.CreateMap<PersonModel, PersonEntity>().Reverse();
    }
}
```

### 3. Register with Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Mapper.Generated;

var services = new ServiceCollection();
services.RegisterZeroReflectionMapping();

var serviceProvider = services.BuildServiceProvider();
var mapper = serviceProvider.GetRequiredService<IMapper>();
```

### 4. Use the Mapper

#### Option A: Extension Methods (Recommended)
```csharp
var model = new PersonModel { Name = "John", Age = 30, Email = "john@example.com" };

// Map single object
var entity = model.MapToPersonEntity();

// Map collections
var models = new List<PersonModel> { model };
var entities = MapPersonModelToPersonEntity.MapListToPersonEntity(models);
```

#### Option B: IMapper Interface
```csharp
// Map single object
var entity = mapper.MapSingleObject<PersonModel, PersonEntity>(model);

// Map collections
var entities = mapper.Map<List<PersonEntity>>(models);

// Map arrays
var entityArray = mapper.Map<PersonEntity[]>(modelArray);
```

## Advanced Configuration

### Custom Property Mapping

```csharp
public class MyMapperProfile : MapperProfile
{
    public override void Configure(MapperConfiguration config)
    {
        config.CreateMap<ProductModel, ProductEntity>()
            .ForMember(dest => dest.Id, src => Guid.NewGuid().ToString())
            .ForMember(dest => dest.CreatedAt, src => DateTime.UtcNow);
    }
}
```

### Ignoring Properties

#### Via Configuration (Strongly-Typed)
```csharp
config.CreateMap<ProductModel, ProductEntity>()
    .Ignore(dest => dest.InternalField)
    .Ignore(dest => dest.CalculatedProperty);
```

#### Via Attribute
```csharp
public class ProductEntity
{
    public string Name { get; set; }
    
    [IgnoreMap]
    public string InternalField { get; set; }
}
```

### Custom Mapping Methods

Use static methods for full control over mapping logic:

```csharp
public class MyMapperProfile : MapperProfile
{
    public override void Configure(MapperConfiguration config)
    {
        config.CreateMap<OrderModel, OrderEntity>()
            .WithCustomMapping(CustomMappers.MapOrder);
    }
}

public static class CustomMappers
{
    public static OrderEntity MapOrder(OrderModel model)
    {
        return new OrderEntity
        {
            Id = model.Id,
            Total = model.Items.Sum(i => i.Price * i.Quantity),
            ItemCount = model.Items.Count
        };
    }
}
```

### Property Name Mapping

Use the `[MapTo]` attribute when property names differ:

```csharp
public class SourceModel
{
    [MapTo("DestinationName")]
    public string SourceName { get; set; }
}

public class DestinationModel
{
    public string DestinationName { get; set; }
}
```

### Reverse Mappings

```csharp
// Simple reverse - creates both directions
config.CreateMap<PersonModel, PersonEntity>().Reverse();

// Note: .Reverse() cannot be used with ForMember, Ignore, or WithCustomMapping
// Define separate mappings instead:
config.CreateMap<ProductModel, ProductEntity>()
    .ForMember(dest => dest.Id, src => Guid.NewGuid());
config.CreateMap<ProductEntity, ProductModel>(); // Separate reverse mapping
```

## Configuration Options

Configure global settings in your `MapperProfile`:

```csharp
public override void Configure(MapperConfiguration config)
{
    // Use switch-based dispatcher (default: true)
    // Switch mode: O(1) lookup via dictionary + switch statement
    // If/else mode: Sequential type comparisons
    config.UseSwitchDispatcher = true;
    
    // Throw compile-time errors for unmapped properties (default: false)
    config.ThrowIfPropertyMissing = false;
    
    // Projection functions (disabled for AOT safety)
    config.EnableProjectionFunctions = false;
    
    config.CreateMap<PersonModel, PersonEntity>().Reverse();
}
```

## Collection Mapping

ZeroReflection.Mapper automatically generates collection mapping methods:

```csharp
// Lists
List<PersonModel> models = GetModels();
var entities = MapPersonModelToPersonEntity.MapListToPersonEntity(models);

// Arrays
PersonModel[] modelArray = GetModelArray();
var entityArray = MapPersonModelToPersonEntity.MapArrayToPersonEntity(modelArray);

// Via IMapper
var entities2 = mapper.Map<List<PersonEntity>>(models);
var entityArray2 = mapper.Map<PersonEntity[]>(modelArray);
```

## AOT / NativeAOT Support

ZeroReflection.Mapper is fully compatible with .NET Native AOT:

- ✅ **No runtime reflection** - All mapping generated at compile time
- ✅ **No dynamic code** - No `Expression.Compile()` or runtime IL emission
- ✅ **Static methods only** - Custom mappers must be static
- ✅ **Trim-safe** - No reflection-based type discovery

### AOT Restrictions

- ❌ Expression-based custom mappings (use `Func<TSource, TDestination>` instead)
- ❌ Projection functions (disabled by default)
- ❌ Instance method custom mappers (use static methods)

## Performance

ZeroReflection.Mapper generates optimal mapping code:

- **Zero reflection overhead** - All type information resolved at compile time
- **Inlined methods** - Extension methods marked with `[MethodImpl(AggressiveInlining)]`
- **Switch optimization** - Dictionary + switch for O(1) type dispatch
- **Direct property assignment** - No intermediate objects or delegates

## Generated Code Example

For a mapping configuration like:
```csharp
config.CreateMap<PersonModel, PersonEntity>().Reverse();
```

The generator creates:
```csharp
public static class MapPersonModelToPersonEntity
{
    public static PersonEntity MapToPersonEntity(this PersonModel source)
    {
        if (source == null) return null;
        return new PersonEntity
        {
            Name = source.Name,
            Age = source.Age,
            Email = source.Email
        };
    }
    
    public static List<PersonEntity> MapListToPersonEntity(List<PersonModel> source)
        => MapCollectionHelpers.MapList<PersonModel, PersonEntity>(source, x => x.MapToPersonEntity());
    
    public static PersonEntity[] MapArrayToPersonEntity(PersonModel[] source) { ... }
}
```

## License
MIT

## Repository
https://github.com/younos1986/ZeroReflection

