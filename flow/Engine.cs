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
    public class Engine :
        IEngineLoader, IEngineRunner, IEngineScope, IEngineFinalize, IEngineComplete
    {

        internal Config Configuration;

        //store reference to all factory created actions
        private Dictionary<string, IAction> actions = new Dictionary<string, IAction>();
        //store references to all factory created evaluators
        private Dictionary<string, IRuleEvaluator> evaluators = new Dictionary<string, IRuleEvaluator>();

        //store known evaluator results to avoid rerunning already run evaluators
        private Dictionary<string, bool?> results = new Dictionary<string, bool?>();
        //store all built expressions
        private Dictionary<string, IRule> expressions = new Dictionary<string, IRule>();

        //workflow definition provided by caller
        internal WorkDefine.Workflow workFlow;

        //current status of running engine
        private EngineStatusOptions engineStatus = EngineStatusOptions.NotStarted;

        //store all validations created by validators/evaluators
        private ValidationContainer container = new ValidationContainer();

        //store reference to all actions to run during finalize (if all validations succeed)
        private List<IDeferredAction> finalize = new List<IDeferredAction>();
        //store reference to all actions to run during finalize always
        private List<IDeferredAction> finalizeAlways = new List<IDeferredAction>(); //actions to complete at end regardless of validation state

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

        }
        public static IEngineLoader CreateEngine(WorkDefine.Workflow workFlow)
        {


            return new Engine(workFlow);


        }
        public static IEngineLoader CreateEngine(WorkDefine.Workflow workFlow, Action<Config> Configure)
        {
            Engine toReturn = new Engine(workFlow);


            Configure(toReturn.Configuration);

            return toReturn;

        }


        internal EngineStepTracer Tracer { get; set; }
        internal ActivityStatusOptions CurrentActivityStatus { get; set; } = ActivityStatusOptions.Engine_Loading;

        internal LogicDefine.Rule CurrentRuleDefinition { get; set; } = null;
        LogicDefine.Rule IEngineScope.CurrentRuleDefinition => this.CurrentRuleDefinition;

        internal WorkDefine.Activity CurrentActivity { get; set; }
        WorkDefine.Activity IEngineScope.CurrentActivity => this.CurrentActivity;

        StepTraceNode<ActivityProcess> IEngineScope.Process => this.Tracer.Root;

        StepTraceNode<ActivityProcess> IEngineComplete.Process => this.Tracer.Root;

        EngineStatusOptions IEngineComplete.Status => this.engineStatus;

        IValidationContainer IEngineComplete.Validations => this.container;


        void IEngineScope.AddValidation(Validation toAdd)
        {
            //can only occure when rule is evaluating or activity is executing
            if (this.CurrentActivityStatus == ActivityStatusOptions.Action_Running)
            {
                string scope = this.CurrentActivity.Id;
                this.container.Scope(scope).AddValidation(toAdd);


            }
            else if (this.CurrentActivityStatus == ActivityStatusOptions.Rule_Evaluating)
            {
                string scope = this.CurrentRuleDefinition.Id;
                if (!string.IsNullOrEmpty(this.CurrentRuleDefinition.Context))
                {
                    scope = string.Format("{0}.{1}", scope, this.CurrentRuleDefinition.Context.GetHashCode().ToString());
                }
                this.container.Scope(scope).AddValidation(toAdd);

            }
            else
            {
                ((IValidationContainer)this.container).AddValidation(toAdd);
            }

        }

        void IEngineScope.Defer(IDeferredAction action, bool onlyIfValidationsResolved)
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
            Activity toLoad = this.LoadActivity(activityId);

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
            if (this.container.ResolveValidations())
            {


                foreach (IDeferredAction toFinalize in this.finalize)
                {
                    this.Tracer.CurrentStep = this.Tracer.TraceStep(mark, new ActivityProcess(toFinalize.Id, ActivityStatusOptions.Action_Running, null));

                    await toFinalize.CompleteAsync(this, new WorkflowEngineTrace(this.Tracer), token);

                }


            }

            foreach (IDeferredAction toFinalize in this.finalizeAlways)
            {
                this.Tracer.CurrentStep = this.Tracer.TraceStep(mark, new ActivityProcess(toFinalize.Id, ActivityStatusOptions.Action_Running, null));

                await toFinalize.CompleteAsync(this, new WorkflowEngineTrace(this.Tracer), token);
            }

            return this;
        }

        T IEngineScope.GetActivityModel<T>(string key)
        {
            string activityId = this.CurrentActivity.Id;
            return this.state.Spawn(activityId).Read<T>(key);
        }

        T IEngineScope.GetModel<T>(string key)
        {
            return this.state.Read<T>(key);
        }

        T IEngineComplete.GetModel<T>(string key)
        {
            return this.state.Read<T>(key);
        }

        IEngineLoader IEngineLoader.OverrideValidation(ValidationOverride oride)
        {

            ((IValidationContainer)this.container).AddOverride(oride.Key, oride.Comment, oride.AuditCode);

            return this;
        }

        IEngineLoader IEngineLoader.SetActionFactory(IActionFactory factory)
        {
            this.actionFactory = factory;
            return this;
        }

        void IEngineScope.SetActivityModel<T>(string key, T value)
        {
            string activityId = this.CurrentActivity.Id;
            this.state.Spawn(activityId).Insert<T>(key, value);
        }

        IEngineLoader IEngineLoader.SetEvaluatorFactory(IRuleEvaluatorFactory factory)
        {
            this.ruleEvaluatorFactory = factory;
            return this;
        }

        IEngineLoader IEngineLoader.SetModel<T>(string key, T model)
        {
            this.state.Insert<T>(key, model);
            return this;
        }

        void IEngineScope.SetModel<T>(string key, T value)
        {
            this.state.Insert<T>(key, value);



        }

        IEngineRunner IEngineLoader.Start()
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

        internal IAction GetAction(string actionId)
        {
            IAction toReturn = default(IAction);
            if (!this.actions.ContainsKey(actionId))
            {
                if ("*placeHolder" == actionId)
                {
                    toReturn = new NoAction();
                }
                else
                {

                    WorkDefine.ActionDefinition def = this.workFlow.Actions.FirstOrDefault(g => g.Id.Equals(actionId));

                    try
                    {
                        toReturn = this.actionFactory.GetAction(def);
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

        internal IRuleEvaluator GetEvaluator(string id)
        {
            IRuleEvaluator toReturn = default(IRuleEvaluator);

            if (id == "true")
            {
                return new AlwaysTrueEvaluator();
            } 

            if (!this.evaluators.ContainsKey(id))
            {
                LogicDefine.Evaluator def = this.workFlow.Evaluators.FirstOrDefault(g => g.Id.Equals(id));
                try
                {
                    toReturn = this.ruleEvaluatorFactory.GetRuleEvaluator(def);
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
            IRule toEval = this.LoadLogic(equationId);

            bool result = await toEval.EvaluateAsync(false, token);

            return result;


        }


        private Activity LoadActivity(string activityId)
        {

            WorkDefine.Activity definition = this.workFlow.Activities.FirstOrDefault(a => a.Id == activityId);



            Activity toReturn = new Activity(this, definition);

            Action<Activity, WorkDefine.Activity> LoadReactions = null;
            LoadReactions = (a, d) =>
            {
                //if (d.Action == null) d.Action = "*placeHolder";

                WorkDefine.ActionDefinition match = this.workFlow.Actions.FirstOrDefault(z => z.Id == d.Action.ActionId);
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
                            a.Reactions = new List<Reaction>();
                        }
                        Activity toCreate = new Activity(this, toCreatedef);
                        LoadReactions(toCreate, toCreatedef);
                        a.Reactions.Add(new Reaction(r.Logic, toCreate));
                    });
                }

            };

            LoadReactions(toReturn, definition);

            return toReturn;

        }

        private IRule LoadLogic(string equationId)
        {
            StepTracer<LintTrace> trace = new StepTracer<LintTrace>();
            StepTraceNode<LintTrace> root = trace.TraceFirst(new LintTrace(LintStatusOptions.Loading, "Loading Logic", equationId));

            //load conventions
            IRuleEvaluator trueEvaluator = new AlwaysTrueEvaluator();
            //            this.evaluators.Add("true", trueEvaluator);
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
            Func<LogicDefine.Rule, StepTraceNode<LintTrace>, IRule> LoadRule = null;
            LoadRule = (rule, parentStep) =>
            {
                StepTraceNode<LintTrace> step = trace.TraceNext(parentStep, new LintTrace(LintStatusOptions.Inspecting, "Inspecting Rule", rule.Id));
                IRule toReturn = null;
                //if id is an equation, we are creating an expression
                LogicDefine.Equation eq = this.workFlow.Equations.FirstOrDefault(g => g.Id.Equals(rule.Id));

                IRule first = LoadRule(eq.First, step);
                IRule second = LoadRule(eq.Second, step);
                toReturn = new Expression(rule, eq.Condition, first, second, this);

                //if (null != eq)
                //{
                //    IRule first = null, second = null;
                //    if (null != eq.First)
                //    {
                //        first = LoadRule(eq.First, step);
                //    }
                //    else
                //    {
                //        //should never hit this... sanitizer should have ensured the def exists
                //        first = new Rule(
                //            new LogicDefine.Rule() { Id = "true", Context = string.Empty, TrueCondition = true },
                //            this);
                //    }

                //    if (null != eq.Second)
                //    {
                //        second = LoadRule(eq.Second.Id, step);
                //    }
                //    else
                //    {

                //        second = new Rule(
                //            new LogicDefine.Rule() { Id = "true", Context = string.Empty, TrueCondition = true },
                //            this);
                //    }
                //    toReturn = new Expression(rule, eq.Condition, first, second, this);

                //}
                //else
                //{
                //    LogicDefine.Evaluator ev = this.workFlow.Evaluators.FirstOrDefault(g => g.Id.Equals(rule.Id));

                //    if (null == ev)
                //    {
                //        this.workFlow.Evaluators.Add(new LogicDefine.Evaluator()
                //        {
                //            Id = rule.Id,
                //            Description = string.Empty
                //        });
                //    }


                //    toReturn = new Rule(rule, this);

                //}


                return toReturn;
            };




            LogicDefine.Rule eqRule = equationId;
            IRule loaded = LoadRule(eqRule, root);

            return loaded;

        }


        public List<LogicTest> Lint(Action<LogicLinter> addIntents)
        {
            StepTracer<LintTrace> lintTrace = new StepTracer<LintTrace>();
            lintTrace.TraceFirst(new LintTrace(LintStatusOptions.Linting, "Starting Lint"));


            //follows convention


            Sanitizer sanitizer = new Sanitizer(lintTrace, this.Configuration);
            WorkDefine.Workflow sanitized = sanitizer.Sanitize(this.workFlow);


            //LogicLinter linter = new LogicLinter(this.workFlow.Evaluators, this.workFlow.Equations);
            //addIntents(linter);


            //List<LogicTest> toReturn = linter.Lint();


            return null;
        }

        public WorkDefine.Workflow Workflow { get => this.workFlow; }
    }
}
