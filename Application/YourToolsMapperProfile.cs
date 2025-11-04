using Application.Models.Entities;
using Application.Models.ViewModels;
using ZeroReflection.Mapper;

namespace Application;

public class ZeroReflectionMapperProfile : MapperProfile
{
    public override void Configure(MapperConfiguration config)
    {
        config.EnableProjectionFunctions = false;
        config.UseSwitchDispatcher = true;
        
        config.CreateMap<PersonModel, PersonEntity>().Reverse();
        config.CreateMap<CertificateModel, CertificateEntity>().Reverse();
        config.CreateMap<AddressModel, AddressEntity>().Reverse();
        
        config.CreateMap<UserModel, UserEntity>().Reverse();
        
        config.CreateMap<OrderModel, OrderEntity>().Reverse();

        config.CreateMap<ProductModel, ProductEntity>()
            .ForMember(dest => dest.Id, source => Guid.NewGuid().ToString() + "#125")
            .Ignore(dest => dest.Manufacturer);  // Strongly typed ignore

        // don't forget reverse mapping for customized ones
        config.CreateMap<ProductEntity, ProductModel>();

        config.CreateMap<BalanceEntity, BalanceModel>()
            .WithCustomMapping(StaticMappers.MapBalanceModelToBalanceEntity);

        // don't forget reverse mapping for WithCustomMapping
        config.CreateMap<BalanceModel, BalanceEntity>();
    }
}

public class StaticMappers
{
    public static BalanceModel MapBalanceModelToBalanceEntity(BalanceEntity model)
    {
        return new BalanceModel
        {
            Id = model.Id,
            UserId = model.UserId,
            Amount = model.Amount,
            CreatedAt = model.CreatedAt,
            UpdatedAt = model.UpdatedAt,
            IsDeleted = model.IsDeleted
        };
    }
}