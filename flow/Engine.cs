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
       
        private List<IAction> finalize = new List<IAction>();
        private List<IAction> finalizeAlways = new List<IAction>(); //actions to complete at end regardless of validation state
        private ICacheManager state;
        private IActionFactory actionFactory;
        private IRuleEvaluatorFactory ruleEvaluatorFactory;
        

        internal Engine(WorkDefine.Workflow workFlow)
        {
            this.workFlow = workFlow;
            this.Tracer = new EngineStepTracer(new ActivityProcess("CreateEngine", ActivityStatusOptions.Engine_Loading, null));
        }
        public static IEngineLoader CreateEngine(WorkDefine.Workflow workFlow)
        {
            return new Engine(workFlow);
        }


        internal EngineStepTracer Tracer { get; set; }
        internal ActivityStatusOptions CurrentActivityStatus { get; set; } = ActivityStatusOptions.Engine_Loading;

        internal LogicDefine.Rule? CurrentRuleDefinition { get; set; } = null;
        LogicDefine.Rule? IEngineScope.CurrentRuleDefinition => this.CurrentRuleDefinition;

        internal WorkDefine.Activity? CurrentActivity { get; set; }
        WorkDefine.Activity IEngineScope.CurrentActivity => this.CurrentActivity.Value;

        StepTraceNode<ActivityProcess> IEngineScope.Process => throw new NotImplementedException();

        StepTraceNode<ActivityProcess> IEngineComplete.Process => throw new NotImplementedException();

        EngineStatusOptions IEngineComplete.Status => this.engineStatus;

        ValidationContainer IEngineComplete.Validations => this.container;


        void IEngineScope.AddValidation(Validation toAdd)
        {
            //can only occure when rule is evaluating or activity is executing
            if (this.CurrentActivityStatus == ActivityStatusOptions.Action_Running)
            {
                string scope = this.CurrentActivity.Value.Id;
                this.container.Scope(scope).AddValidation(toAdd);


            }
            else if (this.CurrentActivityStatus == ActivityStatusOptions.Rule_Evaluating)
            {
                string scope = this.CurrentRuleDefinition.Value.Id;
                if (!string.IsNullOrEmpty(this.CurrentRuleDefinition.Value.Context))
                {
                    scope = string.Format("{0}.{1}", scope, this.CurrentRuleDefinition.Value.Context.GetHashCode().ToString());
                }
                this.container.Scope(scope).AddValidation(toAdd);

            }
            else
            {
                ((IValidationContainer)this.container).AddValidation(toAdd);
            }

        }

        void IEngineScope.Defer(IAction action, bool onlyIfValidationsResolved)
        {
            if (onlyIfValidationsResolved)
            {
                this.finalize.Add(action);
            } else
            {
                this.finalizeAlways.Add(action);
            }
        }

        Task<IEngineFinalize> IEngineRunner.ExecuteAsync(string activityId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        Task<IEngineComplete> IEngineRunner.ExecuteAutoFinalizeAsync(string activityId, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        Task<IEngineComplete> IEngineFinalize.FinalizeAsync(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        T IEngineScope.GetActivityModel<T>(string key)
        {
            throw new NotImplementedException();
        }

        T IEngineScope.GetModel<T>(string key)
        {
            throw new NotImplementedException();
        }

        T IEngineComplete.GetModel<T>(string key)
        {
            throw new NotImplementedException();
        }

        IEngineLoader IEngineLoader.OverrideValidations(ValidationContainer overrides)
        {
            throw new NotImplementedException();
        }

        IEngineLoader IEngineLoader.SetActionFactory(IActionFactory factory)
        {
            this.actionFactory = factory;
            return this;
        }

        T IEngineScope.SetActivityModel<T>(string key, T value)
        {
            throw new NotImplementedException();
        }

        IEngineLoader IEngineLoader.SetEvaluatorFactory(IRuleEvaluatorFactory factory)
        {
            this.ruleEvaluatorFactory = factory;
            return this;
        }

        IEngineLoader IEngineLoader.SetModel<T>(string key, T model)
        {
            throw new NotImplementedException();
        }

        T IEngineScope.SetModel<T>(string key, T value)
        {
            throw new NotImplementedException();
        }

        IEngineRunner IEngineLoader.Start()
        {
            return this;
        }


        internal bool? GetResult(LogicDefine.Rule rule)
        {
            EvaluatorKey key = new EvaluatorKey() { Id = rule.Id, Context = rule.Context };
            return this.results[key.ToString()].Value;
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
            return this.actions[actionId];
        }

        internal IRuleEvaluator GetEvaluator(string id)
        {
            return this.evaluators[id];
        }

        private Activity LoadActivity(string activityId)
        {

            WorkDefine.Activity definition = this.workFlow.Activities.FirstOrDefault(a => a.Id == activityId);

            Activity toReturn = new Activity(this, definition);

            Action<Activity, WorkDefine.Activity> LoadReactions = null;
            LoadReactions = (a, d) =>
            {
                if (d.Reactions != null && d.Reactions.Count > 0)
                {
                    d.Reactions.ForEach(r =>
                    {
                        WorkDefine.Activity toCreatedef = this.workFlow.Activities.FirstOrDefault(z => z.Id == r.ActivityId);
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

        private void LoadLogic()
        {
            StepTracer<string> trace = new StepTracer<string>();
            StepTraceNode<string> root = trace.TraceFirst("Load");

            //load conventions
            IRuleEvaluator trueEvaluator = new AlwaysTrueEvaluator();
            this.evaluators.Add("true", trueEvaluator);
            LogicDefine.Evaluator trueDef = new LogicDefine.Evaluator() { Id = "true", Description = "Always True" };
            this.workFlow.Evaluators.Add(trueDef);

            List<string> lefts = (from e in this.workFlow.Equations
                                  where !string.IsNullOrEmpty(e.First.Id)
                                  select e.First.Id).ToList();

            List<string> rights = (from e in this.workFlow.Equations
                                   where null != e.Second
                                   select e.Second.Value.Id).ToList();

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
                if (!string.IsNullOrEmpty(eq.Id))
                {
                    IRule first = null, second = null;
                    if (!string.IsNullOrEmpty(eq.First.Id))
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
                        second = LoadRule(eq.Second.Value.Id, step);
                    }
                    else
                    {

                        first = new Rule(
                            new LogicDefine.Rule() { Id = "true", Context = string.Empty, TrueCondition = true },
                            this);
                    }
                    toReturn = new Expression(rule, eq.Condition, first, second, this);

                }
                else
                {
                    LogicDefine.Evaluator ev = this.workFlow.Evaluators.FirstOrDefault(g => g.Id.Equals(rule.Id));
                    if (!string.IsNullOrEmpty(ev.Id))
                    {
                        toReturn = new Rule(rule, this);
                    }
                }


                return toReturn;
            };
            roots.ForEach(r =>
            {
                LogicDefine.Rule eqRule = new LogicDefine.Rule() { Id = r, TrueCondition = true };
                IRule loaded = LoadRule(eqRule, root);
            });
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
