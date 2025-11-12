// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using AotSample.Models.Entities;
using AotSample.Models.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Mapper;
using ZeroReflection.Mapper.Generated;

Console.WriteLine("Hello, World!");

var services = new ServiceCollection();
services.RegisterZeroReflectionMapping();
var sp = services.BuildServiceProvider();
var myMapper = sp.GetRequiredService<IMapper>();

var personModel = new PersonModel
{
    Name = "Younes Baghei Moghaddam",
    Age = 38,
    Email = "unos.bm65@gmail.com"
};

var personEntity = myMapper.MapSingleObject<PersonModel, PersonEntity>(personModel);

Console.WriteLine(personEntity.Name + " " + personEntity.Age + " " + personEntity.Email);

Console.ReadKey();