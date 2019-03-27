using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Work.Define;

namespace Mchnry.Flow.Work
{
    internal class NoAction<TModel> : IAction<TModel>
    {
        public ActionDefinition Definition => new ActionDefinition()
        {
            Id = "noaction",
            Description = "Action that doesn't do anything"
        };

        public async Task<bool> CompleteAsync(IEngineScope<TModel> scope, WorkflowEngineTrace trace, CancellationToken token)
        {
            return await Task.FromResult<bool>(true);
        }
    }
}
