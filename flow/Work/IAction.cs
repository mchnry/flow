using Mchnry.Flow.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Work
{
    public interface IAction<TModel>
    {
        Task<bool> CompleteAsync(IEngineScope<TModel> scope, WorkflowEngineTrace trace, CancellationToken token);

    }
    public interface IDeferredAction<TModel> : IAction<TModel>
    {
        string Id { get; }
    }
}
