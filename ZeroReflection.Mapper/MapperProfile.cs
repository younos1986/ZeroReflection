namespace ZeroReflection.Mapper;

/// <summary>
/// Base class for defining mapping profiles in ZeroReflection.Mapper.
/// Implement this class and override <see cref="Configure"/> to register custom mappings and configuration.
/// </summary>
public abstract class MapperProfile
{
    /// <summary>
    /// Configures mappings and custom rules for this profile.
    /// Override this method to register mappings using the provided <paramref name="config"/>.
    /// </summary>
    /// <param name="config">The mapper configuration to register mappings with.</param>
    public abstract void Configure(MapperConfiguration config);
}