namespace Mchnry.Flow.Logic
{
    using Mchnry.Core.Cache;
    using Mchnry.Flow.Diagnostics;
    using Mchnry.Flow.Logic.Define;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;

    public class RuleEngine : IRuleEngine
    {


        public RuleEngine(IRuleEvaluatorFactory factory, 
            List<Evaluator> evaluatorDefinitions,
            List<Equation> equationDefinitions)
        {
            this.Factory = factory;
            this.EvaluatorDefinitions = evaluatorDefinitions;
            this.EquationDefinitions = equationDefinitions;



        }

        private Dictionary<string, IRuleEvaluator> Evaluators { get; set; } = new Dictionary<string, IRuleEvaluator>();
        private Dictionary<string, bool?> Results { get; set; } = new Dictionary<string, bool?>();
        private ValidationContainer Validations { get; set; } = new ValidationContainer();

        //loaded during construction
        private Dictionary<string, IRule> Expressions { get; set; } = new Dictionary<string, IRule>();


        internal string CurrentProcessId { get; set; }

        /// <summary>
        /// The ID of the current evaluator in process
        /// </summary>
        string IRuleEngine.CurrentProcessId { get => this.CurrentProcessId; }

        public ICacheManager State { get; set; }
        private IRuleEvaluatorFactory Factory { get; set; }
        private List<Evaluator> EvaluatorDefinitions { get; set; }
        private List<Equation> EquationDefinitions { get; set; }

        IValidationContainer IRuleEngine.GetContainer(Define.Rule definition)
        {
            EvaluatorKey key = new EvaluatorKey() { Id = definition.Id, Context = definition.Context };
            return this.Validations.Scope(key.ToString());
        }

        IRuleEvaluator IRuleEngine.GetEvaluator(string id)
        {
            return this.Evaluators[id];
        }

        bool? IRuleEngine.GetResult(Define.Rule rule)
        {
            EvaluatorKey key = new EvaluatorKey() { Id = rule.Id, Context = rule.Context };
            return this.Results[key.ToString()].Value;
        }

        void IRuleEngine.SetResult(Define.Rule rule, bool result)
        {
            EvaluatorKey key = new EvaluatorKey() { Context = rule.Context, Id = rule.Id };
            if (this.Results.ContainsKey(key.ToString())) {
                this.Results.Remove(key.ToString());
                
            }
            this.Results.Add(key.ToString(), result);

        }

        private void Load()
        {
            StepTracer<string> trace = new StepTracer<string>();
            StepTraceNode<string> root = trace.TraceFirst("Load", string.Empty);

            //load conventions
            IRuleEvaluator trueEvaluator = new AlwaysTrueEvaluator();
            this.Evaluators.Add("true", trueEvaluator);
            Evaluator trueDef = new Evaluator() { Id = "true", Description = "Always True" };
            this.EvaluatorDefinitions.Add(trueDef);

            List<string> lefts = (from e in this.EquationDefinitions
                                  where e.First != null
                                  select e.First.Id).ToList();

            List<string> rights = (from e in this.EquationDefinitions
                                   where e.Second != null
                                   select e.Second.Id).ToList();

            List<string> roots = (from e in this.EquationDefinitions
                                  where !lefts.Contains(e.Id) && !rights.Contains(e.Id)
                                  select e.Id).ToList();


            //Lint.... make sure we have everything we need first.
            Func<Define.Rule, StepTraceNode<string>, IRule> LoadRule = null;
            LoadRule = (rule, parentStep) =>
            {
                StepTraceNode<string> step = trace.TraceNext(parentStep, rule.Id, string.Empty);
                IRule toReturn = null;
                //if id is an equation, we are creating an expression
                Define.Equation eq = this.EquationDefinitions.FirstOrDefault(g => g.Id.Equals(rule.Id));
                if (eq != null)
                {
                    IRule first = null, second = null;
                    if (eq.First != null)
                    {
                        first = LoadRule(eq.First, step);
                    } else {

                        first = new Rule(
                            new Define.Rule() { Id = "true", Context = string.Empty, TrueCondition = true }, 
                            (IRuleEngine)this);
                    }

                    if (eq.Second != null)
                    {
                        second = LoadRule(eq.Second.Id, step);
                    }
                    else
                    {

                        first = new Rule(
                            new Define.Rule() { Id = "true", Context = string.Empty, TrueCondition = true },
                            (IRuleEngine)this);
                    }
                    toReturn = new Expression(rule, eq.Condition, first, second, this);

                } else
                {
                    Define.Evaluator ev = this.EvaluatorDefinitions.FirstOrDefault(g => g.Id.Equals(rule.Id));
                    if (ev != null)
                    {
                        toReturn = new Rule(rule, this);
                    }
                }
                

                return toReturn;
            };
            roots.ForEach(r =>
            {
                Define.Rule eqRule = new Define.Rule() { Id = r, TrueCondition = true };
                IRule loaded = LoadRule(eqRule, root);
            });
        }
    }


}
