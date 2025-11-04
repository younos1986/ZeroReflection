using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace ZeroReflection.Mediator.Tests;

public class MediatorTests
{
    [Fact]
    public async Task Test_All_Application_Handlers_Dynamically()
    {
        // Arrange - Set up DI container
        var services = new ServiceCollection();
        services.AddTransient<IMediator, ZeroReflection.Mediator.MediatorImplementation>();
        
        // Auto-discover and register all handlers from Application assembly
        var handlerInfos = RegisterAllHandlersFromApplication(services);
        
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        
        // Act & Assert - Test each discovered handler dynamically
        foreach (var handlerInfo in handlerInfos)
        {
            await TestHandlerDynamically(mediator, handlerInfo);
        }
        
        Console.WriteLine($"✅ Successfully tested {handlerInfos.Count} handlers from Application layer");
    }
    
    private List<HandlerInfo> RegisterAllHandlersFromApplication(IServiceCollection services)
    {
        var handlerInfos = new List<HandlerInfo>();
        
        // Get the Application assembly
        var applicationAssembly = Assembly.LoadFrom("Application.dll");
        
        // Find all handler types that implement IRequestHandler<,>
        var handlerTypes = applicationAssembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            .ToList();
        
        // Register each handler and collect info
        foreach (var handlerType in handlerTypes)
        {
            var handlerInterface = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
            
            var genericArgs = handlerInterface.GetGenericArguments();
            var requestType = genericArgs[0];
            var responseType = genericArgs[1];
            
            services.AddTransient(handlerInterface, handlerType);
            
            // Store handler info for testing
            handlerInfos.Add(new HandlerInfo
            {
                HandlerType = handlerType,
                RequestType = requestType,
                ResponseType = responseType,
                HandlerName = handlerType.Name
            });
        }
        
        // Find and register all validators
        var validatorTypes = applicationAssembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>)))
            .ToList();
        
        foreach (var validatorType in validatorTypes)
        {
            var validatorInterface = validatorType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));
            
            services.AddTransient(validatorInterface, validatorType);
        }
        
        Console.WriteLine($"📋 Discovered {handlerInfos.Count} handlers and {validatorTypes.Count} validators");
        foreach (var info in handlerInfos)
        {
            Console.WriteLine($"   - {info.HandlerName}: {info.RequestType.Name} -> {info.ResponseType.Name}");
        }
        
        return handlerInfos;
    }
    
    private async Task TestHandlerDynamically(IMediator mediator, HandlerInfo handlerInfo)
    {
        try
        {
            // Create a fake request instance
            var fakeRequest = CreateFakeRequest(handlerInfo.RequestType);
            
            // Use reflection to call mediator.Send<TResponse>(request)
            var sendMethod = typeof(IMediator).GetMethod("Send");
            var genericSendMethod = sendMethod.MakeGenericMethod(handlerInfo.ResponseType);
            
            // Call the handler
            var task = (Task)genericSendMethod.Invoke(mediator, new[] { fakeRequest, CancellationToken.None });
            await task;
            
            // Get the result
            var resultProperty = task.GetType().GetProperty("Result");
            var result = resultProperty.GetValue(task);
            
            Console.WriteLine($"✅ {handlerInfo.HandlerName} executed successfully. Result: {result?.ToString() ?? "null"}");
        }
        catch (Exception ex)
        {
            // Handle validation errors or other expected exceptions
            if (ex.InnerException is ArgumentNullException)
            {
                Console.WriteLine($"⚠️  {handlerInfo.HandlerName} validation triggered (expected for some handlers)");
            }
            else
            {
                Console.WriteLine($"❌ {handlerInfo.HandlerName} failed: {ex.Message}");
                throw; // Re-throw unexpected exceptions to fail the test
            }
        }
    }
    
    private object CreateFakeRequest(Type requestType)
    {
        // Create an instance of the request type
        var request = Activator.CreateInstance(requestType);
        
        // Fill in properties with fake data
        var properties = requestType.GetProperties().Where(p => p.CanWrite);
        
        foreach (var property in properties)
        {
            var fakeValue = CreateFakeValue(property.PropertyType, property.Name);
            if (fakeValue != null)
            {
                property.SetValue(request, fakeValue);
            }
        }
        
        return request;
    }
    
    private object CreateFakeValue(Type type, string propertyName)
    {
        // Handle common types with realistic fake data
        if (type == typeof(string))
        {
            return string.IsNullOrEmpty(propertyName) ? "FakeValue" : $"Fake{propertyName}";
        }
        
        if (type == typeof(int))
        {
            return 42;
        }
        
        if (type == typeof(bool))
        {
            return true;
        }
        
        if (type == typeof(DateTime))
        {
            return DateTime.Now;
        }
        
        if (type == typeof(Guid))
        {
            return Guid.NewGuid();
        }
        
        if (type.IsEnum)
        {
            var enumValues = Enum.GetValues(type);
            return enumValues.Length > 0 ? enumValues.GetValue(0) : null;
        }
        
        // Handle nullable types
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            return CreateFakeValue(underlyingType, propertyName);
        }
        
        // Handle complex objects (try to create them recursively)
        if (type.IsClass && type != typeof(string) && type.GetConstructors().Any(c => c.GetParameters().Length == 0))
        {
            var instance = Activator.CreateInstance(type);
            var properties = type.GetProperties().Where(p => p.CanWrite);
            
            foreach (var property in properties)
            {
                var fakeValue = CreateFakeValue(property.PropertyType, property.Name);
                if (fakeValue != null)
                {
                    property.SetValue(instance, fakeValue);
                }
            }
            
            return instance;
        }
        
        return null; // For types we can't handle, leave as default
    }
    
    private class HandlerInfo
    {
        public Type HandlerType { get; set; }
        public Type RequestType { get; set; }
        public Type ResponseType { get; set; }
        public string HandlerName { get; set; }
    }
}