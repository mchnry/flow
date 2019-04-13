using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic.Define;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Logic
{
    internal class DynamicEvaluator<TModel> : IRuleEvaluator<TModel>
    {
        private Func<IEngineScope<TModel>, LogicEngineTrace, IRuleResult, CancellationToken, Task> evaluator;
        private Evaluator definition;

        public DynamicEvaluator(Evaluator definition, Func<IEngineScope<TModel>, LogicEngineTrace, IRuleResult, CancellationToken, Task> evaluator)
        {
            this.evaluator = evaluator;
            this.definition = definition;
        }

        public Evaluator Definition => this.definition;

        public async Task EvaluateAsync(IEngineScope<TModel> scope, LogicEngineTrace trace, IRuleResult status, CancellationToken token)
        {
            await this.evaluator(scope, trace, status, token);
        }
    }
}
