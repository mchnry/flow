using System;

namespace Mchnry.Flow
{
    public struct ActivityProcess
    {
        public ActivityProcess(string processId, ActivityStatusOptions status, string message, TimeSpan elapsed)
            : this(processId, status, message)
        {

            this.Elapsed = elapsed;
            
        }
        public ActivityProcess(string processId, ActivityStatusOptions status, string message)
            
        {
            this.ProcessId = processId;
            this.Status = status;
            this.Message = message ?? string.Empty;
            this.TimeStamp = DateTimeOffset.UtcNow;
            this.Elapsed = null;
        }

        public string ProcessId { get; }
        public ActivityStatusOptions Status { get; internal set; }
        public string Message { get; }
        public DateTimeOffset TimeStamp { get; }
        public TimeSpan? Elapsed { get; }
    }
}
