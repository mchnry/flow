using Mchnry.Flow.Cache;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic.Define;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Logic
{
    internal class AlwaysTrueEvaluator<TModel> : IEvaluatorRule<TModel>
    {
        public Evaluator Definition => new Evaluator()
        {
            Id = "true",
            Description = "Always Returns True"
        };

        public async Task<bool> EvaluateAsync(IEngineScope<TModel> scope, IEngineTrace trace, CancellationToken token)
        {
            trace.TraceStep("Always True");
            return await Task.FromResult(true);
        }
    }
}
