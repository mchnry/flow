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

        public async Task<bool> EvaluateAsync(IEngineScope<TModel> scope, LogicEngineTrace trace, CancellationToken token)
        {
            trace.TraceStep(string.Format("Preset:{0}", this.Expected));
            return this.Expected;
        }
    }
}
