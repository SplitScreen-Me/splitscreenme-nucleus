using System;

namespace Nucleus.Gaming
{
    /// <summary>
    /// Custom attribute for variables that will matter if updated after the Javascript engine
    /// has already initialized your script
    /// </summary>
    public class DynamicAttribute : Attribute
    {
        public bool AutoHandles { get; set; }

        public DynamicAttribute()
        {

        }

    }
}
