using System;
using System.Threading;
using System.Threading.Tasks;
using Mchnry.Flow.Diagnostics;
using Mchnry.Flow.Logic;
using Mchnry.Flow.Work;
using WorkDefine = Mchnry.Flow.Work.Define;
using LogicDefine = Mchnry.Flow.Logic.Define;
using Mchnry.Flow.Test;
using System.Collections.Generic;
using System.Linq;

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
        private ActivityStatusOptions currentActivityStatus = ActivityStatusOptions.Engine_Loading;
        internal Engine(WorkDefine.Workflow workFlow)
        {
            this.workFlow = workFlow;
        }
        public IEngineLoader CreateEngine(WorkDefine.Workflow workflow)
        {
            return new Engine(workFlow);
        }

        private LogicDefine.Rule? CurrentRuleDefinition { get; set; } = null;
        LogicDefine.Rule? IEngineScope.CurrentRuleDefinition => this.CurrentRuleDefinition; 

        private WorkDefine.Activity? CurrentActivity { get; set; }
        WorkDefine.Activity IEngineScope.CurrentActivity => this.CurrentActivity.Value;

        StepTraceNode<ActivityProcess> IEngineScope.Process => throw new NotImplementedException();

        StepTraceNode<ActivityProcess> IEngineComplete.Process => throw new NotImplementedException();

        EngineStatusOptions IEngineComplete.Status => this.engineStatus;

        ValidationContainer IEngineComplete.Validations => this.container;

        
        void IEngineScope.AddValidation(Validation toAdd)
        {
            //can only occure when rule is evaluating or activity is executing
            if (this.currentActivityStatus == ActivityStatusOptions.Action_Running)
            {

            }
        }

        void IEngineScope.Defer(IAction action, bool onlyIfValidationsResolved)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        T IEngineScope.SetActivityModel<T>(string key, T value)
        {
            throw new NotImplementedException();
        }

        IEngineLoader IEngineLoader.SetEvaluatorFactory(IRuleEvaluatorFactory factory)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }


        internal bool? GetResult(LogicDefine.Rule rule)
        {
            EvaluatorKey key = new EvaluatorKey() { Id = rule.Id, Context = rule.Context };
            return this.Results[key.ToString()].Value;
        }

        internal void SetResult(LogicDefine.Rule rule, bool result)
        {
            EvaluatorKey key = new EvaluatorKey() { Context = rule.Context, Id = rule.Id };
            if (this.Results.ContainsKey(key.ToString()))
            {
                this.Results.Remove(key.ToString());

            }
            this.Results.Add(key.ToString(), result);

        }

        internal IAction GetAction(string actionId)
        {
            return this.Actions[actionId];
        }

        internal IRuleEvaluator GetEvaluator(string id)
        {
            return this.Evaluators[id];
        }

        private Activity LoadActivity(string activityId)
        {

            WorkDefine.Activity definition = this.WorkFlow.Activities.FirstOrDefault(a => a.Id == activityId);

            Activity toReturn = new Activity(this, definition);

            Action<Activity, WorkDefine.Activity> LoadReactions = null;
            LoadReactions = (a, d) =>
            {
                if (d.Reactions != null && d.Reactions.Count > 0)
                {
                    d.Reactions.ForEach(r =>
                    {
                        WorkDefine.Activity toCreatedef = this.WorkFlow.Activities.FirstOrDefault(z => z.Id == r.ActivityId);
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
            this.Evaluators.Add("true", trueEvaluator);
            LogicDefine.Evaluator trueDef = new LogicDefine.Evaluator() { Id = "true", Description = "Always True" };
            this.WorkFlow.Evaluators.Add(trueDef);

            List<string> lefts = (from e in this.WorkFlow.Equations
                                  where !string.IsNullOrEmpty(e.First.Id)
                                  select e.First.Id).ToList();

            List<string> rights = (from e in this.WorkFlow.Equations
                                   where null != e.Second
                                   select e.Second.Value.Id).ToList();

            List<string> roots = (from e in this.WorkFlow.Equations
                                  where !lefts.Contains(e.Id) && !rights.Contains(e.Id)
                                  select e.Id).ToList();


            //Lint.... make sure we have everything we need first.
            Func<LogicDefine.Rule, StepTraceNode<string>, IRule> LoadRule = null;
            LoadRule = (rule, parentStep) =>
            {
                StepTraceNode<string> step = trace.TraceNext(parentStep, rule.Id);
                IRule toReturn = null;
                //if id is an equation, we are creating an expression
                LogicDefine.Equation eq = this.WorkFlow.Equations.FirstOrDefault(g => g.Id.Equals(rule.Id));
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
                    LogicDefine.Evaluator ev = this.WorkFlow.Evaluators.FirstOrDefault(g => g.Id.Equals(rule.Id));
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
            Linter linter = new Linter(this.WorkFlow.Evaluators, this.WorkFlow.Equations);
            addIntents(linter);


            List<LogicTest> toReturn = linter.Lint();


            return toReturn;
        }
    }
}
