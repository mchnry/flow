using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow
{
    public enum ValidationSeverity
    {
        /// <summary>
        /// Alerts the calling system that a confirmation is needed. 
        /// </summary>
        Confirm = 0,
        /// <summary>
        /// Similar to Confirm, but advises the calling system that the confirmation should be
        /// provided by someone with escalated permissions
        /// </summary>
        Escalate = 1,
        /// <summary>
        /// Alerts the calling system of a stop-condition that must be fixed.
        /// </summary>
        Fatal = 2

    }
}
