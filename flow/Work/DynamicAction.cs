using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mchnry.Flow.Diagnostics;

namespace Mchnry.Flow.Work
{
    internal class DynamicAction<TModel> : IAction<TModel>
    {
        private Func<IEngineScope<TModel>, WorkflowEngineTrace, CancellationToken, Task<bool>> action;

        public DynamicAction(Func<IEngineScope<TModel>, WorkflowEngineTrace, CancellationToken, Task<bool>> action) {
            this.action = action;
        }
        
        public async Task<bool> CompleteAsync(IEngineScope<TModel> scope, WorkflowEngineTrace trace, CancellationToken token)
        {
            return await this.action(scope, trace, token);
        }
    }
}
