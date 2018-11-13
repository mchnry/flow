using Mchnry.Core.Cache;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Test;
using Mchnry.Flow.Work;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LogicDefine = Mchnry.Flow.Logic.Define;
using WorkDefine = Mchnry.Flow.Work.Define;

namespace Mchnry.Flow
{
    public class Engine :
        IEngineLoader, IEngineRunner, IEngineScope, IEngineFinalize, IEngineComplete
    {

        private Dictionary<string, IRuleEvaluator> evaluators = new Dictionary<string, IRuleEvaluator>();
        private Dictionary<string, bool?> results = new Dictionary<string, bool?>();
        private Dictionary<string, IRule> expressions = new Dictionary<string, IRule>();
        private Dictionary<string, IAction> actions = new Dictionary<string, IAction>();
        internal WorkDefine.Workflow workFlow;
        private EngineStatusOptions engineStatus = EngineStatusOptions.NotStarted;
        private ValidationContainer container = new ValidationContainer();

        private List<IDeferredAction> finalize = new List<IDeferredAction>();
        private List<IDeferredAction> finalizeAlways = new List<IDeferredAction>(); //actions to complete at end regardless of validation state
        private ICacheManager state;
        private IActionFactory actionFactory;
        private IRuleEvaluatorFactory ruleEvaluatorFactory;


        internal Engine(WorkDefine.Workflow workFlow)
        {
            this.workFlow = workFlow;
            this.Tracer = new EngineStepTracer(new ActivityProcess("CreateEngine", ActivityStatusOptions.Engine_Loading, null));

            this.state = new MemoryCacheManager();

        }
        public static IEngineLoader CreateEngine(WorkDefine.Workflow workFlow)
        {


            return new Engine(workFlow);
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
            StepTraceNode<ActivityProcess> mark =  this.Tracer.CurrentStep = this.Tracer.TraceStep(this.Tracer.Root, new ActivityProcess("Finalize", ActivityStatusOptions.Engine_Finalizing, null));
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
            if (results.ContainsKey(key.ToString())) {
                return results[key.ToString()];
            } else
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
                    toReturn = this.actionFactory.GetAction(def);
                    this.actions.Add(actionId, toReturn);
                }
            } else
            {
                toReturn = this.actions[actionId];
            }
            return toReturn;
        }

        internal IRuleEvaluator GetEvaluator(string id)
        {
            IRuleEvaluator toReturn = default(IRuleEvaluator);
            if (!this.evaluators.ContainsKey(id))
            {
                LogicDefine.Evaluator def = this.workFlow.Evaluators.FirstOrDefault(g => g.Id.Equals(id));
                toReturn = this.ruleEvaluatorFactory.GetRuleEvaluator(def);
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
                if (d.Action == null) d.Action = "*placeHolder";
                if (d.Reactions != null && d.Reactions.Count > 0)
                {
                    d.Reactions.ForEach(r =>
                    {
                        WorkDefine.Activity toCreatedef = this.workFlow.Activities.FirstOrDefault(z => z.Id == r.ActivityId);

                        if (null == toCreatedef)
                        {
                            //if we can't find activity... look for a matching action.  if found, create an activity from it.
                            WorkDefine.ActionRef asActionRef = r.ActivityId;
                            WorkDefine.ActionDefinition toCreateAction = this.workFlow.Actions.FirstOrDefault(z => z.Id == asActionRef.ActionId);
                            if (null != toCreateAction)
                            {
                                toCreatedef = new WorkDefine.Activity()
                                {
                                    Action = asActionRef,
                                    Id = Guid.NewGuid().ToString(),
                                    Reactions = new List<WorkDefine.Reaction>()
                                };
                            }
                        }

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

        private IRule LoadLogic(string equationId)
        {
            StepTracer<string> trace = new StepTracer<string>();
            StepTraceNode<string> root = trace.TraceFirst("Load");

            //load conventions
            IRuleEvaluator trueEvaluator = new AlwaysTrueEvaluator();
//            this.evaluators.Add("true", trueEvaluator);
            LogicDefine.Evaluator trueDef = new LogicDefine.Evaluator() { Id = "true", Description = "Always True" };
            this.workFlow.Evaluators.Add(trueDef);

            List<string> lefts = (from e in this.workFlow.Equations
                                  where e.First != null
                                  select e.First.Id).ToList();

            List<string> rights = (from e in this.workFlow.Equations
                                   where null != e.Second
                                   select e.Second.Id).ToList();

            List<string> roots = (from e in this.workFlow.Equations
                                  where !lefts.Contains(e.Id) && !rights.Contains(e.Id)
                                  select e.Id).ToList();


            //Lint.... make sure we have everything we need first.
            Func<LogicDefine.Rule, StepTraceNode<string>, IRule> LoadRule = null;
            LoadRule = (rule, parentStep) =>
            {
                StepTraceNode<string> step = trace.TraceNext(parentStep, rule.Id);
                IRule toReturn = null;
                //if id is an equation, we are creating an expression
                LogicDefine.Equation eq = this.workFlow.Equations.FirstOrDefault(g => g.Id.Equals(rule.Id));
                if (null != eq)
                {
                    IRule first = null, second = null;
                    if (null != eq.First)
                    {
                        first = LoadRule(eq.First, step);
                    }
                    else
                    {

                        first = new Rule(
                            new LogicDefine.Rule() { Id = "true", Context = string.Empty, TrueCondition = true },
                            this);
                    }

                    if (null != eq.Second)
                    {
                        second = LoadRule(eq.Second.Id, step);
                    }
                    else
                    {

                        second = new Rule(
                            new LogicDefine.Rule() { Id = "true", Context = string.Empty, TrueCondition = true },
                            this);
                    }
                    toReturn = new Expression(rule, eq.Condition, first, second, this);

                }
                else
                {
                    LogicDefine.Evaluator ev = this.workFlow.Evaluators.FirstOrDefault(g => g.Id.Equals(rule.Id));
                    if (null != ev)
                    {
                        toReturn = new Rule(rule, this);
                    }
                }


                return toReturn;
            };




            LogicDefine.Rule eqRule = equationId;
            IRule loaded = LoadRule(eqRule, root);

            return loaded;
            
        }


        public List<LogicTest> Lint(Action<Linter> addIntents)
        {
            Linter linter = new Linter(this.workFlow.Evaluators, this.workFlow.Equations);
            addIntents(linter);


            List<LogicTest> toReturn = linter.Lint();


            return toReturn;
        }
    }
}
