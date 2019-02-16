using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mchnry.Flow.Diagnostics;

namespace Mchnry.Flow.Work.Define
{
    internal class FakeAction<TModel> : IAction<TModel>
    {
        private readonly ActionDefinition definition;

        public FakeAction(ActionDefinition definition)
        {
            this.definition = definition;
        }
        public Task<bool> CompleteAsync(IEngineScope<TModel> scope, WorkflowEngineTrace trace, CancellationToken token)
        {
            trace.TraceStep(definition.Id);
            return Task.FromResult<bool>(true);
        }
    }
}
