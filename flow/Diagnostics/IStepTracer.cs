namespace Mchnry.Flow.Diagnostics
{
    public interface IStepTracer
    {

        StepTrace Trace(StepTrace toTrace);
        StepTrace Trace(StepTrace previous, string toTrace);
        
        StepTrace Root { get; }
        void Flush();
    }
}
