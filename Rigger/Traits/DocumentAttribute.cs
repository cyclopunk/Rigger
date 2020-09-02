using System;

namespace Rigger.Attributes
{
    /// <summary>
    /// This attribute can be used to add documentation to a class, method, property or field. It can be used by
    /// assembly scanners to automatically document the usage of that member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property)]
    public class DocumentAttribute : Attribute
    {
        public string Synopsis { get; set; }
        public string Description { get; set; }
        public string Example { get; set; }
    }
}