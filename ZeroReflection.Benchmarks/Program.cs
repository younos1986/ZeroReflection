using System.Text.Json;
using Application.Models.Entities;
using Application.Models.ViewModels;
using AutoMapper;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Benchmarks;
using ZeroReflection.Mapper;
using ZeroReflection.Mapper.Generated;
using IMapper = ZeroReflection.Mapper.IMapper;

// var config = ManualConfig.Create(DefaultConfig.Instance)
//     .AddJob(Job.ShortRun
//         .WithIterationCount(5)
//         .WithLaunchCount(1)
//         .WithWarmupCount(3));

//BenchmarkRunner.Run<MappingBenchmarksCustomMapper>(config);
BenchmarkRunner.Run<MappingBenchmarksComplexObject>();
//BenchmarkRunner.Run<MappingBenchmarksSimpleObject>(config);
//BenchmarkRunner.Run<MappingBenchmarksOrderEntity>();
//BenchmarkRunner.Run<MappingBenchmarksAddressModelSimpleObject>(config);

return 0;

List<PersonEntity> peopleList =
[
    new()
    {
        Email = $"person@mail.com",
        Age = 38,
        Name = $"Person Name ",
        Certificate = new CertificateEntity()
        {
            CertificateId = $"CertId ",
            CertificateName = $"Certificate Name ",
            ExpiryDate = DateTime.Now
        },
        Addresses =
        [
            new()
            {
                Street = $"Street ",
                City = $"City ",
                ZipCode = $"ZipCode ",
            }
        ]
    }

];


var ssss = peopleList.Select(q => (PersonModel)q).ToList();
Console.WriteLine(JsonSerializer.Serialize(ssss));

Console.Read();


var services = new ServiceCollection();
services.RegisterZeroReflectionMapping();
        
var sp = services.BuildServiceProvider();
var myMapper = sp.GetRequiredService<IMapper>();

BalanceEntity BalanceSample = new()
{
    Id = Guid.NewGuid().ToString(),
    UserId = Guid.NewGuid().ToString(),
    Amount = 100,
    CreatedAt = DateTime.Now,
    UpdatedAt = DateTime.Now,
    IsDeleted = false,
};
var balanceModel = myMapper.MapSingleObject<BalanceEntity ,BalanceModel>(BalanceSample);

var result = myMapper.Map<List<PersonModel>>(peopleList);

Console.WriteLine(JsonSerializer.Serialize(result));


