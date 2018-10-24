using Mchnry.Flow.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Work
{
    internal class Activity
    {
        private readonly WorkflowEngine engineRef;
        private readonly Define.Activity activityDefinition;

        public Activity(WorkflowEngine engineRef, Define.Activity activityDefinition)
        {
            this.engineRef = engineRef;
            this.activityDefinition = activityDefinition;
        }

        public bool Executed { get; set; } = false;
        public List<Reaction> Reactions { get; set; }

        public async Task Execute(IStepTracer<string> tracer, CancellationToken token)
        {
            //execute action
            IAction toExecute = this.engineRef.Actions[this.activityDefinition.Action.ActionId];

            bool result = await toExecute.CompleteAsync(this.engineRef, this.activityDefinition.Action.Context, token);
            if (result)
            {
                
            }

        }


    }
}
