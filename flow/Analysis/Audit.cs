using System;
using System.Collections.Generic;
using System.Text;

namespace Mchnry.Flow.Analysis
{

    public enum AuditSeverity
    {
        Informational,
        Warning,
        Critical
    }
    public struct Audit
    {
        public Audit(AuditSeverity severity, string itemId, string message)
        {
            this.Severity = severity;
            this.ItemId = itemId;
            this.Message = message;
        }

        public AuditSeverity Severity { get; }
        public string ItemId { get; }
        public string Message { get; }
    }
}
