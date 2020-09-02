using System.Reflection;

namespace Rigger.Validators
{
    /// <summary>
    /// <exception cref="MethodValidationException">This exception is thrown if there is a validation error</exception>
    /// </summary>
    public interface IMethodValidator
    {
        void Validate(MethodInfo method);
    }
}