using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Work;

namespace Mchnry.Flow
{
    public interface IEngineComplete<TModel>
    {

        EngineStatusOptions Status { get; }
        IValidationContainer Validations { get; }
        StepTraceNode<ActivityProcess> Process { get; }

        TModel GetModel(string key);


    }
}
