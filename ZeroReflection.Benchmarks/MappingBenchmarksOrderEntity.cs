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
//[ShortRunJob] // built-in fast mode
[LongRunJob]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
//[SimpleJob(RuntimeMoniker.Net80)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class MappingBenchmarksOrderEntity
{
    private IMapper _myMapper = default!;
    private OrderEntity _singlePerson = default!;

    private AutoMapper.IMapper _mapper;

    private static OrderEntity _OrderEntity = new()
    {
        Id = 1,
        OrderNumber = "OrderNumber",
        OrderDate = DateTime.Now,
        TotalAmount = 10,
        CustomerName = "CustomerName",
        CustomerEmail = "CustomerEmail",
        ShippingAddress = "ShippingAddress",
        BillingAddress = "BillingAddress",
        OrderStatus = $"Pending ",
        ShippedDate = DateTime.Now,
        DeliveredDate = DateTime.Now,
        TrackingNumber =  $"Pending 11",
        PaymentMethod = $"Pending 9",
        Notes = $"Pending 10",
        CustomerPhone = $"Pending 1",
        CustomerAddress = $"Pending 2",
        CustomerCity = $"Pending 3",
        CustomerState = $"Pending 4",
        CustomerZipCode = $"Pending 5",
        CustomerCountry = $"Pending 6",
        CustomerCompany = $"Pending 7",
        CustomerTaxId = $"Pending 8",
    };

    private static OrderEntity[] _orderArray = [];
    private static List<OrderEntity> _orderList = [];

    [GlobalSetup]
    public void Setup()
    {
        for (int i = 0; i < 25; i++)
        {
            _orderList.Add(new()
            {
                Id = 1,
                OrderNumber = $"{i}",
                OrderDate = DateTime.Now,
                TotalAmount = i,
                CustomerName = $"CustomerName{i}",
                CustomerEmail = $"CustomerEmail{i}",
                ShippingAddress = $"ShippingAddress{i}",
                BillingAddress = $"BillingAddress{i}",
                OrderStatus = $"Pending {i}",
                ShippedDate = DateTime.Now,
                DeliveredDate = DateTime.Now,
                TrackingNumber =  $"Pending {i}",
                PaymentMethod = $"Pending {i}",
                Notes = $"Pending {i}",
                CustomerPhone = $"Pending {i}",
                CustomerAddress = $"Pending {i}",
                CustomerCity = $"Pending {i}",
                CustomerState = $"Pending {i}",
                CustomerZipCode = $"Pending {i}",
                CustomerCountry = $"Pending {i}",
                CustomerCompany = $"Pending {i}",
                CustomerTaxId = $"Pending {i}",
            });
        }

        _orderArray = _orderList.ToArray();
        Console.WriteLine("************************************************************************************");
        Console.WriteLine($"People List Count: {_orderList.Count}");
        Console.WriteLine("************************************************************************************");

        var services = new ServiceCollection();
        services.RegisterZeroReflectionMapping();
        services.AddLogging();
        services.AddAutoMapper(cfg => { },
            typeof(OrderEntity).Assembly);

        var sp = services.BuildServiceProvider();
        _myMapper = sp.GetRequiredService<IMapper>();
        _mapper = sp.GetRequiredService<AutoMapper.IMapper>();
    }

    [Benchmark]
    [BenchmarkCategory("Single")]
    public OrderModel ZeroReflection()
    {
        //return OrderEntity.MapToOrderModel();
        return _myMapper.MapSingleObject<OrderEntity, OrderModel>(_OrderEntity);
    }

    [Benchmark]
    [BenchmarkCategory("Single")]
    public OrderModel Mapster()
    {
        return _OrderEntity.Adapt<OrderModel>();
    }

    [Benchmark]
    [BenchmarkCategory("Single")]
    public OrderModel AutoMapper()
    {
        return _mapper.Map<OrderModel>(_OrderEntity);
    }

    //***********************************************************************************

    [Benchmark]
    [BenchmarkCategory("List to List")]
    public List<OrderModel> ZeroReflection_List()
    {
        return _myMapper.Map<List<OrderModel>>(_orderList);
        //return _myMapper.Map<List<Person>, List<OrderModel>>(_orderList);
    }

    [Benchmark]
    [BenchmarkCategory("List to List")]
    public List<OrderModel> Mapster_List()
    {
        return _orderList.Adapt<List<OrderModel>>();
    }

    [Benchmark]
    [BenchmarkCategory("List to List")]
    public List<OrderModel> AutoMapper_List()
    {
        return _mapper.Map<List<OrderModel>>(_orderList);
    }


    //***********************************************************************************

    [Benchmark]
    [BenchmarkCategory("Array To Array")]
    public OrderModel[] ZeroReflection_Array()
    {
        return _myMapper.Map<OrderModel[]>(_orderArray);
        //return _myMapper.Map<Person[], OrderModel[]>(_orderArray);
    }

    [Benchmark]
    [BenchmarkCategory("Array To Array")]
    public OrderModel[] Mapster_Array()
    {
        return _orderArray.Adapt<OrderModel[]>();
    }

    [Benchmark]
    [BenchmarkCategory("Array To Array")]
    public OrderModel[] AutoMapper_Array()
    {
        return _mapper.Map<OrderModel[]>(_orderArray);
    }
}