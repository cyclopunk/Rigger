using System;

namespace Rigger.Attributes
{
    /// <summary>
    /// Attribute to denote a field or property should be autowired, i.e. the container will
    /// look up the type of the field, property or parameter and insert the proper underlying
    /// concrete class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field  | AttributeTargets.Property | AttributeTargets.Constructor, Inherited = true)]
    public class AutowireAttribute : Attribute
    {

    }
}
