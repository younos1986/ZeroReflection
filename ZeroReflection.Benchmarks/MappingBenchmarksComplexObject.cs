using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;
using ZeroReflection.Mapper.Generated;
using Application.Models.ViewModels;
using Application.Models.Entities;
using Mapster;
using IMapper = ZeroReflection.Mapper.IMapper;

namespace ZeroReflection.Benchmarks;

[MemoryDiagnoser]
//[SimpleJob(RuntimeMoniker.Net80, id: "LongRun", launchCount: 1, warmupCount: 3, iterationCount: 10)]
[LongRunJob]
//[ShortRunJob]
[Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class MappingBenchmarksComplexObject
{
    private IMapper _myMapper = default!;
    private AutoMapper.IMapper _mapper = default!;

    private static readonly PersonEntity PersonSample = new()
    {
        Email = "person@mail.com",
        Age = 10,
        Name = "Person Name",
        Certificate = new CertificateEntity
        {
            CertificateId = $"CertId",
            CertificateName = $"Certificate Name",
            ExpiryDate = DateTime.Now
        },
        Addresses =
        [
            new()
            {
                Street = $"Street",
                City = $"City",
                ZipCode = $"ZipCode",
            }
        ]
    };

    private static PersonEntity[] _peopleArray = [];
    private static List<PersonEntity> _peopleList = [];

    [GlobalSetup]
    public void Setup()
    {
        for (int i = 0; i < 1025; i++)
        {
            _peopleList.Add(new()
            {
                Email = $"person{i}@mail.com",
                Age = i,
                Name = $"Person Name {i}",
                Certificate = new CertificateEntity()
                {
                    CertificateId = $"CertId {i}",
                    CertificateName = $"Certificate Name {i}",
                    ExpiryDate = DateTime.Now.AddYears(i % 5)
                },
                Addresses =
                [
                    new()
                    {
                        Street = $"Street {i}",
                        City = $"City {i}",
                        ZipCode = $"ZipCode {i}",
                    }
                ]
            });
        }
        _peopleArray = _peopleList.ToArray();

        var services = new ServiceCollection();
        services.RegisterZeroReflectionMapping();
        services.AddLogging();
        services.AddAutoMapper(cfg => { }, typeof(PersonEntity).Assembly);
        var sp = services.BuildServiceProvider();
        _myMapper = sp.GetRequiredService<IMapper>();
        _mapper = sp.GetRequiredService<AutoMapper.IMapper>();
    }

    [Benchmark]
    [BenchmarkCategory("Single")]
    public PersonModel ZeroReflection() => _myMapper.MapSingleObject<PersonEntity, PersonModel>(PersonSample);

    [Benchmark]
    [BenchmarkCategory("Single")]
    public PersonModel Mapster() => PersonSample.Adapt<PersonModel>();

    [Benchmark]
    [BenchmarkCategory("Single")]
    public PersonModel AutoMapper() => _mapper.Map<PersonModel>(PersonSample);

    [Benchmark]
    [BenchmarkCategory("Single")]
    public PersonModel Implicit() => (PersonModel)PersonSample;

    [Benchmark]
    [BenchmarkCategory("List to List")]
    public List<PersonModel> ZeroReflection_List() => _myMapper.Map<List<PersonModel>>(_peopleList);

    [Benchmark]
    [BenchmarkCategory("List to List")]
    public List<PersonModel> Mapster_List() => _peopleList.Adapt<List<PersonModel>>();

    [Benchmark]
    [BenchmarkCategory("List to List")]
    public List<PersonModel> AutoMapper_List() => _mapper.Map<List<PersonModel>>(_peopleList);

    [Benchmark]
    [BenchmarkCategory("List to List")]
    public List<PersonModel> Implicit_List() => _peopleList.Select(q => (PersonModel)q).ToList();

    [Benchmark]
    [BenchmarkCategory("Array To Array")]
    public PersonModel[] ZeroReflection_Array() => _myMapper.Map<PersonModel[]>(_peopleArray);

    [Benchmark]
    [BenchmarkCategory("Array To Array")]
    public PersonModel[] Mapster_Array() => _peopleArray.Adapt<PersonModel[]>();

    [Benchmark]
    [BenchmarkCategory("Array To Array")]
    public PersonModel[] AutoMapper_Array() => _mapper.Map<PersonModel[]>(_peopleArray);
}
