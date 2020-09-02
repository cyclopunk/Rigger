using System;

namespace Rigger.API.Attributes
{
    /// <summary>
    /// This attribute marks a class as external. External classes are included in
    /// API schema building and documentation.
    /// </summary>
    
    [AttributeUsage(AttributeTargets.Class)]
    public class ExternalAttribute : Attribute
    {
        
    }
}