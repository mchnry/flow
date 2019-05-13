using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic.Define;
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

        public Evaluator Definition => new Evaluator()
        {
            Id = "preset",
            Description = "Preset rule evaluator for testing"
        };

        public async Task EvaluateAsync(IEngineScope<TModel> scope, IEngineTrace trace, IRuleResult result, CancellationToken token)
        {
            trace.TraceStep(string.Format("Preset:{0}", this.Expected));
            result.SetResult(Expected);
        }
    }
}
