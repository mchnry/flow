using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Work.Define;

namespace Mchnry.Flow.Work
{

    //set state
    //execute

    public interface IWorkflowEngineLoader
    {
        IWorkflowEngineLoader SetModel<T>(string key, T model);
        IWorkflowEngineLoader OverrideValidations(ValidationContainer overrides);
        Task<IWorkflowEngineComplete> ExecuteAutoFinalizeAsync(string activityId, CancellationToken token);
        Task<IWorkflowEngineFinalize> ExecuteAsync(string activityId, CancellationToken token);
        
    }


    public interface IWorkflowEngineFinalize
    {
        Task<IWorkflowEngineComplete> FinalizeAsync(CancellationToken token);
        
    }
    public interface IWorkflowEngineComplete
    {
        WorkflowEngineStatusOptions Status { get; }
        ValidationContainer Validations { get; }
        StepTraceNode<ActivityProcess> Process { get; }
    }
    

    public class WorkflowEngine : IWorkflowEngineScope, IWorkflowEngineFinalize, IWorkflowEngineLoader, IWorkflowEngineComplete
    {
        private readonly Workflow workflow;
        private readonly IActionFactory actionFactory;
        private readonly Logic.IRuleEvaluatorFactory ruleEvaluatorFactory;
        private WorkflowEngineStatusOptions status;
        private ValidationContainer validations;
        private List<IAction> finalize = new List<IAction>();
        private List<IAction> finalizeAlways = new List<IAction>(); //actions to complete at end regardless of validation state

        internal Dictionary<string, IAction> Actions { get; set; }

        internal EngineStepTracer processTracer;

        private WorkflowEngine(Define.Workflow workflow, IActionFactory actionFactory, Logic.IRuleEvaluatorFactory ruleEvaluatorFactory)
        {
            this.workflow = workflow;
            this.actionFactory = actionFactory;
            this.ruleEvaluatorFactory = ruleEvaluatorFactory;
            this.validations = new ValidationContainer();
            this.status = WorkflowEngineStatusOptions.NotStarted;

        }

        public static IWorkflowEngineLoader CreateWorkflowEngine(Define.Workflow workflow, IActionFactory actionFactory, Logic.IRuleEvaluatorFactory ruleEvaluatorFactory)
        {
            WorkflowEngine toReturn = new WorkflowEngine(workflow, actionFactory, ruleEvaluatorFactory);

            return toReturn;
        }





        StepTraceNode<ActivityProcess> IWorkflowEngineScope.CurrentProcess => this.processTracer.CurrentStep;

        IActionFactory IWorkflowEngineScope.ActionFactory => throw new NotImplementedException();


        void IWorkflowEngineScope.Defer(IAction action, bool onlyIfValidationsResolved)
        {
            throw new NotImplementedException();
        }

        T IWorkflowEngineScope.GetStateObject<T>(ActivityProcess currentProcess, string key)
        {
            throw new NotImplementedException();
        }

        T IWorkflowEngineScope.GetStateObject<T>(string key)
        {
            throw new NotImplementedException();
        }

        //void IWorkflowEngine.Inject(Activity activityDefinition, object model)
        //{
        //    throw new NotImplementedException();
        //}

        void IWorkflowEngineScope.SetStateObject<T>(ActivityProcess currentProcess, string key, T toSave)
        {
            throw new NotImplementedException();
        }

        void IWorkflowEngineScope.SetStateObject<T>(string key, T toSave)
        {
            throw new NotImplementedException();
        }

        async Task<IWorkflowEngineComplete> IWorkflowEngineFinalize.FinalizeAsync(CancellationToken token)
        {
            await this.FinalizeAsync(token);
            return this;
        }

        IWorkflowEngineLoader IWorkflowEngineLoader.SetModel<T>(string key, T model)
        {
            ((IWorkflowEngineScope)this).SetStateObject<T>(key, model);
            return this;
        }

        async Task<IWorkflowEngineComplete> IWorkflowEngineLoader.ExecuteAutoFinalizeAsync(string activityId, CancellationToken token)
        {
            await this.ExecuteAsync(activityId, token);
            await this.FinalizeAsync(token);
            return this;
        }

        async Task<IWorkflowEngineFinalize> IWorkflowEngineLoader.ExecuteAsync(string activityId, CancellationToken token)
        {
            await this.ExecuteAsync(activityId, token);
            return this;
        }

        IWorkflowEngineLoader IWorkflowEngineLoader.OverrideValidations(ValidationContainer overrides)
        {
            if (overrides != null)
            {
                this.validations = overrides;
            }
            return this;
        }

        WorkflowEngineStatusOptions IWorkflowEngineComplete.Status => this.status;

        ValidationContainer IWorkflowEngineComplete.Validations { get => this.validations; }

        StepTraceNode<ActivityProcess> IWorkflowEngineComplete.Process => this.processTracer.Root;
        StepTraceNode<ActivityProcess> IWorkflowEngineScope.Process => this.processTracer.Root;

        private async Task FinalizeAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }
        private async Task ExecuteAsync(string activityId, CancellationToken token)
        {
            Activity toRun = this.LoadActivity(activityId);

            await toRun.Execute(this.processTracer, token);

        }

        private Activity LoadActivity(string activityId)
        {

            Define.Activity definition = this.workflow.Activities.FirstOrDefault(a => a.Id == activityId);

            Activity toReturn = new Activity(this, definition);

            Action<Activity, Define.Activity> LoadReactions = null;
            LoadReactions = (a, d) =>
            {
                if (d.Reactions != null && d.Reactions.Count > 0)
                {
                    d.Reactions.ForEach(r =>
                    {
                        Define.Activity toCreatedef = this.workflow.Activities.FirstOrDefault(z => z.Id == r.ActivityId);
                        a.Reactions = new List<Reaction>();
                        Activity toCreate = new Activity(this, toCreatedef);
                        LoadReactions(toCreate, toCreatedef);
                        a.Reactions.Add(new Reaction(r.EquationId, toCreate));
                    });
                }

            };

            LoadReactions(toReturn, definition);

            return toReturn;

        }

    }
}
