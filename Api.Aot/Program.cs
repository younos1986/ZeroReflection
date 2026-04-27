using Api;
using Api.Generated;
using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Api;

var services = new ServiceCollection();
services.RegisterControllers();

var sp = services.BuildServiceProvider();

if (ApplicationExtensions.IsRunningInLambda())
{
    await sp.RunAsLambdaAsync<ApiJsonContext>(GeneratedApiRouter.RouteAsync);
}
else
{
    using var server = sp.UseSwaggerUI("http://localhost:5200/");
    Console.WriteLine("Press Ctrl+C to stop the server...");
    var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
    try { await Task.Delay(Timeout.Infinite, cts.Token); } catch (TaskCanceledException) { }
}
