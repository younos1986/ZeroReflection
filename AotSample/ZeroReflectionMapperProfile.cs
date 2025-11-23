using AotSample.Models.Entities;
using AotSample.Models.ViewModels;
using ZeroReflection.Mapper;

namespace AotSample;

public class ZeroReflectionMapperProfile : MapperProfile
{
    public override void Configure(MapperConfiguration config)
    {
        config.ThrowIfPropertyMissing = false;
        config.EnableProjectionFunctions = false;
        config.UseSwitchDispatcher = true;
        
        config.CreateMap<UserModel, UserEntity>().Reverse();
    }
}