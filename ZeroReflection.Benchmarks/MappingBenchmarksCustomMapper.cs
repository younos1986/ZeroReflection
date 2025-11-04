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
[ShortRunJob] // built-in fast mode
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
//[SimpleJob(RuntimeMoniker.Net80)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class MappingBenchmarksCustomMapper
{
    private IMapper _myMapper = default!;
    private BalanceEntity _singleBalance = default!;
    
    private AutoMapper.IMapper _mapper;
    private static BalanceEntity BalanceSample = new()
    {
         Id = Guid.NewGuid().ToString(),
         UserId = Guid.NewGuid().ToString(),
         Amount = 100,
         CreatedAt = DateTime.Now,
         UpdatedAt = DateTime.Now,
         IsDeleted = false,
    };
    private static BalanceEntity[] _balanceArray = [];
    private static List<BalanceEntity> _balanceList = [];

    [GlobalSetup]
    public void Setup()
    {
        for (int i = 0; i < 25; i++)
        {
            _balanceList.Add(new()
            {
                Id = Guid.NewGuid().ToString(),
                UserId = Guid.NewGuid().ToString(),
                Amount = 100,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsDeleted = false,
            });
        }
        _balanceArray = _balanceList.ToArray();
        Console.WriteLine("************************************************************************************");
        Console.WriteLine($"Balance List Count: {_balanceList.Count}");
        Console.WriteLine("************************************************************************************");
        
        var services = new ServiceCollection();
        services.RegisterZeroReflectionMapping();
        services.AddLogging();
        services.AddAutoMapper(cfg => { },
            typeof(BalanceEntity).Assembly);
        
        var sp = services.BuildServiceProvider();
        _myMapper = sp.GetRequiredService<IMapper>();
        _mapper = sp.GetRequiredService<AutoMapper.IMapper>();
    }

    [Benchmark]
    [BenchmarkCategory("Single")]
    public BalanceModel ZeroReflection()
    {
        return _myMapper.MapSingleObject<BalanceEntity ,BalanceModel>(BalanceSample);
    }
    
    [Benchmark]
    [BenchmarkCategory("Single")]
    public BalanceModel Mapster()
    {
        return BalanceSample.Adapt<BalanceModel>();
    }
    
    [Benchmark]
    [BenchmarkCategory("Single")]
    public BalanceModel AutoMapper()
    {
        return _mapper.Map<BalanceModel>(BalanceSample);
    }
    
    //***********************************************************************************

    [Benchmark]
    [BenchmarkCategory("List to List")]
    public List<BalanceModel> ZeroReflection_List()
    {
        return _myMapper.Map<List<BalanceModel>>(_balanceList);
    }
    
    [Benchmark]
    [BenchmarkCategory("List to List")]
    public List<BalanceModel> Mapster_List()
    {
        return _balanceList.Adapt<List<BalanceModel>>();
    }
    
    [Benchmark]
    [BenchmarkCategory("List to List")]
    public List<BalanceModel> AutoMapper_List()
    {
        return _mapper.Map<List<BalanceModel>>(_balanceList);
    }
    
    //***********************************************************************************
    
    [Benchmark]
    [BenchmarkCategory("Array To Array")]
    public BalanceModel[] ZeroReflection_Array()
    {
        return _myMapper.Map<BalanceModel[]>(_balanceArray);
    }
    
    [Benchmark]
    [BenchmarkCategory("Array To Array")]
    public BalanceModel[] Mapster_Array()
    {
        return _balanceArray.Adapt<BalanceModel[]>();
    }
    
    [Benchmark]
    [BenchmarkCategory("Array To Array")]
    public BalanceModel[] AutoMapper_Array()
    {
        return _mapper.Map<BalanceModel[]>(_balanceArray);
    }
}
