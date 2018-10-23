using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Work
{
    public interface IAction
    {
        Task<bool> CompleteAsync(IWorkflowEngineScope scope, CancellationToken token);

    }
}
