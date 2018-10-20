using Mchnry.Core.Cache;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic.Define;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Logic
{
    internal class AlwaysTrueEvaluator : IRuleEvaluator
    {
        public async Task<bool> EvaluateAsync(Define.Rule definition, string processId, ICacheManager state, IValidationContainer validations, IStepTracer<string> tracer, CancellationToken token)
        {
            return true;
        }
    }
}
