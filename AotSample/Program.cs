using AotSample;
using AotSample.Generated;
using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Api;
using ZeroReflection.Mapper.Generated;
using ZeroReflection.Mediator;

// Setup dependency injection
var services = new ServiceCollection();
services.RegisterZeroReflectionMapping();
services.RegisterZeroReflectionMediatorHandlers();
services.RegisterControllers();

var sp = services.BuildServiceProvider();

// Run in appropriate mode based on environment
if (ApplicationExtensions.IsRunningInLambda())
{
    // AWS Lambda mode - delegated to library
    await sp.RunAsLambdaAsync<AotJsonContext>(GeneratedApiRouter.RouteAsync);
}
else
{
    // Local mode with Swagger UI
    using var server = sp.UseSwaggerUI("http://localhost:5100/");
    Console.ReadKey();
}