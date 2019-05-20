using Mchnry.Flow.Configuration;
using Mchnry.Flow.Logic.Define;
using Mchnry.Flow.Work.Define;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mchnry.Flow.Analysis
{

    internal class Linter
    {


        private List<string> lefts = null;
        private List<string> rights = null;
        private List<string> roots = null;
        private readonly Config configuration;
        internal List<LogicTest> logicTests = default;

        internal List<ContextDefinition> ContextUsed { get; set; }


        internal Linter(WorkflowManager workflowManager, Configuration.Config configuration)
        {
            this.WorkflowManager = workflowManager;
            this.configuration = configuration;

            //infer intents
            this.lefts = (from e in this.WorkflowManager.WorkFlow.Equations
                          where e.First != null
                          select e.First.Id).ToList();

            this.rights = (from e in this.WorkflowManager.WorkFlow.Equations
                           where null != e.Second
                           select e.Second.Id).ToList();

            this.roots = (from e in this.WorkflowManager.WorkFlow.Equations
                          where !lefts.Contains(e.Id) && !rights.Contains(e.Id)
                          && e.Id != ConventionHelper.TrueEquation(this.configuration.Convention)
                          select e.Id).ToList();



            this.ExtractContextRules();

        }

        public WorkflowManager WorkflowManager { get; }

        internal void ExtractContextRules()
        {
            //all evaluators used in this workflow
            List<string> evalIds = (from e in this.WorkflowManager.WorkFlow.Evaluators select e.Id).ToList();

            //all of the rules that use the evaluators
            List<Rule> evalRules = (from l in this.WorkflowManager.WorkFlow.Equations
                                    where evalIds.Contains(l.First.Id)
                                    select l.First)
                                    .Union(from r in this.WorkflowManager.WorkFlow.Equations
                                           where null != r.Second
                                           && evalIds.Contains(r.Second.Id)
                                           select r.Second).ToList();


            //all rules that pass context
            List<string> rulesThatHaveContext = (from r in evalRules where r.Context != null select r.Id).Distinct().ToList();

            //all contexts used
            List<string> contextUsed = (from r in evalRules where r.Context != null select r.Context.Name.ToLower()).Distinct().ToList();

            //this is the inferred list
            this.ContextUsed = new List<ContextDefinition>(from c in this.WorkflowManager.WorkFlow.ContextDefinitions where contextUsed.Contains(c.Name.ToLower()) select (ContextDefinition)c.Clone());
            //clear all items, we will infer from rules
            //this is the basis for our test cases
            this.ContextUsed.ForEach(e => e.Items = new List<ContextItem>());

            //go through all of the rules, and define the contexts only for the keys used
            rulesThatHaveContext.ForEach(x =>
            {

                List<Rule> matchOnId = (from r in evalRules where r.Id.Equals(x, StringComparison.OrdinalIgnoreCase) select r).ToList();
                matchOnId.ForEach(e =>
                {
                    Context rulesContext = e.Context;
                    ContextDefinition def = ContextUsed.FirstOrDefault(c => c.Name.Equals(rulesContext.Name));
                    ContextDefinition full = this.WorkflowManager.GetContextDefinition(rulesContext.Name);

                    List<ContextItem> inferred = new List<ContextItem>(from z in rulesContext.Keys select new ContextItem() { Key = z, Literal = z });
                    inferred.ForEach(i =>
                    {
                        ContextItem defined = full.Items.FirstOrDefault(g => g.Key.Equals(i.Key, StringComparison.OrdinalIgnoreCase));
                        i.Literal = defined.Literal ?? i.Literal;
                        if (def.Items.Count(g => g.Key.Equals(i.Key, StringComparison.OrdinalIgnoreCase)) == 0)
                        {
                            def.Items.Add(i);
                        }
                    });
                });





            });
        }



        public List<LogicTest> LogicLint()
        {

            int id = 1;
            Func<int> newId = () =>
            {
                return id += 1;
            };

            List<bool> tf = new List<bool>() { true, false };

            Func<Equation, List<Rule>> getAllRulesForEquation = null;
            getAllRulesForEquation = (eq) =>
            {

                List<Rule> rules = new List<Rule>();
                //left hand side of equation

                if (ConventionHelper.MatchesConvention(NamePrefixOptions.Equation, eq.First.Id, this.configuration.Convention))
                {
                    Equation traverse = this.WorkflowManager.GetEquation(eq.First.Id);
                    List<Rule> parsed = getAllRulesForEquation(traverse);

                    rules = rules.Union(parsed, new RuleEqualityComparer()).ToList();

                }
                else
                {
                    if (!rules.Contains(eq.First, new RuleEqualityComparer()))
                    {
                        rules.Add((Rule)eq.First.Clone());
                    }
                }

                if (ConventionHelper.MatchesConvention(NamePrefixOptions.Equation, eq.Second.Id, this.configuration.Convention))
                {
                    Equation traverse = this.WorkflowManager.GetEquation(eq.Second.Id);
                    List<Rule> parsed = getAllRulesForEquation(traverse);

                    rules = rules.Union(parsed, new RuleEqualityComparer()).ToList();

                }
                else
                {
                    if (!rules.Contains(eq.Second, new RuleEqualityComparer()))
                    {
                        rules.Add((Rule)eq.Second.Clone());
                    }
                }

                string trueId = ConventionHelper.TrueEvaluator(this.configuration.Convention);
                return (from r in rules where r.Id != trueId select r).ToList();
      

            };

            Func<Stack<List<Case>>, List<Case>> mergeCases = null;
            mergeCases = (llc) =>
            {
                
                List<Case> merged = llc.Pop();
                if (llc.Count > 0)
                {
                    List<Case> toMerge = mergeCases(llc);
                    List<Case> newMerge = new List<Case>();
                    merged.ForEach(m =>
                    {
                        toMerge.ForEach(g =>
                        {
                            List<Rule> newCaseRules = new List<Rule>(from r in m.Rules select (Rule)r.Clone());
                            newCaseRules.AddRange(from r in g.Rules select (Rule)r.Clone());
                            Case newCase = new Case(newCaseRules);
                            newMerge.Add(newCase);
                        });
                    });

                    merged = newMerge;
                }

                return merged;
            };

            

            if (this.logicTests == default)
            {
                this.logicTests = new List<LogicTest>();
                //get the root
                foreach (string rootEquationId in this.roots)
                {
                    Stack<List<Case>> preMerge = new Stack<List<Case>>();
                    Equation root = this.WorkflowManager.GetEquation(rootEquationId);
                    List<Rule> evalRules = getAllRulesForEquation(root);
                    foreach (Rule evalRule in evalRules)
                    {
                        List<Case> evalRuleCases = new List<Case>();
                        if (evalRule.Context == null)
                        {

                            evalRuleCases.AddRange(from c in tf
                                                   select new Case(new List<Rule>() {
                                              new Rule()
                                              {
                                                  Context = evalRule.Context,
                                                  Id = evalRule.Id,
                                                  TrueCondition = c
                                              }
                                              }));
                        }
                        else
                        {
                            Stack<List<Case>> contextCases = new Stack<List<Case>>();
                           
                            evalRule.Context.Keys.ForEach(k =>
                            {
                                List<Case> cases = new List<Case>();
                                cases.AddRange(from c in tf
                                                       select new Case(new List<Rule>() {
                                              new Rule()
                                              {
                                                  Context = new Context(new List<string>() { k }, evalRule.Context.Name ),
                                                  Id = evalRule.Id,
                                                  TrueCondition = c
                                              }
                                              }));
                                contextCases.Push(cases);
                                evalRuleCases = mergeCases(contextCases);
                                contextCases.Push(evalRuleCases);
                                
                            });
                            var contextDef = this.WorkflowManager.GetContextDefinition(evalRule.Context.Name);
                            if (contextDef.Validate == ValidateOptions.OneOf)
                            {
                                evalRuleCases.RemoveAll(c => c.Rules.Count(r => r.TrueCondition) != 1 && c.Rules.Any(r => r.TrueCondition));
                            }
                            if (contextDef.Exclusive)
                            {
                                evalRuleCases.RemoveAll(c => c.Rules.Count(r => !r.TrueCondition) == c.Rules.Count() && c.Rules.Count == contextDef.Items.Count);
                            }

                        }

                        preMerge.Push(evalRuleCases);
                    }
                    List<Case> finalCases = mergeCases(preMerge);
                    LogicTest eqTest = new LogicTest(rootEquationId, true)
                    {
                        TestCases = finalCases
                    };
                    this.logicTests.Add(eqTest);
                }

            }

            return this.logicTests;
        }



        public List<ActivityTest> AcvityLint()
        {


            //make sure logictest was run
            var runLogicTests = this.LogicLint();


            /*for each root activity
            need to get all equations involved
            merge those test cases from top down without adding duplicates on EvaluatorID|context 
            example
             if (a & b) -> X -> 
                  if !a & c -> y
            result should be
                a, b, c (not a,!a,b,c)

            */
            List<ActivityTest> toReturn = new List<ActivityTest>();



            string rootName = this.WorkflowManager.RootActivityId;
            Activity rootActivity = this.WorkflowManager.GetActivity(rootName);


            List<string> equationsWithCases = new List<string>();
            equationsWithCases = (from z in logicTests where z.TestCases.Count > 0 select z.EquationId).ToList();
            List<List<string>> cases = new List<List<string>>();

            foreach (string eq in equationsWithCases)
            {


                LogicTest eqTest = logicTests.First(g => g.EquationId == eq);
                List<List<string>> thisRun = new List<List<string>>();
                foreach (Case testCase in eqTest.TestCases)
                {
                    List<string> ruleCases = (from g in testCase.Rules select g.ToString()).ToList();
                    if (cases.Count == 0)
                    {
                        thisRun.Add(ruleCases);
                    }
                    else
                    {
                        foreach (List<string> lastRun in cases)
                        {
                            List<string> copyOfLastRun = (from s in lastRun select s).ToList();
                            //whittle out any dupes where id and context match. 
                            List<string> justIdsFromLastRun = (from s in lastRun select ((Rule)s).RuleIdWithContext).ToList();

                            List<string> deduped = (from s in ruleCases where !justIdsFromLastRun.Contains(((Rule)s).RuleIdWithContext) select s).ToList();

                            List<string> thisCase = copyOfLastRun;

                            if (deduped.Count > 0)
                            {
                                thisCase = copyOfLastRun.Union(deduped).ToList();
                                thisRun.Add(thisCase);
                            }

                        }
                    }

                }
                if (thisRun.Count > 0)
                {
                    cases = thisRun;
                }


            }

            List<Case> testCases = (from z in cases select new Case((from x in z select (Rule)x).ToList())).ToList();

            ActivityTest toAdd = new ActivityTest(rootName) { TestCases = testCases };
            toReturn.Add(toAdd);




            int counter = 0;
            toReturn.ForEach(t =>
            {
                t.TestCases.ForEach(c => { counter++; c.Id = counter; });
            });

            return toReturn;

        }
    }


}
