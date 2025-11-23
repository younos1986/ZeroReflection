using Microsoft.Extensions.DependencyInjection;

namespace ZeroReflection.Mediator.Tests;

public class MediatorTests
{
    [Fact]
    public async Task Test_All_Application_Handlers_With_Generated_Dispatcher()
    {
        // Arrange - Use the generated registration
        var services = new ServiceCollection();
        
        // Register using the generated method from Application
        services.RegisterZeroReflectionMediatorHandlers();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        // Test PingCommand
        var pingCommand = new Application.Features.Pings.PingCommand 
        { 
            Message = "Test Message",
            MessageType = Application.Features.Pings.MessageType.Ping,
            PingCommandChild = new Application.Features.Pings.PingCommandChild
            {
                ChildMessage = "Child Test",
                ChildMessageType = Application.Features.Pings.MessageType.Pong
            }
        };
        
        var pingResult = await mediator.Send(pingCommand);
        Assert.NotNull(pingResult);
        Assert.Contains("Pong:", pingResult);
        
        // Test AddProductCommand  
        var addProductCommand = new Application.Features.Products.AddProductCommand();
        
        var productResult = await mediator.Send(addProductCommand);
        // Unit is a value type, so just check it executes
        
        Console.WriteLine($"✅ Successfully tested handlers with generated dispatcher");
    }

    [Fact]
    public async Task Test_Validation_Is_Called()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterZeroReflectionMediatorHandlers();
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        // Act & Assert - Should throw validation error for null message
        var pingCommand = new Application.Features.Pings.PingCommand 
        { 
            Message = null!, // This should trigger validation
            MessageType = Application.Features.Pings.MessageType.Ping,
            PingCommandChild = new Application.Features.Pings.PingCommandChild
            {
                ChildMessage = "Child",
                ChildMessageType = Application.Features.Pings.MessageType.Pong
            }
        };
        
        await Assert.ThrowsAsync<ArgumentNullException>(async () => 
        {
            await mediator.Send(pingCommand);
        });
        
        Console.WriteLine($"✅ Validation correctly triggered");
    }
}