using System;

namespace Mchnry.Flow
{
    public struct ActivityProcess
    {
        public ActivityProcess(string activityId, ActivityStatusOptions status, string message, TimeSpan elapsed)
            : this(activityId, status, message)
        {

            this.Elapsed = elapsed;
            
        }
        public ActivityProcess(string activityId, ActivityStatusOptions status, string message)
            
        {
            this.ActivityId = activityId;
            this.Status = status;
            this.Message = message ?? string.Empty;
            this.TimeStamp = DateTimeOffset.UtcNow;
            this.Elapsed = null;
        }

        public string ActivityId { get; }
        public ActivityStatusOptions Status { get; internal set; }
        public string Message { get; }
        public DateTimeOffset TimeStamp { get; }
        public TimeSpan? Elapsed { get; }
    }
}
