using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mchnry.Flow.Diagnostics;

namespace Mchnry.Flow.Work
{
    internal class NoAction : IAction
    {
        public async Task<bool> CompleteAsync(IEngineScope scope, WorkflowEngineTrace trace, CancellationToken token)
        {
            return await Task.FromResult<bool>(true);
        }
    }
}
