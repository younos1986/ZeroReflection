﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Microsoft.Extensions.DependencyInjection;
using Application.Models.ViewModels;
using Application.Models.Entities;
using Mapster;
using ZeroReflection.Mapper.Generated;
using IMapper = ZeroReflection.Mapper.IMapper;

namespace ZeroReflection.Benchmarks;

[MemoryDiagnoser]
//[ShortRunJob] // built-in fast mode
[LongRunJob]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
//[SimpleJob(RuntimeMoniker.Net80)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class MappingBenchmarksAddressModelSimpleObject
{
    private IMapper _myMapper = default!;
    
    private AutoMapper.IMapper _mapper = default!;
    private static AddressModel _addressModel = new() { Street = "123 Main St", City = "Sample City", ZipCode = "12345" };
    private static AddressModel[] _addressArray = [];
    private static List<AddressModel> _addressList = [];

    [GlobalSetup]
    public void Setup()
    {
        for (int i = 0; i < 25; i++)
        {
            _addressList.Add(new()
            {
                Street = $"123 Main St {i}",
                City = $"Sample City {i}",
                ZipCode = $"12345 {i}"
            });
        }
        _addressArray = _addressList.ToArray();
        Console.WriteLine("************************************************************************************");
        Console.WriteLine($"Address List Count: {_addressList.Count}");
        Console.WriteLine("************************************************************************************");
        
        var services = new ServiceCollection();
        services.RegisterZeroReflectionMapping();
        services.AddLogging();
        services.AddAutoMapper(_ => { },
            typeof(AddressEntity).Assembly);
        
        var sp = services.BuildServiceProvider();
        _myMapper = sp.GetRequiredService<IMapper>();
        _mapper = sp.GetRequiredService<AutoMapper.IMapper>();
    }

    [Benchmark]
    [BenchmarkCategory("Single")]
    public AddressEntity ZeroReflection()
    {
        return _myMapper.MapSingleObject<AddressModel, AddressEntity>(_addressModel);
    }
    
    [Benchmark]
    [BenchmarkCategory("Single")]
    public AddressEntity Mapster()
    {
        return _addressModel.Adapt<AddressEntity>();
    }
    
    [Benchmark]
    [BenchmarkCategory("Single")]
    public AddressEntity AutoMapper()
    {
        return _mapper.Map<AddressEntity>(_addressModel);
    }
    
    //***********************************************************************************

    [Benchmark]
    [BenchmarkCategory("List to List")]
    public List<AddressEntity> ZeroReflection_List()
    {
        return _myMapper.Map<List<AddressEntity>>(_addressList);
    }
    
    [Benchmark]
    [BenchmarkCategory("List to List")]
    public List<AddressEntity> Mapster_List()
    {
        return _addressList.Adapt<List<AddressEntity>>();
    }
    
    [Benchmark]
    [BenchmarkCategory("List to List")]
    public List<AddressEntity> AutoMapper_List()
    {
        return _mapper.Map<List<AddressEntity>>(_addressList);
    }
    
    //***********************************************************************************
    
    [Benchmark]
    [BenchmarkCategory("Array To Array")]
    public AddressEntity[] ZeroReflection_Array()
    {
        return _myMapper.Map<AddressEntity[]>(_addressArray);
    }
    
    [Benchmark]
    [BenchmarkCategory("Array To Array")]
    public AddressEntity[] Mapster_Array()
    {
        return _addressArray.Adapt<AddressEntity[]>();
    }
    
    [Benchmark]
    [BenchmarkCategory("Array To Array")]
    public AddressEntity[] AutoMapper_Array()
    {
        return _mapper.Map<AddressEntity[]>(_addressArray);
    }
}
