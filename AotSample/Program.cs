// See https://aka.ms/new-console-template for more information

using AotSample.Commands;
using AotSample.Models.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Mapper.Generated;
using ZeroReflection.Mediator;

Console.WriteLine("Hello, World!");

var services = new ServiceCollection();
services.RegisterZeroReflectionMapping();
services.RegisterZeroReflectionMediatorHandlers();
var sp = services.BuildServiceProvider();
var myMediator = sp.GetRequiredService<IMediator>();

await myMediator.Send(new CreateUserCommand
{
    UserModel = new UserModel
    {
        Name = "Younes Baghei Moghaddam",
        Age = 38,
        Email = "unos.bm65@gmail.com"
    }
});

Console.ReadKey();