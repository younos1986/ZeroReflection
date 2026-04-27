# ZeroReflection API Generator - Swagger UI Integration

## Overview

The ZeroReflection API Generator now includes an **embedded Swagger-like UI** that runs completely AOT-compatible in your console application. This allows you to visually explore and test your API endpoints without needing any external tools or reflection.

## Features

? **100% AOT-Compatible** - No reflection, works with Native AOT  
? **Built-in HTTP Server** - Simple `HttpListener`-based server  
? **Beautiful Swagger-like UI** - Modern, responsive interface  
? **Zero Dependencies** - All HTML/CSS/JS embedded in generated code  
? **One-line Setup** - Just call `UseSwaggerUI()`  
? **Customizable Port** - Configure any port and path  

## Quick Start

### 1. Add the API Generator to your project

```xml
<ItemGroup>
  <ProjectReference Include="..\ZeroReflection.ApiAttributes\ZeroReflection.ApiAttributes.csproj" />
  <ProjectReference Include="..\ZeroReflection.ApiGenerator\ZeroReflection.ApiGenerator.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

### 2. Create your controllers

```csharp
using ZeroReflection.ApiAttributes;
using ZeroReflection.Mediator;

namespace MyApp.Controllers;

[ApiController("api/users")]
public class UsersController
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}", Summary = "Get user by ID")]
    public async Task<UserModel> GetUser([FromRoute] string id, CancellationToken ct)
    {
        return await _mediator.Send(new GetUserQuery { Id = id }, ct);
    }

    [HttpPost(Summary = "Create a new user")]
    public async Task<Unit> CreateUser([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        return await _mediator.Send(command, ct);
    }
}
```

### 3. Start the server in your Program.cs

```csharp
using Microsoft.Extensions.DependencyInjection;
using AotSample.Generated;

var services = new ServiceCollection();
services.RegisterZeroReflectionMediatorHandlers();
services.RegisterControllers(); // Auto-generated extension method

var sp = services.BuildServiceProvider();

// Start HTTP server with Swagger UI
using var server = sp.UseSwaggerUI("http://localhost:5000/");

Console.WriteLine("Swagger UI: http://localhost:5000/swagger");
Console.ReadKey();
```

### 4. Open your browser

Navigate to: **http://localhost:5000/swagger**

You'll see a beautiful interface showing all your endpoints!

## API Reference

### `UseSwaggerUI()` Extension Method

```csharp
public static SimpleHttpServer UseSwaggerUI(
    this IServiceProvider serviceProvider, 
    string url = "http://localhost:5000/")
```

**Parameters:**
- `serviceProvider` - The DI service provider containing your controllers
- `url` - Base URL for the HTTP server (default: `http://localhost:5000/`)

**Returns:**  
A `SimpleHttpServer` instance that implements `IDisposable`.

**Example:**

```csharp
// Default port 5000
using var server = sp.UseSwaggerUI();

// Custom port
using var server = sp.UseSwaggerUI("http://localhost:8080/");

// Listen on all network interfaces
using var server = sp.UseSwaggerUI("http://+:5000/");
```

## Available Endpoints

Once the server is running, the following endpoints are available:

| Endpoint | Description |
|----------|-------------|
| `/swagger` | Swagger UI interface |
| `/swagger/index.html` | Swagger UI (alternative path) |
| `/swagger/api/endpoints.json` | JSON metadata of all endpoints |
| `/api/*` | Your actual API endpoints |

## Swagger UI Features

The embedded Swagger UI provides:

- ?? **Endpoint List** - All your API endpoints grouped by controller
- ?? **Color-coded HTTP Methods** - GET (blue), POST (green), PUT (orange), DELETE (red)
- ?? **Interactive Forms** - Fill in parameters directly in the UI
- ? **Execute Requests** - Test endpoints with real HTTP calls
- ?? **Response Viewer** - See formatted JSON responses with status codes
- ?? **Route Parameters** - Automatic extraction from URL patterns
- ?? **Query Parameters** - Form fields for query string parameters
- ?? **Request Body** - JSON editor for POST/PUT request bodies

## Configuration Options

### Disable Code Generation

If you want to disable the API generator in a specific project:

```xml
<PropertyGroup>
  <EnableZeroReflectionApiGeneratedCode>false</EnableZeroReflectionApiGeneratedCode>
</PropertyGroup>
```

### Custom Swagger Path

You can modify the Swagger UI path by editing the generated `SwaggerUIMiddleware.g.cs`:

