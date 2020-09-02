using System.Reflection;

namespace TheCommons.Forge.Validators
{
    /// <summary>
    /// <exception cref="MethodValidationException">This exception is thrown if there is a validation error</exception>
    /// </summary>
    public interface IMethodValidator
    {
        void Validate(MethodInfo method);
    }
}