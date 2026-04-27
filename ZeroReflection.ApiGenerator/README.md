# ZeroReflection.ApiGenerator

AOT-compatible API routing and Swagger UI generator for .NET applications.

## Features

- ? **Zero Reflection** - All routing generated at compile-time
- ? **AOT-Compatible** - Full NativeAOT support
- ? **Embedded Swagger UI** - Beautiful API explorer included
- ? **Automatic Code Generation** - Controllers discovered via source generators
- ? **Simple HTTP Server** - Built-in development server
- ? **Type-Safe Routing** - Compile-time route validation

## Installation

```bash
dotnet add package ZeroReflection.ApiGenerator
dotnet add package ZeroReflection.ApiAttributes
```

## Quick Start

### 1. Define a Controller

```csharp
using ZeroReflection.ApiAttributes;

[ApiController("api/users")]
public class UsersController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<List<UserModel>> ListUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        return await _mediator.Send(new ListUsersQuery { Page = page, PageSize = pageSize }, ct);
    }

    [HttpGet("{id}")]
    public async Task<UserModel> GetUser([FromRoute] string id, CancellationToken ct = default)
    {
        return await _mediator.Send(new GetUserQuery { Id = id }, ct);
    }

    [HttpPost]
    public async Task<Unit> CreateUser([FromBody] CreateUserCommand command, CancellationToken ct = default)
    {
        return await _mediator.Send(command, ct);
    }

    [HttpPut("{id}")]
    public async Task<Unit> UpdateUser(
        [FromRoute] string id,
        [FromBody] UpdateUserCommand command,
        CancellationToken ct = default)
    {
        command.Id = id;
        return await _mediator.Send(command, ct);
    }

    [HttpDelete("{id}")]
    public async Task<Unit> DeleteUser([FromRoute] string id, CancellationToken ct = default)
    {
        return await _mediator.Send(new DeleteUserCommand { Id = id }, ct);
    }
}
```

### 2. Register Controllers

```csharp
var services = new ServiceCollection();
services.RegisterControllers();  // Auto-generated extension method
```

### 3. Start the Server with Swagger UI

```csharp
var sp = services.BuildServiceProvider();

// Option 1: Use the built-in HTTP server with Swagger UI
using var server = sp.UseSwaggerUI("http://localhost:5000/");
Console.WriteLine("Server running at http://localhost:5000/swagger");
Console.ReadKey();

// Option 2: Use the generated router directly
var result = await GeneratedApiRouter.RouteAsync(
    path: "/api/users",
    method: "GET",
    routeValues: new(),
    queryValues: new() { { "page", "1" }, { "pageSize", "10" } },
    body: null,
    serviceProvider: sp
);
```

## Swagger UI

The Swagger UI is automatically embedded in the generator and served at `/swagger`. It provides:

- ?? **Beautiful Interface** - Clean, modern design
- ?? **Interactive Testing** - Test endpoints directly from the browser
- ?? **Request/Response Display** - See formatted JSON responses
- ?? **Parameter Forms** - Easy input for route, query, and body parameters
- ? **AOT-Compatible** - No reflection, fully trimmed

### Editing the Swagger UI

The Swagger UI HTML is located at `ZeroReflection.ApiGenerator/Resources/swagger.html`. 

You can customize:
- Styling (CSS in the `<style>` section)
- Layout (HTML structure)
- Behavior (JavaScript functions)

After editing, rebuild the project - the HTML is embedded as a resource during compilation.

## Attributes

### Controller Attribute

```csharp
[ApiController("route/base")]
public class MyController { }
```

### HTTP Method Attributes

```csharp
[HttpGet]                    // GET request
[HttpGet("custom/path")]     // GET with custom route
[HttpPost]                   // POST request
[HttpPut("{id}")]           // PUT with route parameter
[HttpDelete("{id}")]        // DELETE with route parameter
[HttpPatch]                 // PATCH request
```

### Parameter Source Attributes

```csharp
[FromRoute]   // Extract from URL path: /api/users/{id}
[FromQuery]   // Extract from query string: /api/users?page=1
[FromBody]    // Extract from request body (JSON)
```

### Adding Summaries

```csharp
[HttpGet(Summary = "Gets all users with pagination")]
public Task<List<UserModel>> ListUsers(...) { }
```

## How It Works

1. **Discovery Phase** - Source generator scans for `[ApiController]` classes
2. **Code Generation** - Generates routing dispatcher, endpoint metadata, and Swagger middleware
3. **Compilation** - All code is compiled, no runtime reflection
4. **Execution** - Direct method invocations, maximum performance

## Generated Code

The generator creates:

- `GeneratedApiRouter.g.cs` - Main routing dispatcher
- `GeneratedEndpointsMetadata.g.cs` - Endpoint definitions
- `SwaggerUIMiddleware.g.cs` - Swagger UI server
- `SimpleHttpServer.g.cs` - Development HTTP server
- `RouteHelper.g.cs` - Route matching utilities

## AOT Compatibility

Fully compatible with NativeAOT:

```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <InvariantGlobalization>true</InvariantGlobalization>
</PropertyGroup>
```

All JSON serialization uses source-generated contexts - no reflection at runtime!

## Integration with ZeroReflection.Mediator

Works seamlessly with ZeroReflection.Mediator for CQRS patterns:

```csharp
// Controller delegates to mediator
[HttpPost]
public async Task<Unit> CreateUser([FromBody] CreateUserCommand command, CancellationToken ct)
{
    return await _mediator.Send(command, ct);
}

// Command handler does the work
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Unit>
{
    public Task<Unit> Handle(CreateUserCommand request, CancellationToken ct)
    {
        // Handle the command
        return Task.FromResult(Unit.Value);
    }
}
```

## License

MIT

## Contributing

Contributions welcome! Please open an issue or PR on GitHub.
