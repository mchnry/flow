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
        public Audit(AuditCode code, AuditSeverity severity, string itemId, string message)
        {
            this.Code = code;
            this.Severity = severity;
            this.ItemId = itemId;
            this.Message = message;
        }

        public AuditCode Code { get; }
        public AuditSeverity Severity { get; }
        public string ItemId { get; }
        public string Message { get; }
    }
}
