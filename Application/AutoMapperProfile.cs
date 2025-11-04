using Application.Models.Entities;
using Application.Models.ViewModels;
using AutoMapper;

namespace Application;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<PersonModel, PersonEntity>().ReverseMap();
        CreateMap<AddressModel, AddressEntity>().ReverseMap();
        CreateMap<CertificateModel, CertificateEntity>().ReverseMap();

        CreateMap<ProductModel, ProductEntity>().ReverseMap();
        CreateMap<UserModel, UserEntity>().ReverseMap();
        CreateMap<BalanceModel, BalanceEntity>().ReverseMap();

        CreateMap<OrderModel, OrderEntity>().ReverseMap();
        CreateMap<OrderItemModel, OrderItemEntity>().ReverseMap();
    }
}