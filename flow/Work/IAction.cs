using Mchnry.Flow.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Mchnry.Flow.Work
{
    public interface IAction<TModel>
    {
        WorkDefine.ActionDefinition Definition { get; }
        Task<bool> CompleteAsync(IEngineScope<TModel> scope, WorkflowEngineTrace trace, CancellationToken token);

    }
    public interface IDeferredAction<TModel> 
    {
        Task<bool> CompleteAsync(IEngineScopeDefer<TModel> scope, WorkflowEngineTrace trace, CancellationToken token);

    }
}
