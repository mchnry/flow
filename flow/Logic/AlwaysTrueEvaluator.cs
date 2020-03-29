using Mchnry.Flow.Cache;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic.Define;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Logic
{
    internal class AlwaysTrueEvaluator<TModel> : IRuleEvaluator<TModel>
    {
        public Evaluator Definition => new Evaluator()
        {
            Id = "true",
            Description = "Always Returns True"
        };

        public async Task EvaluateAsync(IEngineScope<TModel> scope, IEngineTrace trace, IRuleResult status, CancellationToken token)
        {
            trace.TraceStep("Always True");
            status.Pass();
        }
    }
}
