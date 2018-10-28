using Mchnry.Flow.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Work
{
    internal class Activity
    {
        private readonly Engine engineRef;
        private readonly Define.Activity activityDefinition;

        public Activity(Engine engineRef, Define.Activity activityDefinition)
        {
            this.engineRef = engineRef;
            this.activityDefinition = activityDefinition;
        }

        public bool Executed { get; set; } = false;
        public List<Reaction> Reactions { get; set; }

        public async Task Execute(EngineStepTracer tracer, CancellationToken token)
        {
            //execute action
            IAction toExecute = this.engineRef.GetAction(this.activityDefinition.Action.ActionId);



            bool result = await toExecute.CompleteAsync(this.engineRef, token);


            this.Executed = true;
            if (result)
            {

            }

        }


    }
}
