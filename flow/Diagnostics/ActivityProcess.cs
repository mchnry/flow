using System;

namespace Mchnry.Flow
{
    public struct ActivityProcess
    {
        public ActivityProcess(string activityId, ActivityStatusOptions status, DateTimeOffset timeStamp, TimeSpan elapsed)
            : this(activityId, status, timeStamp)
        {
            this.Elapsed = elapsed;
        }
        public ActivityProcess(string activityId, ActivityStatusOptions status, DateTimeOffset timeStamp)
            
        {
            this.ActivityId = activityId;
            this.Status = status;
            this.TimeStamp = timeStamp;
            this.Elapsed = null;
        }

        public string ActivityId { get; }
        public ActivityStatusOptions Status { get; internal set; }

        public DateTimeOffset TimeStamp { get; }
        public TimeSpan? Elapsed { get; }
    }
}
