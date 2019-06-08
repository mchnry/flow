using Mchnry.Flow.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Mchnry.Flow.Work
{
    public interface IAction<TModel>
    {
        Task<bool> CompleteAsync(IEngineScope<TModel> scope, IEngineTrace trace, CancellationToken token);

    }
    public interface IDeferredAction<TModel> 
    {
        Task<bool> CompleteAsync(IEngineScopeDefer<TModel> scope, IEngineTrace trace, CancellationToken token);

    }
}
