using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Mapper.Generated;
using Application.Models.ViewModels;
using Application.Models.Entities;
using Mapster;
using IMapper = ZeroReflection.Mapper.IMapper;

namespace ZeroReflection.Benchmarks;

[MemoryDiagnoser]
[MediumRunJob]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class MappingBenchmarksSimpleObject
{
    private IMapper _myMapper = default!;
    private PersonEntity _singlePerson = default!;
    
    private AutoMapper.IMapper _mapper;
    private static PersonEntity PersonSample = new()
    {
        Email = "person@mail.com",
        Age = 38, 
        Name = "Person"
    };
    private static PersonEntity[] _peopleArray = [];
    private static List<PersonEntity> _peopleList = [];

    [GlobalSetup]
    public void Setup()
    {
        for (int i = 0; i < 25; i++)
        {
            _peopleList.Add(new()
            {
                Email = $"person{i}@mail.com",
                Age = i,
                Name = $"Person Name {i}"
            });
        }
        _peopleArray = _peopleList.ToArray();
        Console.WriteLine("************************************************************************************");
        Console.WriteLine($"People List Count: {_peopleList.Count}");
        Console.WriteLine("************************************************************************************");
        
        var services = new ServiceCollection();
        services.RegisterZeroReflectionMapping();
        services.AddLogging();
        services.AddAutoMapper(cfg => { },
            typeof(PersonEntity).Assembly);
        
        var sp = services.BuildServiceProvider();
        _myMapper = sp.GetRequiredService<IMapper>();
        _mapper = sp.GetRequiredService<AutoMapper.IMapper>();
    }

    [Benchmark]
    [BenchmarkCategory("Single")]
    public PersonModel ZeroReflection()
    {
        //return PersonSample.MapToPersonModel();
        return _myMapper.MapSingleObject<PersonEntity ,PersonModel>(PersonSample);
    }
    
    [Benchmark]
    [BenchmarkCategory("Single")]
    public PersonModel Mapster()
    {
        return PersonSample.Adapt<PersonModel>();
    }
    
    [Benchmark]
    [BenchmarkCategory("Single")]
    public PersonModel AutoMapper()
    {
        return _mapper.Map<PersonModel>(PersonSample);
    }
    
    //***********************************************************************************

    [Benchmark]
    [BenchmarkCategory("List to List")]
    public List<PersonModel> ZeroReflection_List()
    {
        return _myMapper.Map<List<PersonModel>>(_peopleList);
        //return _myMapper.Map<List<Person>, List<PersonModel>>(_peopleList);
    }
    
    [Benchmark]
    [BenchmarkCategory("List to List")]
    public List<PersonModel> Mapster_List()
    {
        return _peopleList.Adapt<List<PersonModel>>();
    }
    
    [Benchmark]
    [BenchmarkCategory("List to List")]
    public List<PersonModel> AutoMapper_List()
    {
        return _mapper.Map<List<PersonModel>>(_peopleList);
    }
    
    //***********************************************************************************
    
    [Benchmark]
    [BenchmarkCategory("Array To Array")]
    public PersonModel[] ZeroReflection_Array()
    {
        return _myMapper.Map<PersonModel[]>(_peopleArray);
        //return _myMapper.Map<Person[], PersonModel[]>(_peopleArray);
    }
    
    [Benchmark]
    [BenchmarkCategory("Array To Array")]
    public PersonModel[] Mapster_Array()
    {
        return _peopleArray.Adapt<PersonModel[]>();
    }
    
    [Benchmark]
    [BenchmarkCategory("Array To Array")]
    public PersonModel[] AutoMapper_Array()
    {
        return _mapper.Map<PersonModel[]>(_peopleArray);
    }
}
