using Mchnry.Flow.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Logic
{
    internal class PreSetRuleEvaluator<TModel> : IRuleEvaluator<TModel>
    {
        public PreSetRuleEvaluator(bool expected)
        {
            this.Expected = expected;
        }

        public bool Expected { get; }

        public async Task EvaluateAsync(IEngineScope<TModel> scope, LogicEngineTrace trace, IRuleResult result, CancellationToken token)
        {
            trace.TraceStep(string.Format("Preset:{0}", this.Expected));
            result.SetResult(Expected);
        }
    }
}
