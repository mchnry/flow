using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mchnry.Flow.Diagnostics;

namespace Mchnry.Flow.Work
{
    public class DeferProxy<TModel, TSubModel> : IDeferredAction<TModel>
    {
        private readonly IDeferredAction<TSubModel> deferAction;
        private readonly IEngineScopeDefer<TSubModel> deferScope;

        public DeferProxy(IDeferredAction<TSubModel> deferAction, IEngineScopeDefer<TSubModel> scope) 
        {
            this.deferAction = deferAction;
            this.deferScope = scope;
        }

        async Task<bool> IDeferredAction<TModel>.CompleteAsync(IEngineScopeDefer<TModel> scope, IEngineTrace trace, CancellationToken token)
        {
            return await deferAction.CompleteAsync(deferScope, trace, token);
        }
    }
}
