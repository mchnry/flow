namespace Mchnry.Flow.Work
{
    public class ActivityProcess
    {
        public ActivityProcess(string activityId, ActivityStatusOptions status, bool isPlaceHolder)
        {
            this.ActivityId = activityId;
            this.Status = status;
            this.IsPlaceHolder = isPlaceHolder;
        }

        public string ActivityId { get; }
        public ActivityStatusOptions Status { get; internal set; }
        public bool IsPlaceHolder { get; }
    }
}