```csharp
// Change from /swagger to /docs
if (path.EndsWith("/docs") || path.EndsWith("/docs/"))
{
    content = GetSwaggerHTML();
    contentType = "text/html";
    return true;
}
```

## How It Works

### 1. Build-Time Generation

The source generator scans your controllers decorated with `[ApiController]` and methods with `[HttpGet]`, `[HttpPost]`, etc. It generates:

- `GeneratedApiRouter.g.cs` - Routes HTTP requests to controller methods
- `SwaggerUIMiddleware.g.cs` - Serves the Swagger UI HTML
- `SimpleHttpServer.g.cs` - Lightweight HTTP server
- `GeneratedEndpointsMetadata.g.cs` - Metadata about all endpoints
- `RouteHelper.g.cs` - Route pattern matching utilities

### 2. Runtime Execution

When you call `UseSwaggerUI()`:

1. Creates an `HttpListener` on the specified port
2. Starts listening for incoming HTTP requests
3. Routes `/swagger` requests to the embedded UI
4. Routes API requests to `GeneratedApiRouter`
5. Automatically serializes responses to JSON

### 3. Zero Reflection

Everything is generated at compile-time:
- No `Reflection.Emit`
- No runtime assembly scanning
- No dynamic proxy generation
- Fully compatible with Native AOT

## Examples

### Example: Multiple Servers

```csharp
// Run multiple servers on different ports
using var adminServer = sp.UseSwaggerUI("http://localhost:5000/");
using var publicServer = sp.UseSwaggerUI("http://localhost:8080/");

Console.WriteLine("Admin API: http://localhost:5000/swagger");
Console.WriteLine("Public API: http://localhost:8080/swagger");
Console.ReadKey();
```

### Example: Programmatic Testing

```csharp
// You can also test endpoints programmatically
var result = await GeneratedApiRouter.RouteAsync(
    path: "/api/users/123",
    method: "GET",
    routeValues: new(),
    queryValues: new(),
    body: null,
    serviceProvider: sp
);

Console.WriteLine($"Status: {result.StatusCode}");
Console.WriteLine($"Response: {JsonSerializer.Serialize(result.Data)}");
```

### Example: Conditional Swagger

```csharp
#if DEBUG
using var server = sp.UseSwaggerUI("http://localhost:5000/");
Console.WriteLine("Swagger UI: http://localhost:5000/swagger");
#endif

Console.ReadKey();
```

## Comparison with Traditional Swagger

| Feature | Traditional Swagger | ZeroReflection Swagger UI |
|---------|-------------------|---------------------------|
| Reflection | ? Heavy usage | ? Zero |
| AOT Compatible | ? No | ? Yes |
| Runtime overhead | ?? High | ? Minimal |
| External dependencies | ?? Many | ?? None |
| Build-time generation | ? No | ? Yes |
| OpenAPI spec | ? Full | ?? Simplified |

## Limitations

1. **No OpenAPI 3.0 Export** - The UI is custom, not standard OpenAPI
2. **Basic Styling** - Not as feature-rich as Swagger UI
3. **No Authentication UI** - Auth must be handled in your code
4. **No Model Schemas** - Request/response models shown as JSON only

## Future Enhancements

- [ ] Export OpenAPI 3.0 spec
- [ ] Support for file uploads
- [ ] Authentication UI (Bearer, API Key)
- [ ] Request/response examples
- [ ] Model schema visualization
- [ ] Dark mode theme
- [ ] Export as Postman collection

## Troubleshooting

### Port Already in Use

```
System.Net.HttpListenerException: The process cannot access the file because it is being used by another process
```

**Solution:** Change the port in `UseSwaggerUI("http://localhost:XXXX/")`

### Swagger UI Shows No Endpoints

**Check:**
1. Controllers are decorated with `[ApiController]`
2. Methods have HTTP verb attributes (`[HttpGet]`, `[HttpPost]`, etc.)
3. Build is successful (source generator ran)
4. `RegisterControllers()` is called

### CORS Errors in Browser

The embedded server doesn't include CORS headers. If calling from a different origin:

```csharp
// Add CORS headers in generated HttpServerEmitter.cs
response.Headers.Add("Access-Control-Allow-Origin", "*");
```

## Contributing

Contributions are welcome! Please submit PRs to:  
https://github.com/younos1986/ZeroReflection

## License

MIT License - see LICENSE file for details

---

**Made with ?? by the ZeroReflection team**
