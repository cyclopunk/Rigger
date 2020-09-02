using System;

namespace Rigger.Attributes
{
    /// <summary>
    /// Attribute that specifies that a method should be ran after an interval period.
    /// This method will run indefinitely or until the object on which it is attached
    /// is destroyed.
    ///
    /// Requires Forge.Chronos module.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class RunEveryAttribute : Attribute
    {
        /// <summary>
        /// Millisecond interval
        /// </summary>
        public double Interval { get; set; }
    }
}