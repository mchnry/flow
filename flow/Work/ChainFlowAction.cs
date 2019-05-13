using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Work.Define;

namespace Mchnry.Flow.Work
{
    internal class ChainFlowAction<TModel> : IAction<TModel>
    {
        private readonly string actionId;
        private readonly string workflowId;

        internal ChainFlowAction(string actionId, string workflowId)
        {
            this.actionId = actionId;
            this.workflowId = workflowId;
        }

        public ActionDefinition Definition => new ActionDefinition
        {
            Id = this.actionId,
            Description = $"Chain {this.workflowId}"
        };

        public async Task<bool> CompleteAsync(IEngineScope<TModel> scope, IEngineTrace trace, CancellationToken token)
        {
            TModel model = scope.GetModel();
            await scope.RunWorkflowAsync<TModel>(this.workflowId, model, token);
            return await Task.FromResult(true);
        }
    }
}
