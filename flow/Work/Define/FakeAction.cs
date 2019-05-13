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
        public FakeAction(ActionDefinition definition)
        {
            this.Definition = definition;
        }

        public ActionDefinition Definition { get; private set; }

        public Task<bool> CompleteAsync(IEngineScope<TModel> scope, IEngineTrace trace, CancellationToken token)
        {
            trace.TraceStep(Definition.Id);
            return Task.FromResult<bool>(true);
        }
    }
}
