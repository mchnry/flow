using Mchnry.Core.Cache;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Analysis;
using Mchnry.Flow.Work;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogicDefine = Mchnry.Flow.Logic.Define;
using WorkDefine = Mchnry.Flow.Work.Define;
using Mchnry.Flow.Configuration;

namespace Mchnry.Flow
{
    public class Engine<TModel> :
        IEngineLoader<TModel>, IEngineRunner, IEngineScope<TModel>, IEngineFinalize, IEngineComplete
    {

        internal Config Configuration;
        internal bool Sanitized = false;
        private StepTracer<LintTrace> lintTracer = default(StepTracer<LintTrace>);

        //store reference to all factory created actions
        private Dictionary<string, IAction<TModel>> actions = new Dictionary<string, IAction<TModel>>();
        //store references to all factory created evaluators
        private Dictionary<string, IRuleEvaluator<TModel>> evaluators = new Dictionary<string, IRuleEvaluator<TModel>>();

        //store known evaluator results to avoid rerunning already run evaluators
        private Dictionary<string, bool?> results = new Dictionary<string, bool?>();
        //store all built expressions
        private Dictionary<string, IRule<TModel>> expressions = new Dictionary<string, IRule<TModel>>();

        //workflow definition provided by caller
        internal WorkDefine.Workflow workFlow;

        //store reference to all actions to run during finalize (if all validations succeed)
        private List<IDeferredAction<TModel>> finalize = new List<IDeferredAction<TModel>>();
        //store reference to all actions to run during finalize always
        private List<IDeferredAction<TModel>> finalizeAlways = new List<IDeferredAction<TModel>>(); //actions to complete at end regardless of validation state

        //container for all state objects
        private ICacheManager state;

        //action factory provided by caller
        private IActionFactory actionFactory;
        //evaluator factory provided by caller
        private IRuleEvaluatorFactory ruleEvaluatorFactory;

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


            this.workFlow = workFlow;
            this.Configuration = new Config();
            this.Tracer = new EngineStepTracer(new ActivityProcess("CreateEngine", ActivityStatusOptions.Engine_Loading, null));

            this.state = new MemoryCacheManager();
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


        internal EngineStepTracer Tracer { get; set; }
        internal ActivityStatusOptions CurrentActivityStatus { get; set; } = ActivityStatusOptions.Engine_Loading;

        internal LogicDefine.Rule CurrentRuleDefinition { get; set; } = null;
        LogicDefine.Rule IEngineScope<TModel>.CurrentRuleDefinition => this.CurrentRuleDefinition;

        internal WorkDefine.Activity CurrentActivity { get; set; }
        WorkDefine.Activity IEngineScope<TModel>.CurrentActivity => this.CurrentActivity;


        StepTraceNode<ActivityProcess> IEngineScope<TModel>.Process => this.Tracer.Root;

        StepTraceNode<ActivityProcess> IEngineComplete.Process => this.Tracer.Root;

        //current status of running engine
        internal EngineStatusOptions EngineStatus { get; set; } = EngineStatusOptions.NotStarted;
        EngineStatusOptions IEngineComplete.Status => this.EngineStatus;


        //store all validations created by validators/evaluators
        internal ValidationContainer ValidationContainer { get; set; } = new ValidationContainer();
        IValidationContainer IEngineComplete.Validations => this.ValidationContainer;


        void IEngineScope<TModel>.AddValidation(Validation toAdd)
        {
            //can only occure when rule is evaluating or activity is executing
            if (this.CurrentActivityStatus == ActivityStatusOptions.Action_Running)
            {
                string scope = this.CurrentActivity.Id;
                this.ValidationContainer.Scope(scope).AddValidation(toAdd);


            }
            else if (this.CurrentActivityStatus == ActivityStatusOptions.Rule_Evaluating)
            {
                string scope = this.CurrentRuleDefinition.Id;
                if (!string.IsNullOrEmpty(this.CurrentRuleDefinition.Context))
                {
                    scope = string.Format("{0}.{1}", scope, this.CurrentRuleDefinition.Context.GetHashCode().ToString());
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
            string activityId = this.CurrentActivity.Id;
            return this.state.Spawn(activityId).Read<T>(key);
        }

        TModel IEngineScope<TModel>.GetModel()
        {
            return this.state.Read<TModel>("workflowmodel");
        }

        T IEngineComplete.GetModel<T>(string key)
        {
            return this.state.Read<T>(key);
        }

        IEngineLoader<TModel> IEngineLoader<TModel>.OverrideValidation(ValidationOverride oride)
        {

            ((IValidationContainer)this.ValidationContainer).AddOverride(oride.Key, oride.Comment, oride.AuditCode);

            return this;
        }

        IEngineLoader<TModel> IEngineLoader<TModel>.SetActionFactory(IActionFactory factory)
        {
            this.actionFactory = factory;
            return this;
        }

        void IEngineScope<TModel>.SetActivityModel<T>(string key, T value)
        {
            string activityId = this.CurrentActivity.Id;
            this.state.Spawn(activityId).Insert<T>(key, value);
        }

        IEngineLoader<TModel> IEngineLoader<TModel>.SetEvaluatorFactory(IRuleEvaluatorFactory factory)
        {
            this.ruleEvaluatorFactory = factory;
            return this;
        }

        IEngineLoader<TModel> IEngineLoader<TModel>.SetModel(TModel model)
        {
            this.state.Insert<TModel>("workflowmodel", model);
            return this;
        }

        void IEngineScope<TModel>.SetModel(TModel value)
        {
            this.state.Insert<TModel>("workflowmodel", value);



        }

        IEngineRunner IEngineLoader<TModel>.Start()
        {
            
            return this;
        }


        internal bool? GetResult(LogicDefine.Rule rule)
        {
            EvaluatorKey key = new EvaluatorKey() { Id = rule.Id, Context = rule.Context };
            if (results.ContainsKey(key.ToString()))
            {
                return results[key.ToString()];
            }
            else
            {
                return null;
            }
        }

        internal void SetResult(LogicDefine.Rule rule, bool result)
        {
            EvaluatorKey key = new EvaluatorKey() { Context = rule.Context, Id = rule.Id };
            if (this.results.ContainsKey(key.ToString()))
            {
                this.results.Remove(key.ToString());

            }
            this.results.Add(key.ToString(), result);

        }

        internal IAction<TModel> GetAction(string actionId)
        {
            IAction<TModel> toReturn = default(IAction<TModel>);
            if (!this.actions.ContainsKey(actionId))
            {
                if ("*placeHolder" == actionId)
                {
                    toReturn = new NoAction<TModel>();
                }
                else
                {

                    WorkDefine.ActionDefinition def = this.workFlow.Actions.FirstOrDefault(g => g.Id.Equals(actionId));

                    try
                    {
                        toReturn = this.actionFactory.GetAction<TModel>(def);
                    } catch (System.Exception ex)
                    {
                        throw new LoadActionException(actionId, ex);
                    }

                    if (null == toReturn)
                    {
                        throw new LoadActionException(actionId);
                    }

                    this.actions.Add(actionId, toReturn);
                }
            }
            else
            {
                toReturn = this.actions[actionId];
            }
            return toReturn;
        }

        internal IRuleEvaluator<TModel> GetEvaluator(string id)
        {
            IRuleEvaluator<TModel> toReturn = default(IRuleEvaluator<TModel>);

            if (id == "true")
            {
                return new AlwaysTrueEvaluator<TModel>();
            } 

            if (!this.evaluators.ContainsKey(id))
            {
                LogicDefine.Evaluator def = this.workFlow.Evaluators.FirstOrDefault(g => g.Id.Equals(id));
                try
                {
                    toReturn = this.ruleEvaluatorFactory.GetRuleEvaluator<TModel>(def);
                } catch (System.Exception ex)
                {
                    throw new LoadEvaluatorException(id, ex);
                }

                if (null == toReturn)
                {
                    throw new LoadEvaluatorException(id);
                }

                this.evaluators.Add(def.Id, toReturn);
            }
            else
            {
                toReturn = this.evaluators[id];
            }
            return toReturn;
        }

        internal async Task<bool> Evaluate(string equationId, CancellationToken token)
        {
            IRule<TModel> toEval = this.LoadLogic(equationId);

            bool result = await toEval.EvaluateAsync(false, token);

            return result;


        }

        IEngineLoader<TModel> IEngineLoader<TModel>.AddEvaluator(string id, Func<IEngineScope<TModel>, LogicEngineTrace, CancellationToken, Task<bool>> evaluator)
        {
            this.evaluators.Add(id, new DynamicEvaluator<TModel>(evaluator));
            return this;
        }
        IEngineLoader<TModel> IEngineLoader<TModel>.AddAction(string id, Func<IEngineScope<TModel>, WorkflowEngineTrace, CancellationToken, Task<bool>> action)
        {

            this.actions.Add(id, new DynamicAction<TModel>(action));
            return this;
        }




        private Activity<TModel> LoadActivity(string activityId)
        {

            WorkDefine.Activity definition = this.workFlow.Activities.FirstOrDefault(a => a.Id == activityId);



            Activity<TModel> toReturn = new Activity<TModel>(this, definition);

            Action<Activity<TModel>, WorkDefine.Activity> LoadReactions = null;
            LoadReactions = (a, d) =>
            {
                //if (d.Action == null) d.Action = "*placeHolder";

                WorkDefine.ActionDefinition match = this.workFlow.Actions.FirstOrDefault(z => z.Id == d.Action.Id);
                //if (null == match)
                //{
                //    this.workFlow.Actions.Add(new WorkDefine.ActionDefinition()
                //    {
                //        Id = d.Action.ActionId,
                //        Description = ""
                //    });
                //}

                if (d.Reactions != null && d.Reactions.Count > 0)
                {
                    d.Reactions.ForEach(r =>
                    {
                        WorkDefine.Activity toCreatedef = this.workFlow.Activities.FirstOrDefault(z => z.Id == r.Work);

                        //if (null == toCreatedef)
                        //{
                        //    //if we can't find activity... look for a matching action.  if found, create an activity from it.
                        //    WorkDefine.ActionRef asActionRef = r.Work;
                        //    WorkDefine.ActionDefinition toCreateAction = this.workFlow.Actions.FirstOrDefault(z => z.Id == asActionRef.ActionId);

                        //    //didn't bother to add the action definition, we will create it for them
                        //    if (null == toCreateAction)
                        //    {
                        //        this.workFlow.Actions.Add(new WorkDefine.ActionDefinition()
                        //        {
                        //            Id = asActionRef.ActionId,
                        //            Description = ""
                        //        });
                        //    }


                        //    toCreatedef = new WorkDefine.Activity()
                        //    {
                        //        Action = asActionRef,
                        //        Id = Guid.NewGuid().ToString(),
                        //        Reactions = new List<WorkDefine.Reaction>()
                        //    };

                        //}

                        //if (string.IsNullOrEmpty(r.Logic))
                        //{
                        //    r.Logic = "true";
                        //}

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

            //load conventions
            
            if (this.evaluators.Count(g => g.Key == "true") == 0)
            {
                IRuleEvaluator<TModel> trueEvaluator = new AlwaysTrueEvaluator<TModel>();
                this.evaluators.Add("true", trueEvaluator);
            }
             
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
                    LogicDefine.Equation eq = this.workFlow.Equations.FirstOrDefault(g => g.Id.Equals(rule.Id));

                    IRule<TModel> first = LoadRule(eq.First, step);
                    IRule<TModel> second = LoadRule(eq.Second, step);
                    toReturn = new Expression<TModel>(rule, eq.Condition, first, second, this);
                } else
                {
                    LogicDefine.Evaluator ev = this.workFlow.Evaluators.FirstOrDefault(g => g.Id.Equals(rule.Id));
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

            LogicLinter linter = new LogicLinter(this.workFlow.Evaluators, this.workFlow.Equations);
            addIntents(linter);


            List<LogicTest> logicTests = linter.Lint();

            int lintHash = this.workFlow.GetHashCode();
            return new LintResult(this.lintTracer, logicTests, lintHash.ToString());
        }

        public WorkDefine.Workflow Workflow { get => this.workFlow; }

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
            WorkDefine.Workflow sanitized = sanitizer.Sanitize(this.workFlow);
            this.workFlow = sanitized;
            this.lintTracer = lintTrace;
            this.Sanitized = true;
        }
    }



}
