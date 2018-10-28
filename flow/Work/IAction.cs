using Mchnry.Flow.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Work
{
    public interface IAction
    {
        Task<bool> CompleteAsync(IEngineScope scope, WorkflowEngineTrace trace, CancellationToken token);

    }
}
