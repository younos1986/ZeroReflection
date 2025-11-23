# ZeroReflection.Mediator

ZeroReflection.Mediator is a high-performance .NET source generator for implementing the Mediator pattern with zero runtime reflection. It provides compile-time code generation for request handling and validation logic in your applications.

## Features
- **Request/Response handling** - Send commands and queries with typed responses
- **Validation support** - Automatic validation before request handling
- **Zero reflection** - All dispatch logic generated at compile time
- **Optimized dispatching** - Switch-based jump table or if/else chains
- **Automatic DI registration** - Generated extension method for service registration
- **AOT-friendly** - No runtime code generation or reflection

## Getting Started

### Installation
Add the NuGet package to your project:

```bash
dotnet add package ZeroReflection.Mediator
dotnet add package ZeroReflection.MediatorGenerator
```

### Basic Usage

#### 1. Define a Request and Handler

```csharp
using ZeroReflection.Mediator;

// Request with response
public class PingCommand : IRequest<string>
{
    public string Message { get; set; }
}

// Handler
public class PingCommandHandler : IRequestHandler<PingCommand, string>
{
    public Task<string> Handle(PingCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Pong: {request.Message}");
    }
}
```

#### 2. Commands Without Response (Using Unit)

For commands that don't return a value, use the `Unit` type:

```csharp
public class AddProductCommand : IRequest<Unit>
{
    public string ProductName { get; set; }
}

public class AddProductCommandHandler : IRequestHandler<AddProductCommand, Unit>
{
    public Task<Unit> Handle(AddProductCommand request, CancellationToken cancellationToken)
    {
        // Do something
        Console.WriteLine($"Added product: {request.ProductName}");
        return Task.FromResult(Unit.Value);
    }
}
```

#### 3. Add Validation (Optional)

```csharp
public class PingCommandValidator : IValidator<PingCommand>
{
    public void Validate(PingCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            throw new ArgumentNullException(nameof(request.Message));
    }
}
```

#### 4. Register Services

The source generator automatically creates a `RegisterZeroReflectionMediatorHandlers()` extension method:

```csharp
using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Mediator;

var services = new ServiceCollection();

// Automatically registers IMediator and all handlers/validators
services.RegisterZeroReflectionMediatorHandlers();

var serviceProvider = services.BuildServiceProvider();
```

#### 5. Use the Mediator

```csharp
var mediator = serviceProvider.GetRequiredService<IMediator>();

var command = new PingCommand { Message = "Hello" };
var result = await mediator.Send(command);
// result: "Pong: Hello"
```

## Configuration

### Disable Code Generation

To disable code generation in a specific project, add this to your `.csproj` file:

```xml
<PropertyGroup>
  <EnableZeroReflectionMediatorGeneratedCode>false</EnableZeroReflectionMediatorGeneratedCode>
</PropertyGroup>
```

When set to `false`, the source generator will not emit generated code for ZeroReflection.Mediator.

### Switch Dispatcher Mode

By default, the generator uses a switch-based dispatcher for optimal performance. To use if/else chains instead:

```xml
<PropertyGroup>
  <ZeroReflectionMediatorUseSwitchDispatcher>false</ZeroReflectionMediatorUseSwitchDispatcher>
</PropertyGroup>
```

The switch dispatcher uses a dictionary lookup + switch statement for fast type-based dispatch, while if/else mode uses sequential type comparisons.

## How It Works

The source generator:
1. Scans your project for classes implementing `IRequestHandler<,>` and `IValidator<>`
2. Generates a `GeneratedMediatorDispatcher` that handles type-based routing without reflection
3. Creates a `RegisterZeroReflectionMediatorHandlers()` method that registers all handlers and validators
4. Generates optimized dispatch code using either switch statements or if/else chains

All dispatching happens at compile time - no reflection or dynamic code generation at runtime.

## License
MIT

## Repository
[GitHub](https://github.com/younos1986/ZeroReflection)
