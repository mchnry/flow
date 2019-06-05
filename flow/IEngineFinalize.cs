using Mchnry.Flow.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow
{
    public interface IEngineFinalize<TModel>
    {
        Task<IEngineComplete<TModel>> FinalizeAsync(CancellationToken token);
        EngineStatusOptions Status { get; }
        IValidationContainer Validations { get; }
        StepTraceNode<ActivityProcess> Process { get; }

        TModel GetModel(string key);
        TModel GetModel();
    }
}