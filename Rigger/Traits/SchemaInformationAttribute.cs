using System;

namespace Rigger.API.Attributes
{
    /// <summary>
    /// Attribute that will mark Query / Mutator / Field objects with
    /// information that can be used in documentation or override the name
    /// values.
    /// </summary>
    public class SchemaInformationAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}