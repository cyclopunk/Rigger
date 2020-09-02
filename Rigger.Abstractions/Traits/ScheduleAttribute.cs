using System;

namespace Rigger.Attributes
{
    /// <summary>
    /// Enable a method to be ran on a schedule using a statement.
    ///
    /// Requires Forge.Chronos module.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ScheduleAttribute : Attribute
    {
        /// <summary>
        /// A statement of when the method should run. Statements can contain the following phrases:
        ///
        /// StartDayOfWeek-EndDayOfWeek (e.g. Mon-Thu)
        /// Every X (seconds|minutes|hours|days)
        /// Mon-Fri At 00:00 And 01:00 And 05:00 (i.e. run at midnight, 1am and 5am Monday through Friday)
        /// </summary>
        public string Statement { get; set; }
    }
}