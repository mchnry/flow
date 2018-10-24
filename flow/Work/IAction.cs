using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Work
{
    public interface IAction
    {
        Task<bool> CompleteAsync(IWorkflowEngine scope, string context, CancellationToken token);

    }
}
