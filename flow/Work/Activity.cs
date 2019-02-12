using Mchnry.Flow.Diagnostics;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Mchnry.Flow.Work
{
    internal class Activity<TModel>
    {
        private readonly Engine<TModel> engineRef;
        private readonly Define.Activity activityDefinition;

        public Activity(Engine<TModel> engineRef, Define.Activity activityDefinition)
        {
            this.engineRef = engineRef;
            this.activityDefinition = activityDefinition;
        }

        public bool Executed { get; set; } = false;
        public List<Reaction<TModel>> Reactions { get; set; } = new List<Reaction<TModel>>();

        public async Task Execute(EngineStepTracer tracer, CancellationToken token)
        {
            this.engineRef.RunManager.CurrentActivity = this.activityDefinition;
            this.engineRef.CurrentActivityStatus = ActivityStatusOptions.Action_Running;
            bool result = false;
            //execute action
            IAction<TModel> toExecute = this.engineRef.ImplementationManager.GetAction(this.activityDefinition.Action.Id);
      


            StepTraceNode<ActivityProcess> mark = this.engineRef.Tracer.CurrentStep = this.engineRef.Tracer.TraceStep(
                new ActivityProcess(this.activityDefinition.Id, ActivityStatusOptions.Action_Running, null));


            try
            {
                Stopwatch t = new Stopwatch(); t.Start();

                result = await toExecute.CompleteAsync(this.engineRef, new WorkflowEngineTrace(this.engineRef.Tracer), token);

                t.Stop();

                this.engineRef.CurrentActivityStatus = ActivityStatusOptions.Action_Completed;
                this.engineRef.Tracer.TraceStep(new ActivityProcess(this.activityDefinition.Action.Id, ActivityStatusOptions.Action_Completed, null, t.Elapsed));

            }
            catch (System.Exception ex)
            {
                this.engineRef.CurrentActivityStatus = ActivityStatusOptions.Action_Failed;
                this.engineRef.Tracer.TraceStep(new ActivityProcess(this.activityDefinition.Action.Id, ActivityStatusOptions.Action_Failed, ex.Message));
            }

            this.Executed = true;

            //if i have reactions, loop through each and run
            if (this.Reactions.Count > 0)
            {
                StepTraceNode<ActivityProcess> reactionMark = this.engineRef.Tracer.CurrentStep = this.engineRef.Tracer.TraceStep(
                new ActivityProcess(this.activityDefinition.Id, ActivityStatusOptions.Action_Running, "React"));
                foreach (Reaction<TModel> react in this.Reactions)
                {
                    this.engineRef.Tracer.CurrentStep = this.engineRef.Tracer.TraceStep(reactionMark,
                        new ActivityProcess(react.Activity.activityDefinition.Id, ActivityStatusOptions.Action_Running, "Evaluating"));
                    bool doReact = await this.engineRef.Evaluate(react.LogicEquationId, token);

                    if (doReact)
                    {
                        this.engineRef.Tracer.CurrentStep = this.engineRef.Tracer.TraceStep(reactionMark,
                            new ActivityProcess(react.Activity.activityDefinition.Id, ActivityStatusOptions.Action_Running, "Reacting"));
                        await react.Activity.Execute(this.engineRef.Tracer, token);
                    }
                }
            }
        }


    }
}
