using System.Reflection;
using Rigger.Exceptions;

namespace Rigger.Validators
{
    /// <summary>
    /// This validator makes sure that a method that is marked as OnCreate has no parameters.
    ///
    /// TODO Flag for removal as methods parameters can now be autowired.
    /// </summary>
    public class OnCreateMethodPatternValidator : IMethodValidator
    {
        public void Validate(MethodInfo method)
        {
            if (method.GetParameters().Length != 0)
            {
                throw new MethodValidationException($"{method.Name} does not have a 0 parameter signature. It cannot be used with OnCreate.");
            }
        }
    }
}