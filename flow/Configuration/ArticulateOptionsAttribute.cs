using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Configuration
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ArticulateOptionsAttribute: Attribute
    {
        public ArticulateOptionsAttribute(string description)
        {
            this.Description = description;
        }

        public string Description { get; }
    }
}
