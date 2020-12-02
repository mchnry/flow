using System.Threading;
using System.Threading.Tasks;
using Mchnry.Flow.Diagnostics;

namespace Mchnry.Flow.Logic
{

    public interface IRuleEvaluatorX<TModel> { }

    public interface IEvaluatorRule<TModel>: IRuleEvaluatorX<TModel>
    {

        
        Task<bool> EvaluateAsync(IEngineScope<TModel> scope, IEngineTrace trace, CancellationToken token);

    }

    public interface IValidatorRule<TModel>: IRuleEvaluatorX<TModel>
    {


        Task ValidateAsync(IEngineScope<TModel> scope, IEngineTrace trace, IRuleResult result, CancellationToken token);

    }
}
