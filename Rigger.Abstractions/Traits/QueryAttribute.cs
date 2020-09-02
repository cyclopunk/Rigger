using System;

namespace Rigger.API.Attributes
{
    /// <summary>
    /// Attribute to mark a method as a query. Queries will be exposed as fields off the root query in
    /// the GraphQL implementation of ForgeAPI
    /// </summary>
    public class QueryAttribute : Attribute
    {
        
    }
}