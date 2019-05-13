using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Work.Define;

namespace Mchnry.Flow.Work
{
    internal class DynamicAction<TModel> : IAction<TModel>
    {
        private Func<IEngineScope<TModel>, IEngineTrace, CancellationToken, Task<bool>> action;
        private ActionDefinition definition;

        public DynamicAction(ActionDefinition definition,Func<IEngineScope<TModel>, IEngineTrace, CancellationToken, Task<bool>> action)
        {
            this.action = action;
            this.definition = definition;
        }

        public ActionDefinition Definition => this.definition;

        public async Task<bool> CompleteAsync(IEngineScope<TModel> scope, IEngineTrace trace, CancellationToken token)
        {
            return await this.action(scope, trace, token);
        }
    }
}
