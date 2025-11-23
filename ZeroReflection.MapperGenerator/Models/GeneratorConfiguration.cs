namespace ZeroReflection.MapperGenerator.Models
{
    /// <summary>
    /// Internal configuration model for the source generator.
    /// This mirrors the runtime MapperConfiguration properties that affect code generation.
    /// </summary>
    public class GeneratorConfiguration
    {
        public bool EnableProjectionFunctions { get; set; }
        public bool UseSwitchDispatcher { get; set; } = true;
        public bool ThrowIfPropertyMissing { get; set; }
    }
}

