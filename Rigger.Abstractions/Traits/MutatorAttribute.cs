using System;

namespace Rigger.API.Attributes
{
    /// <summary>
    /// Attribute to mark a method as a mutator. Mutators will be exposed as fields off the root query in
    /// the GraphQL implementation of ForgeAPI
    /// </summary>
    public class MutatorAttribute : Attribute
    {
        
    }
}