# ZeroReflection.Mediator

ZeroReflection.Mediator is a .NET source generator for implementing the Mediator pattern. It helps you decouple request handling, notifications, and validation logic in your applications.

## Features
- Request/Response handling
- Notification publishing
- Validation support
- Source generator for compile-time performance

## Getting Started
Add the NuGet package to your project:

```
dotnet add package ZeroReflection.Mediator
```

Implement `IRequest`, `INotification`, and their handlers as needed. See the [documentation](https://ZeroReflection.example.com) for details.

## Controlling Code Generation

By default, ZeroReflection.Mediator generates code when referenced. If you want to disable code generation in your project, add the following property to your `.csproj` file:

```xml
<PropertyGroup>
  <EnableZeroReflectionMediatorGeneratedCode>false</EnableZeroReflectionMediatorGeneratedCode>
</PropertyGroup>
```

When set to `false`, the source generator will not emit generated code for ZeroReflection.Mediator.

## License
MIT

## Repository
[GitHub](https://github.com/)
