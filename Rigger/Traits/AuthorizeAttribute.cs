using System;
using System.Collections.Generic;
using System.Linq;

namespace Rigger.Attributes
{
    /// <summary>
    /// This attribute will determine if a class (Service), method, property or field can be accessed
    /// by a user using an API.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class AuthorizeAttribute : Attribute
    {
        public AuthorizeAttribute()
        {
        }

        public AuthorizeAttribute(params string[] claims)
        {
            this.Claims = claims.ToList();
        }

        public List<string> Claims { get; set; }
    }
}