namespace ZeroReflection.Mediator;

public interface IValidator<in T>
{
    void Validate(T request);
}