using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Work;

namespace Mchnry.Flow
{
    public interface IEngineComplete
    {

        EngineStatusOptions Status { get; }
        ValidationContainer Validations { get; }
        StepTraceNode<ActivityProcess> Process { get; }

        T GetModel<T>(string key);


    }
}
