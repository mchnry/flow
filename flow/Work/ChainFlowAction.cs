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
        private readonly IWorkflowBuilder<TModel> builder;
        private readonly string workflowId;

        internal ChainFlowAction(string actionId, IWorkflowBuilder<TModel> builder)
        {
            this.actionId = actionId;
            this.builder = builder;
            this.workflowId = builder.GetBuilder().Workflow.Id;
        }

        public async Task<bool> CompleteAsync(IEngineScope<TModel> scope, IEngineTrace trace, CancellationToken token)
        {
            TModel model = scope.GetModel();
            await scope.RunWorkflowAsync<TModel>(this.builder, model, token);

            scope.SetModel(model);

            return await Task.FromResult(true);
        }
    }
}
