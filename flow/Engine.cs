using Mchnry.Flow.Analysis;
using Mchnry.Flow.Configuration;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Work;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LogicDefine = Mchnry.Flow.Logic.Define;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Mchnry.Flow
{
    public class Engine<TModel> :
        IEngineLoader<TModel>, IEngineRunner, IEngineScope<TModel>, IEngineFinalize, IEngineComplete
    {

        internal Config Configuration;
        internal bool Sanitized = false;

        private StepTracer<LintTrace> lintTracer = default(StepTracer<LintTrace>);
        internal virtual EngineStepTracer Tracer { get; set; }

        internal virtual ImplementationManager<TModel> ImplementationManager { get; set; }
        internal virtual WorkflowManager WorkflowManager { get; set; }
        internal virtual RunManager RunManager { get; set; }

        //store reference to all actions to run during finalize (if all validations succeed)
        private List<IDeferredAction<TModel>> finalize = new List<IDeferredAction<TModel>>();
        //store reference to all actions to run during finalize always
        private List<IDeferredAction<TModel>> finalizeAlways = new List<IDeferredAction<TModel>>(); //actions to complete at end regardless of validation state



        /// <summary>
        /// engine construcor
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// internal so that caller follows fluent construction 
        /// starting with <see cref="CreateEngine(WorkDefine.Workflow)"/>
        /// </item>
        /// </list>
        /// </remarks>
        /// <param name="workFlow">workflow definition</param>
        internal Engine(WorkDefine.Workflow workFlow)
        {

            this.Configuration = new Config();
            this.Tracer = new EngineStepTracer(new ActivityProcess("CreateEngine", ActivityStatusOptions.Engine_Loading, null));
            this.ImplementationManager = new ImplementationManager<TModel>();
            this.WorkflowManager = new WorkflowManager(workFlow);
            this.RunManager = new RunManager();

            this.Sanitized = false;
        }

        public static IEngineLoader<TModel> CreateEngine(WorkDefine.Workflow workFlow)
        {


            return new Engine<TModel>(workFlow);


        }
        public static IEngineLoader<TModel> CreateEngine(WorkDefine.Workflow workFlow, Action<Config> Configure)
        {
            Engine<TModel> toReturn = new Engine<TModel>(workFlow);


            Configure(toReturn.Configuration);

            return toReturn;

        }



        internal virtual ActivityStatusOptions CurrentActivityStatus { get; set; } = ActivityStatusOptions.Engine_Loading;


        LogicDefine.Rule IEngineScope<TModel>.CurrentRuleDefinition => this.RunManager.CurrentRuleDefinition;
        
        WorkDefine.Activity IEngineScope<TModel>.CurrentActivity => this.RunManager.CurrentActivity;


        StepTraceNode<ActivityProcess> IEngineScope<TModel>.Process => this.Tracer.Root;

        StepTraceNode<ActivityProcess> IEngineComplete.Process => this.Tracer.Root;


        EngineStatusOptions IEngineComplete.Status => this.RunManager.EngineStatus;


        //store all validations created by validators/evaluators
        internal virtual ValidationContainer ValidationContainer { get; set; } = new ValidationContainer();
        IValidationContainer IEngineComplete.Validations => this.ValidationContainer;


        void IEngineScope<TModel>.AddValidation(Validation toAdd)
        {
            //can only occure when rule is evaluating or activity is executing
            if (this.CurrentActivityStatus == ActivityStatusOptions.Action_Running)
            {
                string scope = this.RunManager.CurrentActivity.Id;
                this.ValidationContainer.Scope(scope).AddValidation(toAdd);


            }
            else if (this.CurrentActivityStatus == ActivityStatusOptions.Rule_Evaluating)
            {
                string scope = this.RunManager.CurrentRuleDefinition.Id;
                if (!string.IsNullOrEmpty(this.RunManager.CurrentRuleDefinition.Context))
                {
                    scope = string.Format("{0}.{1}", scope, this.RunManager.CurrentRuleDefinition.Context.GetHashCode().ToString());
                }
                this.ValidationContainer.Scope(scope).AddValidation(toAdd);

            }
            else
            {
                ((IValidationContainer)this.ValidationContainer).AddValidation(toAdd);
            }

        }

        void IEngineScope<TModel>.Defer(IDeferredAction<TModel> action, bool onlyIfValidationsResolved)
        {
            if (onlyIfValidationsResolved)
            {
                this.finalize.Add(action);
            }
            else
            {
                this.finalizeAlways.Add(action);
            }
        }

        async Task<IEngineFinalize> IEngineRunner.ExecuteAsync(string activityId, CancellationToken token)
        {

            if (!this.Sanitized)
            {
                this.Sanitize();
            }
            Activity<TModel> toLoad = this.LoadActivity(activityId);

            this.Tracer.CurrentStep = this.Tracer.TraceStep(this.Tracer.Root, new ActivityProcess("Execute", ActivityStatusOptions.Engine_Begin, null));
            await (toLoad.Execute(this.Tracer, token));

            return this;

        }

        async Task<IEngineComplete> IEngineRunner.ExecuteAutoFinalizeAsync(string activityId, CancellationToken token)
        {
            IEngineFinalize finalizer = await ((IEngineRunner)this).ExecuteAsync(activityId, token);
            return await finalizer.FinalizeAsync(token);

        }

        async Task<IEngineComplete> IEngineFinalize.FinalizeAsync(CancellationToken token)
        {
            StepTraceNode<ActivityProcess> mark = this.Tracer.CurrentStep = this.Tracer.TraceStep(this.Tracer.Root, new ActivityProcess("Finalize", ActivityStatusOptions.Engine_Finalizing, null));
            if (this.ValidationContainer.ResolveValidations())
            {


                foreach (IDeferredAction<TModel> toFinalize in this.finalize)
                {
                    this.Tracer.CurrentStep = this.Tracer.TraceStep(mark, new ActivityProcess(toFinalize.Id, ActivityStatusOptions.Action_Running, null));

                    await toFinalize.CompleteAsync(this, new WorkflowEngineTrace(this.Tracer), token);

                }


            }

            foreach (IDeferredAction<TModel> toFinalize in this.finalizeAlways)
            {
                this.Tracer.CurrentStep = this.Tracer.TraceStep(mark, new ActivityProcess(toFinalize.Id, ActivityStatusOptions.Action_Running, null));

                await toFinalize.CompleteAsync(this, new WorkflowEngineTrace(this.Tracer), token);
            }

            return this;
        }

        T IEngineScope<TModel>.GetActivityModel<T>(string key)
        {
            string activityId = this.RunManager.CurrentActivity.Id;
            return this.Configuration.Cache.Spawn(activityId).Read<T>(key);
        }

        T IEngineScope<TModel>.GetScopeModel<T>(string key)
        {
            string activityId = this.RunManager.CurrentActivity.Id;
            return this.Configuration.Cache.Read<T>(key);
        }

        TModel IEngineScope<TModel>.GetModel()
        {
            return this.Configuration.Cache.Read<TModel>("workflowmodel");
        }

        T IEngineComplete.GetModel<T>(string key)
        {
            return this.Configuration.Cache.Read<T>(key);
        }

        IEngineLoader<TModel> IEngineLoader<TModel>.OverrideValidation(ValidationOverride oride)
        {

            ((IValidationContainer)this.ValidationContainer).AddOverride(oride.Key, oride.Comment, oride.AuditCode);

            return this;
        }

        IEngineLoader<TModel> IEngineLoader<TModel>.SetActionFactory(IActionFactory factory)
        {
            this.ImplementationManager.ActionFactory = factory;
            return this;
        }

        void IEngineScope<TModel>.SetActivityModel<T>(string key, T value)
        {
            string activityId = this.RunManager.CurrentActivity.Id;
            this.Configuration.Cache.Spawn(activityId).Insert<T>(key, value);
        }

        void IEngineScope<TModel>.SetScopeModel<T>(string key, T value)
        {

            this.Configuration.Cache.Insert<T>(key, value);
        }

        IEngineLoader<TModel> IEngineLoader<TModel>.SetEvaluatorFactory(IRuleEvaluatorFactory factory)
        {
            this.ImplementationManager.EvaluatorFactory = factory;
            return this;
        }

        IEngineLoader<TModel> IEngineLoader<TModel>.SetModel(TModel model)
        {
            this.Configuration.Cache.Insert<TModel>("workflowmodel", model);
            return this;
        }

        void IEngineScope<TModel>.SetModel(TModel value)
        {
            this.Configuration.Cache.Insert<TModel>("workflowmodel", value);



        }

        IEngineRunner IEngineLoader<TModel>.Start()
        {

            return this;
        }








        internal async Task<bool> Evaluate(string equationId, CancellationToken token)
        {
            IRule<TModel> toEval = this.LoadLogic(equationId);

            bool result = await toEval.EvaluateAsync(false, token);

            return result;


        }

        IEngineLoader<TModel> IEngineLoader<TModel>.AddEvaluator(string id, Func<IEngineScope<TModel>, LogicEngineTrace, CancellationToken, Task<bool>> evaluator)
        {
            this.ImplementationManager.AddEvaluator(id, evaluator);
            return this;
        }
        IEngineLoader<TModel> IEngineLoader<TModel>.AddAction(string id, Func<IEngineScope<TModel>, WorkflowEngineTrace, CancellationToken, Task<bool>> action)
        {

            this.ImplementationManager.AddAction(id, action);
            return this;
        }




        private Activity<TModel> LoadActivity(string activityId)
        {

            WorkDefine.Activity definition = this.WorkflowManager.GetActivity(activityId);



            Activity<TModel> toReturn = new Activity<TModel>(this, definition);

            Action<Activity<TModel>, WorkDefine.Activity> LoadReactions = null;
            LoadReactions = (a, d) =>
            {


                if (d.Reactions != null && d.Reactions.Count > 0)
                {
                    d.Reactions.ForEach(r =>
                    {
                        WorkDefine.ActionRef work = r.Work;
                        WorkDefine.Activity toCreatedef = this.WorkflowManager.GetActivity(work.Id);



                        LoadLogic(r.Logic);

                        if (null == a.Reactions)
                        {
                            a.Reactions = new List<Reaction<TModel>>();
                        }
                        Activity<TModel> toCreate = new Activity<TModel>(this, toCreatedef);
                        LoadReactions(toCreate, toCreatedef);
                        a.Reactions.Add(new Reaction<TModel>(r.Logic, toCreate));
                    });
                }

            };

            LoadReactions(toReturn, definition);

            return toReturn;

        }

        private IRule<TModel> LoadLogic(string equationId)
        {
            StepTracer<LintTrace> trace = new StepTracer<LintTrace>();
            StepTraceNode<LintTrace> root = trace.TraceFirst(new LintTrace(LintStatusOptions.Loading, "Loading Logic", equationId));



            //LogicDefine.Evaluator trueDef = this.workFlow.Evaluators.FirstOrDefault(z => z.Id == "true");
            //if (null == trueDef)
            //{
            //    trueDef = new LogicDefine.Evaluator() { Id = "true", Description = "Always True" };
            //    this.workFlow.Evaluators.Add(trueDef);
            //}


            //List<string> lefts = (from e in this.workFlow.Equations
            //                      where e.First != null
            //                      select e.First.Id).ToList();

            //List<string> rights = (from e in this.workFlow.Equations
            //                       where null != e.Second
            //                       select e.Second.Id).ToList();

            //List<string> roots = (from e in this.workFlow.Equations
            //                      where !lefts.Contains(e.Id) && !rights.Contains(e.Id)
            //                      select e.Id).ToList();


            //Lint.... make sure we have everything we need first.
            Func<LogicDefine.Rule, StepTraceNode<LintTrace>, IRule<TModel>> LoadRule = null;
            LoadRule = (rule, parentStep) =>
            {
                StepTraceNode<LintTrace> step = trace.TraceNext(parentStep, new LintTrace(LintStatusOptions.Inspecting, "Inspecting Rule", rule.Id));
                IRule<TModel> toReturn = null;
                //if id is an equation, we are creating an expression

                //since we've formalized convention, we can just check that
                if (ConventionHelper.MatchesConvention(NamePrefixOptions.Equation, rule.Id, this.Configuration.Convention))
                {
                    LogicDefine.Equation eq = this.WorkflowManager.GetEquation(rule.Id);

                    IRule<TModel> first = LoadRule(eq.First, step);
                    IRule<TModel> second = LoadRule(eq.Second, step);
                    toReturn = new Expression<TModel>(rule, eq.Condition, first, second, this);
                }
                else
                {
                    LogicDefine.Evaluator ev = this.WorkflowManager.GetEvaluator(rule.Id);
                    toReturn = new Rule<TModel>(rule, this);
                }





                return toReturn;
            };




            LogicDefine.Rule eqRule = equationId;
            IRule<TModel> loaded = LoadRule(eqRule, root);

            return loaded;

        }


        public LintResult Lint(Action<LogicLinter> addIntents)
        {
            if (!this.Sanitized)
            {
                this.Sanitize();
            }

            LogicLinter linter = new LogicLinter(this.WorkflowManager);
            addIntents(linter);


            List<LogicTest> logicTests = linter.Lint();

            int lintHash = this.WorkflowManager.WorkFlow.GetHashCode();
            return new LintResult(this.lintTracer, logicTests, lintHash.ToString());
        }

        public WorkDefine.Workflow Workflow { get => this.WorkflowManager.WorkFlow; }

        internal void Sanitize()
        {
            StepTracer<LintTrace> lintTrace = new StepTracer<LintTrace>();
            lintTrace.TraceFirst(new LintTrace(LintStatusOptions.Linting, "Starting Lint"));

            //before sanitize
            //follows convention

            //after sanitize
            //any unused evaluators or actions

            //identify main activities

            //get test cases
            //look for irrelevant evaluators (i.e., doesnt matter in any case if true or false)

            Sanitizer sanitizer = new Sanitizer(lintTrace, this.Configuration);
            WorkDefine.Workflow sanitized = sanitizer.Sanitize(this.WorkflowManager.WorkFlow);
            this.WorkflowManager.WorkFlow = sanitized;
            this.lintTracer = lintTrace;
            this.Sanitized = true;
        }
    }



}
