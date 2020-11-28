using System.Threading;
using System.Threading.Tasks;
using Mchnry.Flow.Diagnostics;

namespace Mchnry.Flow.Logic
{
    public interface IRuleEvaluator<TModel>
    {

        
        Task EvaluateAsync(IEngineScope<TModel> scope, IEngineTrace trace, IRuleResult result, CancellationToken token);

    }
}
