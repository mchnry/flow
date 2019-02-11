using Mchnry.Flow.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Logic
{
    internal class DynamicEvaluator<TModel>: IRuleEvaluator<TModel>
    {
        private Func<IEngineScope<TModel>, LogicEngineTrace, CancellationToken, Task<bool>> evaluator;
        public DynamicEvaluator(Func<IEngineScope<TModel>, LogicEngineTrace, CancellationToken, Task<bool>> evaluator)
        {
            this.evaluator = evaluator;
        }

        public async Task<bool> EvaluateAsync(IEngineScope<TModel> scope, LogicEngineTrace trace, CancellationToken token)
        {
            return await this.evaluator(scope, trace, token);
        }
    }
}
