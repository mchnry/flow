using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow
{


    /// <summary>
    /// exception thrown if engine encounters any issue with naming conventions
    /// </summary>
    public class ConventionMisMatchException: System.Exception
    {
        public string NamedItemId { get; set; }
        public string Reason { get; set; }

        public ConventionMisMatchException(string namedItemId, string reason) : this(namedItemId, reason, null) { }
        

        public ConventionMisMatchException(string namedItemId, string reason, System.Exception innerException) : base(string.Format("Error resolving conventions - {0}", reason ?? string.Empty), innerException)
        {
           
            this.NamedItemId = namedItemId;
            this.Reason = reason;
        }
    }
}
